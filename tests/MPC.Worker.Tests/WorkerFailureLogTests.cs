using System.Text.Json;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Worker.Tests;

public sealed class WorkerFailureLogTests : IDisposable
{
    private const string Id = "5cd392e2-d34a-4789-8bbc-fae88a802fed";
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "MpcFailureLog", Guid.NewGuid().ToString("N"));
    private string LogPath => Path.Combine(_folder, "worker-last-failure.json");

    [Fact]
    public void Record_PreservesCorrelationUtcAndInnerCause()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        var error = new InvalidDataException("Rejected STEP", new FormatException("Radius must be numeric."));
        new WorkerFailureLog(_folder).TryWrite(Id, error);
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        var root = json.RootElement;
        Assert.Equal(Id, root.GetProperty("RequestId").GetString());
        Assert.Equal(typeof(InvalidDataException).FullName, root.GetProperty("ExceptionType").GetString());
        Assert.Contains("System.FormatException", root.GetProperty("ExceptionDetails").GetString());
        Assert.Contains("Radius must be numeric.", root.GetProperty("ExceptionDetails").GetString());
        var timestamp = root.GetProperty("TimestampUtc").GetDateTimeOffset();
        Assert.Equal(TimeSpan.Zero, timestamp.Offset);
        Assert.InRange(timestamp, before, DateTimeOffset.UtcNow);
        Assert.False(root.GetProperty("DetailsTruncated").GetBoolean());
    }

    [Fact]
    public void RepeatedFailures_ReplaceOneBoundedRecord()
    {
        var log = new WorkerFailureLog(_folder);
        log.TryWrite(Id, new IOException("old"));
        log.TryWrite(Id, new IOException(new string('X', 100_000)));
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        Assert.True(json.RootElement.GetProperty("DetailsTruncated").GetBoolean());
        Assert.True(new FileInfo(LogPath).Length < 64 * 1024);
        Assert.Single(Directory.GetFiles(_folder));
    }

    [Fact]
    public void RealImportFailure_RecordsCauseWithoutChangingResult()
    {
        Directory.CreateDirectory(_folder);
        var processor = new RegionSourceValidationProcessor(new WorkerFailureLog(_folder));
        var result = processor.Process(Request());
        AssertFailure(result);
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        Assert.Equal(Id, json.RootElement.GetProperty("RequestId").GetString());
        Assert.Equal(typeof(FileNotFoundException).FullName,
            json.RootElement.GetProperty("ExceptionType").GetString());
    }

    [Theory]
    [InlineData("-178.5", "StepSemanticValueException")]
    [InlineData(".T.", "System.FormatException")]
    public void RejectedStep_RecordsRealInnerCause(string radius, string cause)
    {
        Directory.CreateDirectory(_folder);
        string golden = Path.Combine(AppContext.BaseDirectory, "TestData", "Step", "Golden", "T001_PRISM.step");
        string source = File.ReadAllText(golden);
        int dataEnd = source.LastIndexOf("ENDSEC;", StringComparison.Ordinal);
        Assert.True(dataEnd >= 0);
        string input = Path.Combine(_folder, "rejected.step");
        File.WriteAllText(input, source.Insert(dataEnd,
            "#999999 = TOROIDAL_SURFACE ('test', #175, " + radius + ", 10.);\n"));
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(input)));
        var request = new RegionSourceValidationRequest("1.1", Id,
            WorkerOperationKind.ValidateRegionSource, input, "#8", hash,
            RegionSourceValidationRequirement.SourceLinkOnly);
        AssertFailure(new RegionSourceValidationProcessor(new WorkerFailureLog(_folder)).Process(request));
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        Assert.Equal(Id, json.RootElement.GetProperty("RequestId").GetString());
        Assert.Contains(cause, json.RootElement.GetProperty("ExceptionDetails").GetString());
    }

    [Fact]
    public void UnwritableDestination_DoesNotReplaceImportFailure()
    {
        Directory.CreateDirectory(_folder);
        string blocked = Path.Combine(_folder, "blocked");
        File.WriteAllText(blocked, "A file cannot serve as the log directory.");
        var result = new RegionSourceValidationProcessor(new WorkerFailureLog(blocked)).Process(Request());
        AssertFailure(result);
        Assert.Equal("A file cannot serve as the log directory.", File.ReadAllText(blocked));
    }

    [Fact]
    public void InvalidRequest_DoesNotCreateDiagnosticDirectory()
    {
        var result = new RegionSourceValidationProcessor(new WorkerFailureLog(_folder)).Process(null);
        Assert.Equal(RegionSourceValidationOutcome.RequestRejected, result.Outcome);
        Assert.False(Directory.Exists(_folder));
    }

    [Fact]
    public void GoldenSuccess_DoesNotCreateDiagnosticDirectory()
    {
        string golden = Path.Combine(AppContext.BaseDirectory, "TestData", "Step", "Golden", "T001_PRISM.step");
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(golden)));
        var request = new RegionSourceValidationRequest("1.1", Id,
            WorkerOperationKind.ValidateRegionSource, golden, "#8", hash,
            RegionSourceValidationRequirement.SourceLinkOnly);
        var result = new RegionSourceValidationProcessor(new WorkerFailureLog(_folder)).Process(request);
        Assert.Equal(RegionSourceValidationOutcome.SourceLinkMatched, result.Outcome);
        Assert.False(Directory.Exists(_folder));
    }

    private RegionSourceValidationRequest Request() => new("1.1", Id,
        WorkerOperationKind.ValidateRegionSource,
        Path.Combine(_folder, "missing.step"), "#8", new string('A', 64),
        RegionSourceValidationRequirement.SourceLinkOnly);

    private static void AssertFailure(RegionSourceValidationResult result)
    {
        Assert.Equal(RegionSourceValidationOutcome.ImportFailed, result.Outcome);
        Assert.Equal(Id, result.RequestId);
        Assert.True(RegionSourceValidationResultValidator.Validate(result, Id).IsValid);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }
}
