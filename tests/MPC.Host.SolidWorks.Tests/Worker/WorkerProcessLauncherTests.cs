using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using MPC.Host.SolidWorks.Worker;

namespace MPC.Host.SolidWorks.Tests.Worker;

public sealed class WorkerProcessLauncherTests
{
    [Fact]
    public void Constructor_RejectsNullProcessStarter()
    {
        Assert.Throws<ArgumentNullException>(
            () => new WorkerProcessLauncher(null!));
    }

    [Fact]
    public void Launch_ReturnsInvalidRequest_ForNullRequest()
    {
        var starter =
            new RecordingProcessStarter();

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(null);

        Assert.Equal(
            WorkerLaunchStatus.InvalidRequest,
            result.Status);

        Assert.False(result.IsStarted);
        Assert.Null(result.Handle);
        Assert.Equal(0, starter.CallCount);
    }

    [Theory]
    [InlineData(null, "request.json", "result.json")]
    [InlineData("", "request.json", "result.json")]
    [InlineData(" ", "request.json", "result.json")]
    [InlineData("worker.exe", null, "result.json")]
    [InlineData("worker.exe", " ", "result.json")]
    [InlineData("worker.exe", "request.json", null)]
    [InlineData("worker.exe", "request.json", " ")]
    public void Launch_ReturnsInvalidRequest_ForInvalidPath(
        string? executablePath,
        string? requestPath,
        string? resultPath)
    {
        var starter =
            new RecordingProcessStarter();

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(
                new WorkerLaunchRequest(
                    executablePath,
                    requestPath,
                    resultPath));

        Assert.Equal(
            WorkerLaunchStatus.InvalidRequest,
            result.Status);

        Assert.Equal(0, starter.CallCount);
    }

