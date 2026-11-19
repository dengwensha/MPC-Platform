using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

// Measurement only. This JSON is not Application eligibility evidence or a wire result.
if (args.Length != 4 || args[1].Length != 64 || !args[1].All(Uri.IsHexDigit)
    || args[2].Length != 40 || !args[2].All(Uri.IsHexDigit))
{
    Console.Error.WriteLine("Usage: M2InventoryProbe <input.step> <expected-sha256> <source-commit> <output.json>");
    return 2;
}
string input = Path.GetFullPath(args[0]);
string output = Path.GetFullPath(args[3]);
if (string.Equals(input, output, StringComparison.OrdinalIgnoreCase)) return 2;
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
// A failed observation must not leave an old successful profile at this output path.
File.Delete(output);
var assembly = typeof(StepDataEntityReader).Assembly;
var row = new Dictionary<string, object?>
{
    ["SchemaVersion"] = 1,
    ["Scope"] = "InventoryObservationOnly",
    ["InputName"] = Path.GetFileName(input),
    ["ExpectedSha256"] = args[1].ToUpperInvariant(),
    ["ActualSha256"] = null,
    ["SuppliedSourceCommit"] = args[2],
    ["ReaderAssemblyVersion"] = assembly.GetName().Version?.ToString(),
    ["ReaderInformationalVersion"] = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
    ["ReaderAssemblySha256"] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
    ["InputHashVerified"] = false,
    ["ObservationAvailable"] = false,
    ["ReadFailed"] = true,
    ["InventoryStatus"] = null,
    ["EntityCount"] = null,
    ["RecordStatusCounts"] = null,
    ["DiagnosticCount"] = null,
    ["ErrorCount"] = null,
    ["FatalCount"] = null,
    ["DiagnosticGroups"] = null,
    ["ExceptionType"] = null
};
int exit = 0;
try
{
    // Capture once and hash exactly the bytes read by the production inventory reader.
    byte[] bytes = File.ReadAllBytes(input);
    string hash = Convert.ToHexString(SHA256.HashData(bytes));
    row["ActualSha256"] = hash;
    if (!string.Equals(hash, args[1], StringComparison.OrdinalIgnoreCase))
    {
        row["ExceptionType"] = "InputHashMismatch";
        exit = 2;
    }
    else
    {
        row["InputHashVerified"] = true;
        // Same private byte array as the verified hash; no temp file/reopen/cleanup.
        using var capture = new MemoryStream(bytes, writable: false);
        var result = new StepDataEntityReader(new StepDataInventoryValidator()).ReadCaptured(capture);
        row["ObservationAvailable"] = true;
        row["ReadFailed"] = result.Status == CadConnectorResultStatus.Failed;
        row["InventoryStatus"] = result.Status.ToString();
        row["EntityCount"] = result.Value?.TotalEntityCount;
        row["RecordStatusCounts"] = result.Value?.Entities.GroupBy(x => x.ReadStatus)
            .OrderBy(x => x.Key).Select(x => new { Status = x.Key.ToString(), Count = x.Count() }).ToArray();
        // Result diagnostics already include per-record and inventory-validator diagnostics.
        // Do not count record diagnostics a second time.
        row["DiagnosticCount"] = result.Diagnostics.Count;
        row["ErrorCount"] = result.Diagnostics.Count(x => x.Severity == CadDiagnosticSeverity.Error);
        row["FatalCount"] = result.Diagnostics.Count(x => x.Severity == CadDiagnosticSeverity.Fatal);
        row["DiagnosticGroups"] = result.Diagnostics.GroupBy(x => (x.Code, x.Severity))
            .OrderBy(x => x.Key.Code, StringComparer.Ordinal).ThenBy(x => x.Key.Severity)
            .Select(x => new { x.Key.Code, Severity = x.Key.Severity.ToString(), Count = x.Count(),
                SourceEntityCount = x.Where(d => d.SourceEntityId is not null)
                    .Select(d => d.SourceEntityId).Distinct(StringComparer.Ordinal).Count() }).ToArray();
    }
}
catch (Exception exception)
{
    // A probe failure is unavailable evidence, never a clean file or production input classification.
    row["ObservationAvailable"] = false;
    row["ReadFailed"] = true;
    foreach (string key in new[] { "InventoryStatus", "EntityCount", "RecordStatusCounts",
        "DiagnosticCount", "ErrorCount", "FatalCount", "DiagnosticGroups" })
        row[key] = null;
    row["ExceptionType"] = exception.GetType().Name;
    exit = 1;
}
File.WriteAllText(output, JsonSerializer.Serialize(row, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Inventory observation {Path.GetFileName(input)}: available={row["ObservationAvailable"]}; entities={row["EntityCount"]}; readFailed={row["ReadFailed"]}; errors={row["ErrorCount"]}; fatals={row["FatalCount"]}");
return exit;
