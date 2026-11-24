using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

// Tooling-only benchmark of real methods. No instrumentation/seam/friend access
// is added to Application. A neutral target stage never issues bound evidence.
if (args.Length != 6) throw new ArgumentException("Expected Application bin, input, expected SHA256 or MISSING, capture|target, output.json, label.");
string app = Path.GetFullPath(args[0]), input = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[4]);
if (File.Exists(output)) throw new InvalidOperationException("Stale benchmark output is forbidden.");
AssemblyLoadContext.Default.Resolving += (_, n) => Assembly.LoadFrom(Path.Combine(app,n.Name+".dll"));
var assembly = Assembly.LoadFrom(Path.Combine(app,"MPC.Application.dll"));
var connector = Assembly.LoadFrom(Path.Combine(app,"MPC.Connectors.Step.dll"));
bool missing = args[2] == "MISSING";
string? expected = missing ? null : args[2];
if (missing ? File.Exists(input) : FileHash(input) != expected) throw new InvalidOperationException("Benchmark input identity mismatch.");
if (args[3] is not ("capture" or "target")) throw new ArgumentException("Unknown benchmark unit.");
var sessionType = assembly.GetType("MPC.Application.Inventory.InventoryEvidence+ImportSession")!;
var captureMethod = sessionType.GetMethod("CaptureFromFile",Access.Flags)!;
var capture = (Func<string,Action?,object>)captureMethod.CreateDelegate(typeof(Func<string,Action?,object>));
var producer = assembly.GetType("MPC.Application.Inventory.TargetProducer");
object? retainedInventory = null;
Func<object> operation;
string? setupFailure = null;
if (args[3] == "capture") operation = () => capture(input,null);
else
{
    if (producer is null) throw new InvalidOperationException("Target stage absent from baseline; not a zero-cost observation.");
    if (missing) throw new InvalidOperationException("Target-only stage needs a real captured inventory.");
    var readerType = connector.GetType("MPC.Connectors.Step.Readers.StepDataEntityReader")!;
    var reader = Activator.CreateInstance(readerType,Activator.CreateInstance(connector.GetType("MPC.Connectors.Step.Validation.StepDataInventoryValidator")!))!;
    // This setup read is OUTSIDE the target-only timer. Hash the very same bytes.
    byte[] bytes = File.ReadAllBytes(input);
    if (Hash(bytes) != expected) throw new InvalidOperationException("Setup capture identity mismatch.");
    using var stream = new MemoryStream(bytes,writable:false);
    object read = readerType.GetMethod("ReadCaptured")!.Invoke(reader,[stream])!;
    retainedInventory = Access.P(read,"Value");
    if(retainedInventory is null) setupFailure="InventoryUnavailable";
    var method=producer.GetMethod("Read",Access.Flags)!;
    // Strong concrete parameter conversion is bound once, outside the timer.
    operation = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Call(method,
        Expression.Constant(retainedInventory,method.GetParameters()[0].ParameterType)),typeof(object))).Compile();
}

