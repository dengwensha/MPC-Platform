using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

if (args is ["self-test"])
{
    using var ticks = new Allocations();
    var keep = new List<byte[]>();
    for (int i = 0; i < 64; i++) keep.Add(new byte[128 * 1024]);
    Thread.Sleep(250);
    var selfEvents = ticks.Events.ToArray();
    GC.KeepAlive(keep);
    if (!selfEvents.Any(e => e.Type == "System.Byte[]")) throw new InvalidOperationException("Runtime allocation events unavailable; never report a successful zero profile.");
    Console.WriteLine($"PASS runtime allocation observer: {selfEvents.Length} events; byte[] observed");
    return;
}
if (args.Length != 6) throw new ArgumentException("Expected app-bin input expectedSHA canonical|inventory|diagnostic output.json label");
string app = Path.GetFullPath(args[0]), input = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[4]);
string mode = args[3];
if (mode is not ("canonical" or "inventory" or "diagnostic")) throw new ArgumentException("Unknown mode");
if (File.Exists(output)) throw new InvalidOperationException("Stale output forbidden");
if (File.Exists(Path.Combine(app, "B6DiagnosticOverlay.json")) != (mode == "diagnostic"))
    throw new InvalidOperationException("Diagnostic marker/mode mismatch; diagnostic DLL is not canonical");
string FileHash(string path) { using var file = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(file)); }
if (FileHash(input) != args[2]) throw new InvalidOperationException("Input hash mismatch");
AssemblyLoadContext.Default.Resolving += (_, n) => Assembly.LoadFrom(Path.Combine(app, n.Name + ".dll"));
var assembly = Assembly.LoadFrom(Path.Combine(app, "MPC.Application.dll"));
var connector = Assembly.LoadFrom(Path.Combine(app, "MPC.Connectors.Step.dll"));
const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
var captureMethod = assembly.GetType("MPC.Application.Inventory.InventoryEvidence+ImportSession")!.GetMethod("CaptureFromFile", flags)!;
var capture = (Func<string, Action?, object>)captureMethod.CreateDelegate(typeof(Func<string, Action?, object>));
Func<object> operation;
if (mode == "inventory")
{
    var readerType = connector.GetType("MPC.Connectors.Step.Readers.StepDataEntityReader")!;
    var validator = Activator.CreateInstance(connector.GetType("MPC.Connectors.Step.Validation.StepDataInventoryValidator")!);
    var reader = Activator.CreateInstance(readerType, validator)!;
    operation = () =>
    {
        byte[] bytes = File.ReadAllBytes(input);
        if (Convert.ToHexString(SHA256.HashData(bytes)) != args[2]) throw new InvalidOperationException("Inventory private-byte hash mismatch");
        using var stream = new MemoryStream(bytes, writable: false);
        return readerType.GetMethod("ReadCaptured")!.Invoke(reader, [stream])!;
    };
}
else operation = () => capture(input, null); // Existing production test hook stays null.

using var listener = new Allocations();
var snapshots = new List<Checkpoint>();
var monitor = new MemoryMonitor();
DateTime previousTime = DateTime.UtcNow;
long previousThreadAllocation = 0;
void Observe(string label, object? root)
{
    DateTime enter = DateTime.UtcNow;
    long allocated = GC.GetAllocatedBytesForCurrentThread();
    long liveBefore = GC.GetTotalMemory(false);
    Collect();
    long rootedHeap = GC.GetTotalMemory(false);
    var counts = new Dictionary<string, int>();
    if (root is not null)
        foreach (string name in new[] { "_rows", "_incoming", "_bad", "_reasons", "_units" })
            if (root.GetType().GetField(name, flags)?.GetValue(root) is ICollection collection) counts[name] = collection.Count;
    var checkpoint = new Checkpoint(label, previousTime, enter, default, allocated - previousThreadAllocation,
        liveBefore, rootedHeap, GC.GetGCMemoryInfo().TotalCommittedBytes, counts);
    snapshots.Add(checkpoint);
    GC.KeepAlive(root); // Defined rooted snapshot, NOT natural production liveness.
    previousTime = DateTime.UtcNow;
    snapshots[^1] = checkpoint with { ObserverEnd = previousTime };
    previousThreadAllocation = GC.GetAllocatedBytesForCurrentThread();
}
const string checkpointKey = "MPC.B6.DiagnosticOnly.Checkpoint";
if (mode == "diagnostic") AppDomain.CurrentDomain.SetData(checkpointKey, (Action<string, object?>)Observe);
Collect();
long baselineHeap = GC.GetTotalMemory(false);
previousThreadAllocation = GC.GetAllocatedBytesForCurrentThread();
previousTime = DateTime.UtcNow;
DateTime start = previousTime;
monitor.Start();
Profiled result;
try { result = Run(operation, mode, assembly, args[2]); }
finally { AppDomain.CurrentDomain.SetData(checkpointKey, null); monitor.Stop(); }
DateTime end = DateTime.UtcNow;
if (mode != "diagnostic" && snapshots.Count != 0) throw new InvalidOperationException("Canonical build has diagnostic hooks");
if (mode == "diagnostic" && snapshots.Count < 8) throw new InvalidOperationException("Diagnostic hooks absent/incomplete");
// Allow asynchronous CLR event delivery after the observed operation.
Thread.Sleep(300);
Collect();
if (result.Lifetime.IsAlive) throw new InvalidOperationException("Released result/session remains rooted");
if (FileHash(input) != args[2]) throw new InvalidOperationException("Input changed");
var events = listener.Events.Where(e => e.Time >= start && e.Time <= end).ToArray();
if (events.Length == 0 || listener.Dropped != 0) throw new InvalidOperationException("Missing/truncated allocation profile");
var segments = snapshots.Select(c => new { c.Label, From = c.WorkStart, To = c.ObserverEnter })
    .Append(new { Label = "Return", From = snapshots.Count == 0 ? start : snapshots[^1].ObserverEnd, To = end }).ToArray();
