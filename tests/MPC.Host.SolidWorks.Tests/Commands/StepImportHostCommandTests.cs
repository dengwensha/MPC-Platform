using System;
using System.IO;
using System.Text.Json;
using MPC.Contracts;
using MPC.Host.SolidWorks.Commands;
using MPC.Host.SolidWorks.Worker;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Commands;

public sealed class StepImportHostCommandTests
{
    [Fact]
    public void Execute_InvalidRequest_DoesNotLaunchWorker()
    {
        var launcher =
            new FakeLauncher();

        var command =
            CreateCommand(
                launcher,
                WorkerCompletionResult.Completed(0));

        StepImportHostCommandResult result =
            command.Execute(null);

        Assert.Equal(
            StepImportHostCommandStatus.InvalidRequest,
            result.Status);

        Assert.False(launcher.WasCalled);
        Assert.Null(result.WorkerResult);
    }

    [Fact]
    public void Execute_LaunchFailure_ReturnsFailureAndCleansTransport()
    {
        var launcher =
            new FakeLauncher
            {
                LaunchResultFactory =
                    request =>
                        WorkerLaunchResult.Failed(
                            WorkerLaunchStatus.StartFailed,
                            "Worker start failed.")
            };

        var command =
            CreateCommand(
                launcher,
                WorkerCompletionResult.Completed(0));

        StepImportHostCommandResult result =
            command.Execute(
                CreateRequest());

        Assert.Equal(
            StepImportHostCommandStatus.LaunchFailed,
            result.Status);

        AssertTransportDeleted(launcher);
    }

    [Fact]
    public void Execute_Timeout_ReturnsDistinctStatusAndCleansTransport()
    {
        var launcher =
            CreateStartedLauncher();

        var command =
            CreateCommand(
                launcher,
                WorkerCompletionResult.TimedOut(
                    "Worker timed out."));

        StepImportHostCommandResult result =
            command.Execute(
                CreateRequest());

        Assert.Equal(
            StepImportHostCommandStatus.TimedOut,
            result.Status);

        AssertTransportDeleted(launcher);
    }

    [Fact]
    public void Execute_SucceededWorkerResult_ReturnsCanonicalSummary()
    {
        var launcher =
            CreateStartedLauncher(
                writeSuccessfulResult: true);

        var command =
            CreateCommand(
                launcher,
                WorkerCompletionResult.Completed(0));

        StepImportHostCommandResult result =
            command.Execute(
                CreateRequest());

        Assert.Equal(
            StepImportHostCommandStatus.Succeeded,
            result.Status);

        WorkerOperationResult workerResult =
            Assert.IsType<WorkerOperationResult>(
                result.WorkerResult);

        WorkerOperationSummary summary =
            Assert.IsType<WorkerOperationSummary>(
                workerResult.Summary);

        Assert.Equal("#63", summary.SourceSolidEntityId);
        Assert.Equal(6, summary.FaceCount);
        Assert.Equal(12, summary.EdgeCount);
        Assert.Equal(8, summary.VertexCount);

        AssertTransportDeleted(launcher);
    }

    [Fact]
    public void Execute_FailedWorkerResult_PreservesDiagnostics()
    {
        var launcher =
            CreateStartedLauncher(
                writeSuccessfulResult: false);

        var command =
            CreateCommand(
                launcher,
                WorkerCompletionResult.Completed(4));

        StepImportHostCommandResult result =
            command.Execute(
                CreateRequest());

        Assert.Equal(
            StepImportHostCommandStatus.WorkerFailed,
            result.Status);

        WorkerOperationResult workerResult =
            Assert.IsType<WorkerOperationResult>(
                result.WorkerResult);

        ContractDiagnostic diagnostic =
            Assert.Single(
                workerResult.Diagnostics);

        Assert.Equal(
            "APP-STEP-TEST-001",
            diagnostic.Code);

        AssertTransportDeleted(launcher);
    }

    private static StepImportHostCommand CreateCommand(
        FakeLauncher launcher,
        WorkerCompletionResult completionResult)
    {
        return new StepImportHostCommand(
            launcher,
            new FakeCompletionObserver(
                completionResult),
            new WorkerContractJsonSerializer());
    }

