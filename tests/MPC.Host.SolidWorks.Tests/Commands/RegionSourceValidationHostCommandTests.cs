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

    private static void AssertCleaned(FakeLauncher launcher) =>
        Assert.False(Directory.Exists(Path.GetDirectoryName(launcher.RequestPath!)!));

    private sealed class FakeLauncher : IWorkerProcessLauncher
    {
        internal bool Called;
        internal bool FailLaunch;
        internal bool SkipReply;
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