object Types(IEnumerable<Allocation> source) => source.GroupBy(e => e.Type).Select(g => new {
    Type = g.Key, Samples = g.Count(), SampledAllocationIntervalBytes = g.Sum(e => e.IntervalBytes),
    SampledTriggerObjectBytes = g.Sum(e => e.ObjectBytes)
}).OrderByDescending(g => g.SampledAllocationIntervalBytes).ThenBy(g => g.Type, StringComparer.Ordinal).Take(30).ToArray();
var report = new {
    SchemaRevision = 1, Label = args[5], Mode = mode, InputName = Path.GetFileName(input), InputSha256 = args[2],
    ApplicationAssemblySha256 = FileHash(assembly.Location), ReaderAssemblySha256 = FileHash(connector.Location),
    Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    Platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
    DiagnosticCopyOnly = mode == "diagnostic", ProfileIsEligibilityEvidence = false, ConsumerInvoked = false,
    BaselineRootedHeapBytes = baselineHeap, ResultRootedHeapBytes = result.RootedHeap,
    ApproximateRootedHeapDeltaBytes = result.RootedHeap - baselineHeap, ReleasedObjectCollected = true,
    TotalSampledEvents = events.Length, DroppedEvents = listener.Dropped, AllocationTypes = Types(events),
    Checkpoints = snapshots, Segments = segments.Select(s => new {
        s.Label, SampleCount = events.Count(e => e.Time >= s.From && e.Time < s.To),
        AllocationTypes = Types(events.Where(e => e.Time >= s.From && e.Time < s.To)),
        MemorySamples = monitor.Samples.Count(m => m.Time >= s.From && m.Time < s.To),
        ObservedMaxHeapBytes = monitor.Samples.Where(m => m.Time >= s.From && m.Time < s.To).Select(m => (long?)m.Heap).Max(),
        ObservedMaxWorkingSetBytes = monitor.Samples.Where(m => m.Time >= s.From && m.Time < s.To).Select(m => (long?)m.WorkingSet).Max()
    }).ToArray(),
    MemorySamples = monitor.Samples.ToArray(), SemanticSummary = result.Summary,
    SemanticSha256 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(result.Summary))),
    Caveats = new[] {
        "CLR allocation ticks are sampled. Interval bytes weighted by triggering type are estimates, NOT exact per-type allocation/live sizes.",
        "Observer allocations/sampling overhead included; no latency or budget acceptance from these runs.",
        "Diagnostic copy checkpoints force GC and root Check explicitly; these are controlled reachable-heap observations, NOT natural production peaks or exclusive object graph sizes.",
        "5ms working-set/heap samples can miss peaks. GC committed bytes, live managed heap and resident process memory are different metrics.",
        "Types/counts/IDs/hashes only. No raw CAD, private values, object addresses or heap dumps emitted.",
        "Diagnostic copy output is never accepted as product evidence or given to a consumer. Canonical/diagnostic projected semantics compared separately."
    }
};
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"PASS {mode}: {Path.GetFileName(input)}; events={events.Length}; checkpoints={snapshots.Count}; released=true");