    private static StepImportHostCommandRequest CreateRequest()
    {
        return new StepImportHostCommandRequest(
            @"C:\MPC\MPC.Worker.exe",
            @"C:\MPC\part.step",
            timeoutMilliseconds: 30000);
    }

    private static FakeLauncher CreateStartedLauncher(
        bool? writeSuccessfulResult = null)
    {
        return new FakeLauncher
        {
            LaunchResultFactory =
                request =>
                {
                    if (writeSuccessfulResult.HasValue)
                    {
                        WriteWorkerResult(
                            request,
                            writeSuccessfulResult.Value);
                    }

                    return WorkerLaunchResult.Started(
                        new WorkerProcessHandle(
                            new FakeProcessExecution()));
                }
        };
    }

    private static void WriteWorkerResult(
        WorkerLaunchRequest launchRequest,
        bool succeeded)
    {
        string requestJson =
            File.ReadAllText(
                launchRequest.RequestJsonPath!);

        WorkerOperationRequest request =
            JsonSerializer.Deserialize<WorkerOperationRequest>(
                requestJson)!;

        WorkerOperationResult result;

        if (succeeded)
        {
            result =
                new WorkerOperationResult(
                    ContractVersion.Current,
                    request.RequestId,
                    WorkerOperationStatus.Succeeded,
                    outputArtifactPath: null,
                    Array.Empty<ContractDiagnostic>(),
                    new WorkerOperationSummary(
                        "#63",
                        solidCount: 1,
                        outerShellCount: 1,
                        innerShellCount: 0,
                        faceCount: 6,
                        loopCount: 6,
                        edgeCount: 12,
                        vertexCount: 8));
        }
        else
        {
            result =
                new WorkerOperationResult(
                    ContractVersion.Current,
                    request.RequestId,
                    WorkerOperationStatus.Failed,
                    outputArtifactPath: null,
                    new[]
                    {
                        new ContractDiagnostic(
                            "APP-STEP-TEST-001",
                            ContractDiagnosticSeverity.Error,
                            "STEP import failed.")
                    });
        }

        File.WriteAllText(
            launchRequest.ResultJsonPath!,
            JsonSerializer.Serialize(result));
    }

    private static void AssertTransportDeleted(
        FakeLauncher launcher)
    {
        Assert.False(
            string.IsNullOrWhiteSpace(
                launcher.LastRequestJsonPath));

        string transportDirectory =
            Path.GetDirectoryName(
                launcher.LastRequestJsonPath!)!;

        Assert.False(
            Directory.Exists(
                transportDirectory));
    }

    private sealed class FakeLauncher :
        IWorkerProcessLauncher
    {
        internal Func<
            WorkerLaunchRequest,
            WorkerLaunchResult>? LaunchResultFactory
        {
            get;
            set;
        }

        internal bool WasCalled { get; private set; }

        internal string? LastRequestJsonPath
        {
            get;
            private set;
        }

        public WorkerLaunchResult Launch(
            WorkerLaunchRequest? request)
        {
            WasCalled = true;

            WorkerLaunchRequest typedRequest =
                request
                ?? throw new InvalidOperationException(
                    "A launch request was expected.");

            LastRequestJsonPath =
                typedRequest.RequestJsonPath;

            return LaunchResultFactory?.Invoke(
                       typedRequest)
                   ?? WorkerLaunchResult.Failed(
                       WorkerLaunchStatus.StartFailed,
                       "No fake launch result was configured.");
        }
    }

    private sealed class FakeCompletionObserver :
        IWorkerProcessCompletionObserver
    {
        private readonly WorkerCompletionResult
            _result;

        internal FakeCompletionObserver(
            WorkerCompletionResult result)
        {
            _result = result;
        }

        public WorkerCompletionResult Observe(
            WorkerProcessHandle handle,
            int timeoutMilliseconds)
        {
            handle.Dispose();

            return _result;
        }
    }

    private sealed class FakeProcessExecution :
        IProcessExecution
    {
        public int Id => 1234;

        public int ExitCode => 0;

        public bool WaitForExit(
            int milliseconds)
        {
            return true;
        }

        public void Terminate()
        {
        }

        public void Dispose()
        {
        }
    }
}
