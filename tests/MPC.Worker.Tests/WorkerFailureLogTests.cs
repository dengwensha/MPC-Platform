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

    [Fact]
    public void Cleanup_RemovesOnlyOldUnlockedOwnedStaging()
    {
        string folder = _folder;
        Directory.CreateDirectory(folder);
        string old = Path.Combine(folder, "worker-" + Guid.NewGuid().ToString("N") + ".tmp");
        string busy = Path.Combine(folder, "worker-" + Guid.NewGuid().ToString("N") + ".tmp");
        string recent = Path.Combine(folder, "worker-" + Guid.NewGuid().ToString("N") + ".tmp");
        string other = Path.Combine(folder, "host-" + Guid.NewGuid().ToString("N") + ".tmp");
        string unrelated = Path.Combine(folder, "worker-notes.tmp");
        foreach (string path in new[] { old, busy, other, unrelated })
        {
            File.WriteAllText(path, "keep unless eligible");
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-2));
        }
        File.WriteAllText(recent, "recent");
        try
        {
            using (var active = new FileStream(busy, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                new WorkerFailureLog(folder).TryWrite(Id, new IOException("failure"));
                Assert.True(File.Exists(busy));
            }
            Assert.False(File.Exists(old));
            Assert.True(File.Exists(recent));
            Assert.True(File.Exists(other));
            Assert.True(File.Exists(unrelated));
            Assert.True(File.Exists(Path.Combine(folder, "worker-last-failure.json")));
        }
        finally { /* Fixture cleanup follows. */ }
    }

    [Theory]
    [InlineData(8190)]
    [InlineData(8191)]
    public void UnicodeBoundary_RetainsReadableTurkishWithoutBrokenPair(int leading)
    {
        string folder = _folder;
        string text = new string('İ', leading) + "\U0001F600" + "son";
        try
        {
            new WorkerFailureLog(folder).TryWrite(Id, new ExactTextException(text));
            string path = Path.Combine(folder, "worker-last-failure.json");
            string raw = File.ReadAllText(path);
            Assert.Contains("İ", raw);
            using var json = JsonDocument.Parse(raw);
            string details = json.RootElement.GetProperty("ExceptionDetails").GetString()!;
            Assert.True(json.RootElement.GetProperty("DetailsTruncated").GetBoolean());
            Assert.Equal(-1, details.IndexOf('\uFFFD'));
            Assert.False(char.IsHighSurrogate(details[details.Length - 1]));
            Assert.Equal(leading == 8190 ? text.Substring(0, 8192) : text.Substring(0, 8191), details);
            Assert.True(new FileInfo(path).Length < 64 * 1024);
        }
        finally { /* Fixture cleanup follows. */ }
    }

    private sealed class ExactTextException : Exception
    {
        private readonly string _text;
        internal ExactTextException(string text) { _text = text; }
        public override string ToString() => _text;
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
