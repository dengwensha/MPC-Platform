using System;
using System.IO;
using System.Text.Json;
using MPC.Contracts;
using MPC.Host.SolidWorks.Commands;
using MPC.Host.SolidWorks.Worker;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Commands;

public sealed class RegionSourceValidationHostCommandTests
{
    private static RegionSourceValidationRequest Request(
        RegionSourceValidationRequirement requirement = RegionSourceValidationRequirement.SourceLinkOnly) =>
        new RegionSourceValidationRequest("1.1", Guid.NewGuid().ToString("D"),
            WorkerOperationKind.ValidateRegionSource, "part.step", "#8", new string('A', 64), requirement);

    private static RegionSourceValidationHostCommand Command(FakeLauncher launcher,
        WorkerCompletionResult? completion = null) =>
        new RegionSourceValidationHostCommand(launcher,
            new FakeObserver(completion ?? WorkerCompletionResult.Completed(0)),
            new WorkerContractJsonSerializer());

    [Fact]
    public void ValidMatch_PreservesV11PayloadAndCleansTransport()
    {
        var launcher = new FakeLauncher();
        var request = Request();
        var result = Command(launcher).Execute("worker.exe", request, 30000);
        Assert.Equal(RegionSourceHostStatus.SourceLinkMatched, result.Status);
        Assert.Equal(request.RequestId, launcher.Sent!.RequestId);
        Assert.Equal("1.1", launcher.Sent.ContractVersion);
        Assert.Equal(request.SourceFaceId, launcher.Sent.SourceFaceId);
        Assert.Equal(request.ExpectedDocumentSha256, launcher.Sent.ExpectedDocumentSha256);
        Assert.Equal(request.Requirement, launcher.Sent.Requirement);
        Assert.Equal(RegionSourceValidationOutcome.SourceLinkMatched, result.WorkerResult!.Outcome);
        Assert.True(launcher.Execution.Disposed);
        AssertCleaned(launcher);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(8)]
    public void KnownBlockWithExitTwo_RemainsBlocked(int outcome)
    {
        var launcher = new FakeLauncher { Outcome = (RegionSourceValidationOutcome)outcome };
        var result = Command(launcher, WorkerCompletionResult.Completed(2))
            .Execute("worker.exe", Request(), 30000);
        Assert.Equal(RegionSourceHostStatus.Blocked, result.Status);
        Assert.Equal((RegionSourceValidationOutcome)outcome, result.WorkerResult!.Outcome);
        AssertCleaned(launcher);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public void MatchWithWrongExitOrMaterialRequirement_IsNeverAccepted(int exit)
    {
        var launcher = new FakeLauncher();
        var result = Command(launcher, WorkerCompletionResult.Completed(exit)).Execute(
            "worker.exe", Request(RegionSourceValidationRequirement.MaterialSideDependent), 30000);
        Assert.Equal(RegionSourceHostStatus.ResultReadFailed, result.Status);
        AssertCleaned(launcher);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(1, 3)]
    [InlineData(4, 0)]
    [InlineData(7, 0)]
    [InlineData(7, 2)]
    public void InconsistentExitAndOutcome_FailsClosed(int outcome, int exit)
    {
        var launcher = new FakeLauncher { Outcome = (RegionSourceValidationOutcome)outcome };
        var result = Command(launcher, WorkerCompletionResult.Completed(exit))
            .Execute("worker.exe", Request(), 30000);
        Assert.Equal(RegionSourceHostStatus.ResultReadFailed, result.Status);
        AssertCleaned(launcher);
    }

    [Fact]
    public void ImportFailureWithExitThree_IsWorkerFailure()
    {
        var launcher = new FakeLauncher { Outcome = RegionSourceValidationOutcome.ImportFailed };
        var result = Command(launcher, WorkerCompletionResult.Completed(3))
            .Execute("worker.exe", Request(), 30000);
        Assert.Equal(RegionSourceHostStatus.WorkerFailed, result.Status);
        AssertCleaned(launcher);
    }

    [Theory]
    [InlineData("version")]
    [InlineData("id")]
    [InlineData("operation")]
    [InlineData("default")]
    [InlineData("undefined")]
    [InlineData("legacy")]
    [InlineData("malformed")]
    [InlineData("null")]
    [InlineData("missing")]
    public void InvalidOrOldReply_CannotGrantMatch(string scenario)
    {
        var launcher = new FakeLauncher();
        launcher.Reply = request =>
        {
            if (scenario == "malformed") return "{ broken";
            if (scenario == "null") return "null";
            if (scenario == "missing") return "{}";
            if (scenario == "legacy")
                return JsonSerializer.Serialize(new WorkerOperationResult("1.0", request.RequestId,
                    WorkerOperationStatus.Succeeded, null, Array.Empty<ContractDiagnostic>()));
            return JsonSerializer.Serialize(new RegionSourceValidationResult(
                scenario == "version" ? "1.0" : "1.1",
                scenario == "id" ? Guid.NewGuid().ToString("D") : request.RequestId,
                scenario == "operation" ? WorkerOperationKind.AnalyzeModel : WorkerOperationKind.ValidateRegionSource,
                scenario == "default" ? RegionSourceValidationOutcome.Unknown :
                    scenario == "undefined" ? (RegionSourceValidationOutcome)99 :
                        RegionSourceValidationOutcome.SourceLinkMatched));
        };
        var result = Command(launcher).Execute("worker.exe", Request(), 30000);
        Assert.Equal(RegionSourceHostStatus.ResultReadFailed, result.Status);
        AssertCleaned(launcher);
    }

    [Theory]
    [InlineData("request")]
    [InlineData("worker")]
    [InlineData("timeout")]
    [InlineData("hash")]
    public void InvalidInput_DoesNotLaunch(string scenario)
    {
        var launcher = new FakeLauncher();
        var request = scenario == "request" ? null : scenario == "hash" ?
            new RegionSourceValidationRequest("1.1", Guid.NewGuid().ToString("D"),
                WorkerOperationKind.ValidateRegionSource, "part.step", "#8", "bad",
                RegionSourceValidationRequirement.SourceLinkOnly) : Request();
        var result = Command(launcher).Execute(scenario == "worker" ? "" : "worker.exe",
            request, scenario == "timeout" ? 0 : 30000);
        Assert.Equal(RegionSourceHostStatus.InvalidRequest, result.Status);
        Assert.False(launcher.Called);
    }

    [Fact]
    public void LaunchFailure_CleansTransport()
    {
        var launcher = new FakeLauncher { FailLaunch = true };
        Assert.Equal(RegionSourceHostStatus.LaunchFailed,
            Command(launcher).Execute("worker.exe", Request(), 30000).Status);
        AssertCleaned(launcher);
    }

    [Fact]
    public void Timeout_RemainsDistinctAndCleansTransport()
    {
        var launcher = new FakeLauncher();
        Assert.Equal(RegionSourceHostStatus.TimedOut,
            Command(launcher, WorkerCompletionResult.TimedOut("timeout"))
                .Execute("worker.exe", Request(), 30000).Status);
        AssertCleaned(launcher);
    }

    [Fact]
    public void ObservationFailure_RemainsDistinctAndCleansTransport()
    {
        var launcher = new FakeLauncher();
        Assert.Equal(RegionSourceHostStatus.ObservationFailed,
            Command(launcher, WorkerCompletionResult.ObservationFailed(new InvalidOperationException("observe")))
                .Execute("worker.exe", Request(), 30000).Status);
        AssertCleaned(launcher);
    }

    [Fact]
    public void MissingReply_CannotGrantMatch()
    {
        var launcher = new FakeLauncher { SkipReply = true };
        Assert.Equal(RegionSourceHostStatus.ResultReadFailed,
            Command(launcher).Execute("worker.exe", Request(), 30000).Status);
        AssertCleaned(launcher);
    }

    [Theory]
    [InlineData("launch", 3)]
    [InlineData("timeout", 4)]
    [InlineData("observe", 5)]
    [InlineData("missing", 6)]
    [InlineData("malformed", 6)]
    [InlineData("correlation", 6)]
    [InlineData("exit", 6)]
    [InlineData("transport", 8)]
    public void HostFailures_RecordCorrelationAndPreserveStatus(string scenario, int expectedStatus)
    {
        string folder = Path.Combine(Path.GetTempPath(), "MpcHostLog", Guid.NewGuid().ToString("N"));
        try
        {
            var launcher = new FakeLauncher
            {
                FailLaunch = scenario == "launch",
                SkipReply = scenario == "missing",
                ThrowTransport = scenario == "transport"
            };
            if (scenario == "malformed") launcher.Reply = _ => "{ broken";
            if (scenario == "correlation")
                launcher.Reply = _ => JsonSerializer.Serialize(new RegionSourceValidationResult(
                    "1.1", Guid.NewGuid().ToString("D"), WorkerOperationKind.ValidateRegionSource,
                    RegionSourceValidationOutcome.SourceLinkMatched));
            var completion = scenario == "timeout" ? WorkerCompletionResult.TimedOut("timeout evidence") :
                scenario == "observe" ? WorkerCompletionResult.ObservationFailed(new InvalidOperationException("observe evidence")) :
                WorkerCompletionResult.Completed(scenario == "exit" ? 3 : 0);
            var request = Request();
            DateTimeOffset before = DateTimeOffset.UtcNow;
            var command = new RegionSourceValidationHostCommand(launcher, new FakeObserver(completion),
                new WorkerContractJsonSerializer(), new HostFailureLog(folder));
            var result = command.Execute("worker.exe", request, 30000);
            Assert.Equal((RegionSourceHostStatus)expectedStatus, result.Status);
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "host-last-failure.json")));
            var root = json.RootElement;
            Assert.Equal(request.RequestId, root.GetProperty("RequestId").GetString());
            Assert.Equal(result.Status.ToString(), root.GetProperty("HostStatus").GetString());
            Assert.InRange(root.GetProperty("TimestampUtc").GetDateTimeOffset(), before, DateTimeOffset.UtcNow);
            if (scenario == "missing" || scenario == "malformed" || scenario == "transport")
                Assert.False(string.IsNullOrEmpty(root.GetProperty("FailureType").GetString()));
            if (scenario == "observe") Assert.Equal("InvalidOperationException", root.GetProperty("FailureType").GetString());
            if (scenario == "exit") Assert.Equal(3, root.GetProperty("WorkerExitCode").GetInt32());
            AssertCleaned(launcher);
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [Theory]
    [InlineData("launch", 3)]
    [InlineData("timeout", 4)]
    [InlineData("missing", 6)]
    public void LogDestinationFailure_DoesNotChangeHostFailure(string scenario, int expectedStatus)
    {
        string blocked = Path.GetTempFileName();
        try
        {
            var launcher = new FakeLauncher { FailLaunch = scenario == "launch", SkipReply = scenario == "missing" };
            var completion = scenario == "timeout" ? WorkerCompletionResult.TimedOut("timeout") : WorkerCompletionResult.Completed(0);
            var command = new RegionSourceValidationHostCommand(launcher, new FakeObserver(completion),
                new WorkerContractJsonSerializer(), new HostFailureLog(blocked));
            Assert.Equal((RegionSourceHostStatus)expectedStatus, command.Execute("worker.exe", Request(), 30000).Status);
            AssertCleaned(launcher);
        }
        finally { File.Delete(blocked); }
    }