static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
[MethodImpl(MethodImplOptions.NoInlining)]
static Profiled Run(Func<object> operation, string mode, Assembly assembly, string expectedSha256)
{
    const BindingFlags f = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    static object? Get(object? x, string name) => x?.GetType().GetProperty(name, f)?.GetValue(x);
    object value = operation();
    object? session = mode == "inventory" ? null : Get(value, "Session");
    if (mode != "inventory" && session is null) throw new InvalidOperationException("Capture unavailable, not a zero-memory success");
    if (session is not null && Get(session, "DocumentSha256")?.ToString() != expectedSha256)
        throw new InvalidOperationException("Actual captured-byte identity mismatch");
    Collect();
    long heap = GC.GetTotalMemory(false);
    object? evidence = Get(session, "Evidence");
    object? target = session?.GetType().GetMethod("ReadTarget", f)?.Invoke(session, [evidence]);
    object? decision = mode == "inventory" ? null : assembly.GetType("MPC.Application.Inventory.TargetDecision+TargetEvidenceGate")!
        .GetMethod("Evaluate", f)!.Invoke(null, [evidence, session]);
    object? inventory = mode == "inventory" ? Get(value, "Value") : null;
    if (mode == "inventory" && inventory is null) throw new InvalidOperationException("Inventory unavailable");
    object summary = mode == "inventory" ? new {
        RecordCount = Get(inventory, "TotalEntityCount"), Status = Get(value, "Status")?.ToString()
    } : new {
        DocumentSha256 = Get(session, "DocumentSha256"), RecordCount = Get(session, "RetainedEntityCount"),
        ReadStatus = Get(evidence, "ReadStatus")?.ToString(), RootId = Get(target, "RootId"),
        RepresentationId = Get(target, "RepresentationId"), MainRepresentationId = Get(target, "MainRepresentationId"),
        ContextId = Get(target, "ContextId"), Binding = Get(target, "Binding")?.ToString(),
        Compatibility = Get(target, "Compatibility")?.ToString(), GuardCalled = Get(target, "GuardCalled"),
        Status = Get(decision, "Status")?.ToString(),
        Reasons = ((IEnumerable)Get(decision, "Reasons")!).Cast<object>().Select(r => new {
            Layer = Get(r, "Layer"), Code = Get(r, "Code"), Severity = Get(r, "Severity"),
            SourceId = Get(r, "SourceId"), Component = Get(r, "Component"), Indices = Get(r, "Indices"),
            RelatedIds = Get(r, "RelatedIds"), Occurrence = Get(r, "Occurrence"), OriginalMessage = Get(r, "OriginalMessage"),
            CauseType = Get(r, "CauseType"), CauseMessageSha256 = Get(r, "CauseMessage") is string message
                ? Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(message))) : null
        }).ToArray()
    };
    GC.KeepAlive(value);
    return new(heap, summary, new WeakReference(session ?? value));
}
sealed record Profiled(long RootedHeap, object Summary, WeakReference Lifetime);
sealed record Checkpoint(string Label, DateTime WorkStart, DateTime ObserverEnter, DateTime ObserverEnd,
    long ThreadAllocatedBytes, long HeapBeforeForcedGC, long RootedHeapAfterGC, long GCCommittedBytes, Dictionary<string, int> Counts);
sealed record MemorySample(DateTime Time, long Heap, long WorkingSet, long Committed);
sealed class MemoryMonitor
{
    private readonly Thread _thread;
    private volatile bool _stop;
    internal readonly ConcurrentQueue<MemorySample> Samples = new();
    internal MemoryMonitor() { _thread = new Thread(Watch) { IsBackground = true }; }
    internal void Start() => _thread.Start();
    internal void Stop() { _stop = true; _thread.Join(); }
    private void Watch()
    {
        using var process = Process.GetCurrentProcess();
        while (!_stop)
        {
            process.Refresh();
            Samples.Enqueue(new(DateTime.UtcNow, GC.GetTotalMemory(false), process.WorkingSet64, GC.GetGCMemoryInfo().TotalCommittedBytes));
            if (Samples.Count > 100000) throw new InvalidOperationException("Memory profile sample cap exceeded");
            Thread.Sleep(5);
        }
    }
}

sealed record Allocation(DateTime Time, string Type, long IntervalBytes, long ObjectBytes);
sealed class Allocations : EventListener
{
    private bool _ready;
    internal readonly ConcurrentQueue<Allocation> Events = new();
    internal int Dropped;
    internal Allocations()
    {
        _ready = true;
        foreach (var source in EventSource.GetSources()) OnEventSourceCreated(source);
    }
    protected override void OnEventSourceCreated(EventSource source)
    {
        if (_ready && source.Name == "Microsoft-Windows-DotNETRuntime")
            EnableEvents(source, EventLevel.Verbose, (EventKeywords)1);
    }
    protected override void OnEventWritten(EventWrittenEventArgs e)
    {
        if (e.EventName?.StartsWith("GCAllocationTick", StringComparison.Ordinal) != true) return;
        if (Events.Count >= 100000) { Interlocked.Increment(ref Dropped); return; }
        object? Value(string name)
        {
            int index = e.PayloadNames?.IndexOf(name) ?? -1;
            return index < 0 ? null : e.Payload?[index];
        }
        Events.Enqueue(new(e.TimeStamp.ToUniversalTime(), Value("TypeName")?.ToString() ?? "Unknown",
            Convert.ToInt64(Value("AllocationAmount64") ?? Value("AllocationAmount") ?? 0),
            Convert.ToInt64(Value("ObjectSize") ?? 0)));
    }
}
