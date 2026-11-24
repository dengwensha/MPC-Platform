using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

// Read-only audit adapter to the real internal capture/gate. No friend access or
// producer seam is opened. Reflection is tooling-only and source-pinned in evidence.
if (args.Length != 3) throw new ArgumentException("Expected Application bin, input directory, output directory.");
string app = Path.GetFullPath(args[0]);
AssemblyLoadContext.Default.Resolving += (_, name) => Assembly.LoadFrom(Path.Combine(app, name.Name + ".dll"));
var assembly = Assembly.LoadFrom(Path.Combine(app, "MPC.Application.dll"));
var connector = Assembly.LoadFrom(Path.Combine(app,"MPC.Connectors.Step.dll"));
const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
object? P(object instance, string name) => instance.GetType().GetProperty(name, flags)!.GetValue(instance);
object Call(Type type, string name, object? instance, params object?[] values) => type.GetMethod(name, flags)!.Invoke(instance, values)!;
string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data));
var sessionType = assembly.GetType("MPC.Application.Inventory.InventoryEvidence+ImportSession")!;
var gate = assembly.GetType("MPC.Application.Inventory.TargetDecision+TargetEvidenceGate")!;
Directory.CreateDirectory(args[2]);
if(Directory.EnumerateFiles(args[2],"*.json").Any()) throw new InvalidOperationException("Measurement output must be empty; stale observations are forbidden.");
foreach (string input in Directory.GetFiles(args[1]).Where(p => Path.GetExtension(p).Equals(".step", StringComparison.OrdinalIgnoreCase)
    || Path.GetExtension(p).Equals(".stp", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal))
{
    string hash = Hash(File.ReadAllBytes(input));
    object capture = Call(sessionType, "CaptureFromFile", null, input, null);
    object? session = P(capture, "Session");
    if (session is null) throw new InvalidOperationException("Capture unavailable: " + Path.GetFileName(input) + "; " + P(capture, "FailureStage"));
    object evidence = P(session, "Evidence")!;
    object observation = Call(sessionType, "ReadTarget", session, evidence);
    object decision = Call(gate, "Evaluate", null, evidence, session);
    // Independent measurement read, NOT production evidence or an input to the gate.
    // The producer's A1 call above still uses its original retained inventory.
    var readerType=connector.GetType("MPC.Connectors.Step.Readers.StepDataEntityReader")!;
    var reader=Activator.CreateInstance(readerType,Activator.CreateInstance(connector.GetType("MPC.Connectors.Step.Validation.StepDataInventoryValidator")!))!;
    byte[] independentBytes=File.ReadAllBytes(input);
    string independentHash=Hash(independentBytes);
    if(independentHash!=hash)throw new InvalidOperationException("Independent comparison capture changed.");
    using var independentCapture=new MemoryStream(independentBytes,writable:false);
    var read=Call(readerType,"ReadCaptured",reader,independentCapture);var inventory=P(read,"Value");
    var roots=inventory is null?[]:((IEnumerable)P(inventory,"Entities")!).Cast<object>()
        .Where(r=>P(r,"SourceEntityType")?.ToString()=="MANIFOLD_SOLID_BREP").Select(r=>P(r,"SourceEntityId")?.ToString()).ToArray();
    string precheck=inventory is null?"UnavailableInventory":roots.Length!=1?"UnavailableRootCount":"Passed";
    string? precheckReason=null;
    if(precheck=="Passed")
    {
        try{Call(assembly.GetType("MPC.Application.Step.ImportUnitPrecheck")!,"EnsureCompatible",null,inventory,roots[0]);}
        catch(TargetInvocationException ex) when(ex.InnerException is InvalidDataException failure)
        {precheck="Rejected";precheckReason=failure.Message;}
    }
    var reasons = ((IEnumerable)P(decision, "Reasons")!).Cast<object>().Select(r => new
    {
        Layer = P(r, "Layer"), Code = P(r, "Code"), Severity = P(r, "Severity"), SourceId = P(r, "SourceId"),
        Component = P(r, "Component"), Indices = P(r, "Indices"), RelatedIds = P(r, "RelatedIds"),
        Occurrence = P(r, "Occurrence"), OriginalMessage = P(r, "OriginalMessage"), CauseType = P(r, "CauseType")
        // Inner cause text may contain private STEP tokens; retain only in live evidence.
    }).ToArray();
    var row = new
    {
        InputName = Path.GetFileName(input), InputSha256 = hash,
        CapturedSha256 = P(session, "DocumentSha256"), InputUnchanged = hash == Hash(File.ReadAllBytes(input)),
        IndependentCapturedSha256 = independentHash,
        ApplicationAssemblySha256 = Hash(File.ReadAllBytes(assembly.Location)),
        ReaderAssemblySha256 = Hash(File.ReadAllBytes(Path.Combine(app, "MPC.Connectors.Step.dll"))),
        ManifestRevision = P(observation, "ManifestRevision"), BindingRevision = P(observation, "BindingRevision"),
        CompatibilityRevision = P(observation, "CompatibilityRevision"),
        RootId = P(observation, "RootId"), RepresentationId = P(observation, "RepresentationId"),
        MainRepresentationId = P(observation, "MainRepresentationId"), ContextId = P(observation, "ContextId"),
        RecordCount = P(observation, "RecordCount"), GuardCalled = P(observation, "GuardCalled"),
        Binding = P(observation, "Binding")!.ToString(), Compatibility = P(observation, "Compatibility")!.ToString(),
        TargetStatus = P(decision, "Status")!.ToString(), Reasons = reasons,
        IndependentPrecheck = precheck, IndependentPrecheckReason = precheckReason,
        PhysicalOccurrenceProofIssued = false, AngleAbsenceProofIssued = false, GeometryValidityProofIssued = false, ReadyIssued = false
    };
    if (row.InputSha256 != row.CapturedSha256?.ToString() || !row.InputUnchanged) throw new InvalidOperationException("Measurement identity changed.");
    File.WriteAllText(Path.Combine(args[2], Path.GetFileName(input) + ".json"), JsonSerializer.Serialize(row, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"TARGET {row.InputName}: {row.TargetStatus}, guard={row.GuardCalled}, reasons={reasons.Length}");
}