var context = new ProbeContext(assembly,args[3],expected,missing);
var samples = new List<Sample>(); var released = new List<bool>();
object? summary = null; string? fingerprint = null;
if(setupFailure is null)
{
    for(int i=0;i<6;i++)
    {
        Collect();
        var measured=Measure(operation,context,i==0?"ProcessFirstInvocation":i<=2?"Warmup":"WarmAfterExplicitGC");
        // Validation/summary hashing is after production timing/allocation sampling.
        if(fingerprint is null){fingerprint=measured.Fingerprint;summary=measured.Summary;}
        else if(fingerprint!=measured.Fingerprint) throw new InvalidOperationException("Benchmark semantics changed between iterations.");
        Collect();released.Add(!measured.Lifetime.IsAlive);
        if(i==0||i>=3)samples.Add(measured.Sample);
    }
}
if(released.Any(r=>!r))throw new InvalidOperationException("Released capture/session remained rooted by the harness or production code.");
if(missing?File.Exists(input):FileHash(input)!=expected)throw new InvalidOperationException("Input changed during measurement.");
var report=new {
    SchemaRevision=1,Label=args[5],InputName=Path.GetFileName(input),InputSha256=expected,InputBytes=missing?(long?)null:new FileInfo(input).Length,
    Unit=args[3]=="capture"?"ProductionCaptureFromFile":"IsolatedProductionTargetDataFromPreparedInventory",
    StageAvailable=setupFailure is null,SetupFailure=setupFailure,Framework=RuntimeInformation.FrameworkDescription,
    OS=RuntimeInformation.OSDescription,Architecture=RuntimeInformation.ProcessArchitecture.ToString(),
    ApplicationAssemblySha256=FileHash(assembly.Location),ReaderAssemblySha256=FileHash(connector.Location),
    ApplicationInformationalVersion=assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
    Measurements=samples,SemanticSummary=summary,SummarySha256=fingerprint,ReleasedObjects= released.Count,
    AllReleasedObjectsCollected=released.All(x=>x),WarmupCount=setupFailure is null?2:0,
    NoIndependentPrecheckOrJsonInsideTimedRegion=true,
    Note="First production invocation after assembly/delegate binding and input preflight; NOT cold OS cache or process startup. Explicit full GC before calls; no forced GC inside timing. Peak/working set include process setup, prepared inventory (target-only), warmups and preceding probes; peak is not attributable incremental stage memory. Retained heap is after forced GC with result kept alive, before summary projection."
};
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
File.WriteAllText(output,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
GC.KeepAlive(retainedInventory);
Console.WriteLine($"B6 {args[5]} {Path.GetFileName(input)} {args[3]} available={report.StageAvailable} samples={samples.Count}");

static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));
static string FileHash(string path){using var file=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(file));}
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}

[MethodImpl(MethodImplOptions.NoInlining)]
static Measured Measure(Func<object> operation,ProbeContext context,string kind)
{
    using var process=Process.GetCurrentProcess();process.Refresh();
    long workingBefore=process.WorkingSet64,peakBefore=process.PeakWorkingSet64,heapBefore=GC.GetTotalMemory(false);
    long allocatedBefore=GC.GetTotalAllocatedBytes(precise:true);
    int g0=GC.CollectionCount(0),g1=GC.CollectionCount(1),g2=GC.CollectionCount(2);
    long start=Stopwatch.GetTimestamp();
    object result=operation();
    long end=Stopwatch.GetTimestamp();
    long allocated=GC.GetTotalAllocatedBytes(precise:true)-allocatedBefore;
    int dg0=GC.CollectionCount(0)-g0,dg1=GC.CollectionCount(1)-g1,dg2=GC.CollectionCount(2)-g2;
    long liveHeap=GC.GetTotalMemory(false);process.Refresh();
    long workingAfter=process.WorkingSet64,peakAfter=process.PeakWorkingSet64;
    Collect();long retained=GC.GetTotalMemory(false);
    // Keep result strongly alive through retention sampling; release on method exit.
    object? lifetime=context.Unit=="capture"?Access.P(result,"Session"):result;
    object projection=context.Project(result);
    string hash=Hash(JsonSerializer.SerializeToUtf8Bytes(projection));
    GC.KeepAlive(result);
    return new(new(kind,Stopwatch.GetElapsedTime(start,end).TotalMilliseconds,allocated,heapBefore,liveHeap,retained,
        retained-heapBefore,workingBefore,workingAfter,peakBefore,peakAfter,dg0,dg1,dg2),projection,hash,new WeakReference(lifetime??result));
}