    [Fact]
    public void Launch_ReturnsExecutableNotFound_WhenWorkerDoesNotExist()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: false,
                createRequest: true);

        var starter =
            new RecordingProcessStarter();

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(scope.CreateRequest());

        Assert.Equal(
            WorkerLaunchStatus.ExecutableNotFound,
            result.Status);

        Assert.Equal(0, starter.CallCount);
    }

    [Fact]
    public void Launch_ReturnsRequestFileNotFound_WhenRequestDoesNotExist()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: true,
                createRequest: false);

        var starter =
            new RecordingProcessStarter();

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(scope.CreateRequest());

        Assert.Equal(
            WorkerLaunchStatus.RequestFileNotFound,
            result.Status);

        Assert.Equal(0, starter.CallCount);
    }

    [Fact]
    public void Launch_StartsWorker_WithCanonicalTwoArgumentInvocation()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: true,
                createRequest: true);

        var starter =
            new RecordingProcessStarter(
                processId: 4172);

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(scope.CreateRequest());

        Assert.True(result.IsStarted);
        Assert.Equal(
            WorkerLaunchStatus.Started,
            result.Status);

        Assert.NotNull(result.Handle);
        Assert.Equal(
            4172,
            result.Handle!.ProcessId);

        Assert.Null(result.FailureType);
        Assert.Null(result.FailureMessage);
        Assert.Equal(1, starter.CallCount);

        var processRequest =
            Assert.IsType<ProcessStartRequest>(
                starter.LastRequest);

        Assert.Equal(
            Path.GetFullPath(scope.ExecutablePath),
            processRequest.ExecutablePath);

        Assert.Equal(
            Path.GetDirectoryName(
                Path.GetFullPath(scope.ExecutablePath)),
            processRequest.WorkingDirectory);

        Assert.Equal(
            new[]
            {
                Path.GetFullPath(scope.RequestPath),
                Path.GetFullPath(scope.ResultPath)
            },
            processRequest.Arguments);
    }

    [Fact]
    public void Launch_PreservesPathsContainingSpaces()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: true,
                createRequest: true,
                directoryName: "MPC Worker Launch Tests");

        var starter =
            new RecordingProcessStarter();

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(scope.CreateRequest());

        Assert.True(result.IsStarted);

        var processRequest =
            Assert.IsType<ProcessStartRequest>(
                starter.LastRequest);

        Assert.Contains(
            "MPC Worker Launch Tests",
            processRequest.ExecutablePath);

        Assert.Contains(
            "MPC Worker Launch Tests",
            processRequest.Arguments[0]);

        Assert.Contains(
            "MPC Worker Launch Tests",
            processRequest.Arguments[1]);
    }

    [Fact]
    public void Launch_ReturnsStartFailed_WhenStarterThrows()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: true,
                createRequest: true);

        var starter =
            new RecordingProcessStarter(
                exception:
                    new InvalidOperationException(
                        "Process start failed."));

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(scope.CreateRequest());

        Assert.Equal(
            WorkerLaunchStatus.StartFailed,
            result.Status);

        Assert.False(result.IsStarted);
        Assert.Null(result.Handle);

        Assert.Equal(
            nameof(InvalidOperationException),
            result.FailureType);

        Assert.Equal(
            "Process start failed.",
            result.FailureMessage);
    }

    [Fact]
    public void Launch_ReturnsStartFailed_WhenStarterReturnsInvalidProcessId()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: true,
                createRequest: true);

        var starter =
            new RecordingProcessStarter(
                processId: 0);

        var launcher =
            new WorkerProcessLauncher(starter);

        var result =
            launcher.Launch(scope.CreateRequest());

        Assert.Equal(
            WorkerLaunchStatus.StartFailed,
            result.Status);

        Assert.Null(result.Handle);
    }

    [Fact]
    public void Launch_ProducesSameProcessDefinition_ForSameRequest()
    {
        using var scope =
            new TemporaryLaunchScope(
                createExecutable: true,
                createRequest: true);

        var starter =
            new RecordingProcessStarter();

        var launcher =
            new WorkerProcessLauncher(starter);

        var request =
            scope.CreateRequest();

        var firstResult =
            launcher.Launch(request);

        var firstProcessRequest =
            Assert.IsType<ProcessStartRequest>(
                starter.LastRequest);

        var secondResult =
            launcher.Launch(request);

        var secondProcessRequest =
            Assert.IsType<ProcessStartRequest>(
                starter.LastRequest);

        Assert.True(firstResult.IsStarted);
        Assert.True(secondResult.IsStarted);

        Assert.Equal(
            firstProcessRequest.ExecutablePath,
            secondProcessRequest.ExecutablePath);

        Assert.Equal(
            firstProcessRequest.WorkingDirectory,
            secondProcessRequest.WorkingDirectory);

        Assert.Equal(
            firstProcessRequest.Arguments,
            secondProcessRequest.Arguments);
    }

    private sealed class RecordingProcessStarter :
        IProcessStarter
    {
        private readonly int _processId;
        private readonly Exception? _exception;

        internal int CallCount { get; private set; }

        internal ProcessStartRequest? LastRequest { get; private set; }

        internal RecordingProcessStarter(
            int processId = 1234,
            Exception? exception = null)
        {
            _processId = processId;
            _exception = exception;
        }

        public WorkerProcessHandle Start(
            ProcessStartRequest request)
        {
            CallCount++;
            LastRequest = request;

            if (_exception is not null)
            {
                throw _exception;
            }

            return new WorkerProcessHandle(
                new StartedProcessExecution(_processId));
        }
    }

    private sealed class StartedProcessExecution :
        IProcessExecution
    {
        private readonly int _processId;

        internal StartedProcessExecution(int processId)
        {
            _processId = processId;
        }

        public int Id => _processId;

        public int ExitCode => 0;

        public bool WaitForExit(int milliseconds)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }

    private sealed class TemporaryLaunchScope :
        IDisposable
    {
        internal string DirectoryPath { get; }

        internal string ExecutablePath { get; }

        internal string RequestPath { get; }

        internal string ResultPath { get; }

        internal TemporaryLaunchScope(
            bool createExecutable,
            bool createRequest,
            string directoryName = "MPC.Worker.Launch.Tests")
        {
            DirectoryPath =
                Path.Combine(
                    Path.GetTempPath(),
                    directoryName,
                    Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                DirectoryPath);

            ExecutablePath =
                Path.Combine(
                    DirectoryPath,
                    "MPC.Worker.exe");

            RequestPath =
                Path.Combine(
                    DirectoryPath,
                    "request.json");

            ResultPath =
                Path.Combine(
                    DirectoryPath,
                    "result.json");

            if (createExecutable)
            {
                File.WriteAllText(
                    ExecutablePath,
                    string.Empty);
            }

            if (createRequest)
            {
                File.WriteAllText(
                    RequestPath,
                    "{}");
            }
        }

        internal WorkerLaunchRequest CreateRequest()
        {
            return new WorkerLaunchRequest(
                ExecutablePath,
                RequestPath,
                ResultPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(
                    DirectoryPath,
                    recursive: true);
            }
        }
    }
}