    [Theory]
    [InlineData("invalid", 0)]
    [InlineData("success", 1)]
    [InlineData("blocked", 2)]
    [InlineData("worker", 7)]
    public void NonHostFailures_DoNotCreateLogDirectory(string scenario, int expectedStatus)
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var launcher = new FakeLauncher { Outcome = scenario == "blocked" ?
            RegionSourceValidationOutcome.MissingRegion : scenario == "worker" ?
            RegionSourceValidationOutcome.ImportFailed : RegionSourceValidationOutcome.SourceLinkMatched };
        var completion = WorkerCompletionResult.Completed(scenario == "blocked" ? 2 : scenario == "worker" ? 3 : 0);
        var command = new RegionSourceValidationHostCommand(launcher, new FakeObserver(completion),
            new WorkerContractJsonSerializer(), new HostFailureLog(folder));
        Assert.Equal((RegionSourceHostStatus)expectedStatus,
            command.Execute("worker.exe", scenario == "invalid" ? null : Request(), 30000).Status);
        Assert.False(Directory.Exists(folder));
    }

    [Fact]
    public void LocalRecord_IsBoundedReplacedAndPreservesUnicode()
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var log = new HostFailureLog(folder);
            var result = new RegionSourceValidationHostCommandResult(RegionSourceHostStatus.TransportFailed, "İşler");
            log.TryWrite("first", result, new HostFailureEvidence { Exception = new IOException("old") });
            log.TryWrite("second", result, new HostFailureEvidence { Exception = new IOException(new string('İ', 100000)) });
            string path = Path.Combine(folder, "host-last-failure.json");
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal("second", json.RootElement.GetProperty("RequestId").GetString());
            Assert.Equal("İşler", json.RootElement.GetProperty("FailureMessage").GetString());
            Assert.True(json.RootElement.GetProperty("DetailsTruncated").GetBoolean());
            Assert.True(new FileInfo(path).Length < 64 * 1024);
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [Fact]
    public void Cleanup_RemovesOnlyOldUnlockedOwnedStaging()
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string old = Path.Combine(folder, "host-" + Guid.NewGuid().ToString("N") + ".tmp");
        string busy = Path.Combine(folder, "host-" + Guid.NewGuid().ToString("N") + ".tmp");
        string recent = Path.Combine(folder, "host-" + Guid.NewGuid().ToString("N") + ".tmp");
        string other = Path.Combine(folder, "worker-" + Guid.NewGuid().ToString("N") + ".tmp");
        string unrelated = Path.Combine(folder, "host-notes.tmp");
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
                new HostFailureLog(folder).TryWrite("request",
                    new RegionSourceValidationHostCommandResult(RegionSourceHostStatus.LaunchFailed, "failure"),
                    new HostFailureEvidence());
                Assert.True(File.Exists(busy));
            }
            Assert.False(File.Exists(old));
            Assert.True(File.Exists(recent));
            Assert.True(File.Exists(other));
            Assert.True(File.Exists(unrelated));
            Assert.True(File.Exists(Path.Combine(folder, "host-last-failure.json")));
        }
        finally { Directory.Delete(folder, true); }
    }

    [Theory]
    [InlineData(8190)]
    [InlineData(8191)]
    public void UnicodeBoundary_RetainsReadableTurkishWithoutBrokenPair(int leading)
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string text = new string('İ', leading) + "\U0001F600" + "son";
        try
        {
            new HostFailureLog(folder).TryWrite("request",
                new RegionSourceValidationHostCommandResult(RegionSourceHostStatus.TransportFailed, "İşler"),
                new HostFailureEvidence { Exception = new ExactTextException(text) });
            string path = Path.Combine(folder, "host-last-failure.json");
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
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    private sealed class ExactTextException : Exception
    {
        private readonly string _text;
        internal ExactTextException(string text) { _text = text; }
        public override string ToString() => _text;
    }

    [Theory]
    [InlineData("id")]
    [InlineData("version")]
    [InlineData("outcome")]
    public void RejectedReply_LogsReceivedFieldsWithoutChangingRequestCorrelation(string scenario)
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var request = Request();
            string receivedId = scenario == "id" ? Guid.NewGuid().ToString("D") : request.RequestId!;
            string version = scenario == "version" ? "9.9" : "1.1";
            var outcome = scenario == "outcome" ? (RegionSourceValidationOutcome)99 : RegionSourceValidationOutcome.SourceLinkMatched;
            var launcher = new FakeLauncher { Reply = _ => JsonSerializer.Serialize(
                new RegionSourceValidationResult(version, receivedId, WorkerOperationKind.ValidateRegionSource, outcome)) };
            var command = new RegionSourceValidationHostCommand(launcher, new FakeObserver(WorkerCompletionResult.Completed(0)),
                new WorkerContractJsonSerializer(), new HostFailureLog(folder));
            Assert.Equal(RegionSourceHostStatus.ResultReadFailed, command.Execute("worker.exe", request, 30000).Status);
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "host-last-failure.json")));
            var root = json.RootElement;
            Assert.Equal(request.RequestId, root.GetProperty("RequestId").GetString());
            Assert.Equal(receivedId, root.GetProperty("ReceivedRequestId").GetString());
            Assert.Equal(version, root.GetProperty("ReceivedContractVersion").GetString());
            Assert.Equal((int)WorkerOperationKind.ValidateRegionSource, root.GetProperty("ReceivedOperation").GetInt32());
            Assert.Equal((int)outcome, root.GetProperty("ReceivedOutcome").GetInt32());
            AssertCleaned(launcher);
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [Theory]
    [InlineData(-532462766)]
    [InlineData(17)]
    public void UnexpectedWorkerExitWithoutReply_IsUnverifiedAndRetainsExitEvidence(int exit)
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var request = Request();
            var launcher = new FakeLauncher { SkipReply = true };
            var command = new RegionSourceValidationHostCommand(launcher,
                new FakeObserver(WorkerCompletionResult.Completed(exit)),
                new WorkerContractJsonSerializer(), new HostFailureLog(folder));
            var result = command.Execute("worker.exe", request, 30000);
            Assert.Equal(RegionSourceHostStatus.ResultReadFailed, result.Status);
            Assert.Null(result.WorkerResult);
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "host-last-failure.json")));
            Assert.Equal(exit, json.RootElement.GetProperty("WorkerExitCode").GetInt32());
            Assert.Equal(request.RequestId, json.RootElement.GetProperty("RequestId").GetString());
            Assert.Equal("HostCommand", json.RootElement.GetProperty("Boundary").GetString());
            AssertCleaned(launcher);
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    private static void AssertCleaned(FakeLauncher launcher) =>
        Assert.False(Directory.Exists(Path.GetDirectoryName(launcher.RequestPath!)!));

    private sealed class FakeLauncher : IWorkerProcessLauncher
    {
        internal bool Called;
        internal bool FailLaunch;
        internal bool SkipReply;
        internal bool ThrowTransport;
        internal string? RequestPath;
        internal RegionSourceValidationRequest? Sent;
        internal RegionSourceValidationOutcome Outcome = RegionSourceValidationOutcome.SourceLinkMatched;
        internal Func<RegionSourceValidationRequest, string>? Reply;
        internal FakeExecution Execution = new FakeExecution();

        public WorkerLaunchResult Launch(WorkerLaunchRequest? request)
        {
            Called = true;
            RequestPath = request!.RequestJsonPath;
            Sent = JsonSerializer.Deserialize<RegionSourceValidationRequest>(File.ReadAllText(RequestPath!))!;
            if (ThrowTransport) throw new IOException("transport evidence");
            if (FailLaunch)
                return WorkerLaunchResult.Failed(WorkerLaunchStatus.StartFailed, "launch");
            if (!SkipReply)
                File.WriteAllText(request.ResultJsonPath!, Reply?.Invoke(Sent) ??
                    JsonSerializer.Serialize(new RegionSourceValidationResult("1.1", Sent.RequestId,
                        WorkerOperationKind.ValidateRegionSource, Outcome)));
            return WorkerLaunchResult.Started(new WorkerProcessHandle(Execution));
        }
    }

    private sealed class FakeObserver : IWorkerProcessCompletionObserver
    {
        private readonly WorkerCompletionResult _result;
        internal FakeObserver(WorkerCompletionResult result) { _result = result; }
        public WorkerCompletionResult Observe(WorkerProcessHandle handle, int timeoutMilliseconds)
        {
            handle.Dispose();
            return _result;
        }
    }

    private sealed class FakeExecution : IProcessExecution
    {
        internal bool Disposed;
        public int Id => 1234;
        public int ExitCode => 0;
        public bool WaitForExit(int milliseconds) => true;
        public void Terminate() { }
        public void Dispose() { Disposed = true; }
    }
}