sealed record Sample(string Kind,double ElapsedMilliseconds,long AllocatedBytes,long HeapBeforeBytes,long LiveHeapAfterCallBytes,
    long RetainedHeapAfterForcedGcBytes,long RetainedHeapDeltaBytes,long WorkingSetBeforeBytes,long WorkingSetAfterBytes,
    long LifetimePeakBeforeBytes,long LifetimePeakAfterCallBytes,int Gen0Collections,int Gen1Collections,int Gen2Collections);
sealed record Measured(Sample Sample,object Summary,string Fingerprint,WeakReference Lifetime);
static class Access
{
    internal const BindingFlags Flags=BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    internal static object? P(object o,string name)=>o.GetType().GetProperty(name,Flags)!.GetValue(o);
}
sealed class ProbeContext(Assembly assembly,string unit,string? expected,bool missing)
{
    internal string Unit=>unit;
    internal object Project(object result)
    {
        object? session=unit=="capture"?Access.P(result,"Session"):null;
        object? evidence=session is null?null:Access.P(session,"Evidence");
        object? observation=unit=="target"?result:session?.GetType().GetMethod("ReadTarget",Access.Flags)?.Invoke(session,[evidence]);
        object? target=observation is null?null:new {
            RootId=Access.P(observation,"RootId"),RepresentationId=Access.P(observation,"RepresentationId"),
            MainRepresentationId=Access.P(observation,"MainRepresentationId"),ContextId=Access.P(observation,"ContextId"),
            RecordCount=Access.P(observation,"RecordCount"),Binding=Access.P(observation,"Binding")!.ToString(),
            Compatibility=Access.P(observation,"Compatibility")!.ToString(),GuardCalled=Access.P(observation,"GuardCalled"),
            Reasons=((IEnumerable)Access.P(observation,"Reasons")!).Cast<object>().Select(r=>new {
                Layer=Access.P(r,"Layer"),Code=Access.P(r,"Code"),SourceId=Access.P(r,"SourceId"),Component=Access.P(r,"Component"),
                Indices=Access.P(r,"Indices"),RelatedIds=Access.P(r,"RelatedIds"),Occurrence=Access.P(r,"Occurrence"),
                OriginalMessage=Access.P(r,"OriginalMessage"),CauseType=Access.P(r,"CauseType")}).ToArray()};
        string? targetStatus=null;
        if(evidence is not null&&observation is not null)
        {
            var gate=assembly.GetType("MPC.Application.Inventory.TargetDecision+TargetEvidenceGate")!;
            var decision=gate.GetMethod("Evaluate",Access.Flags)!.Invoke(null,[evidence,session])!;
            targetStatus=Access.P(decision,"Status")!.ToString();
        }
        object? common=null;
        if(unit=="capture")
        {
            if(evidence is not null && Access.P(session!,"DocumentSha256")?.ToString()!=expected)
                throw new InvalidOperationException("Real capture hash mismatch.");
            if(missing&&session is not null)throw new InvalidOperationException("Missing input became an observation.");
            common=new {
                ObservationAvailable=session is not null,DocumentSha256=session is null?null:Access.P(session,"DocumentSha256"),
                ReadStatus=evidence is null?null:Access.P(evidence,"ReadStatus")!.ToString(),
                HasInventory=evidence is null?null:Access.P(evidence,"HasInventory"),
                RecordCount=session is null?null:Access.P(session,"RetainedEntityCount"),
                FailureStage=Access.P(result,"FailureStage")?.ToString(),FailureType=Access.P(result,"Failure")?.GetType().FullName,
                Diagnostics=evidence is null?null:((IEnumerable)Access.P(evidence,"Diagnostics")!).Cast<object>().Select(d=>new {
                    Code=Access.P(d,"Code"),Severity=Access.P(d,"Severity")!.ToString(),SourceEntityId=Access.P(d,"SourceEntityId")}).ToArray()};
        }
        return new {Common=common,TargetData=target,TargetStatus=targetStatus,
            Authority=unit=="capture"?"Real owned capture; target gate sampled AFTER timing if present":"Neutral target data only; no bound authority issued"};
    }
}
