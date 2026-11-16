using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MPC.Contracts;

namespace MPC.Worker.Tests;

public sealed class WorkerProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_ReturnsInvalidInvocation_ForNullArguments()
    {
        var error =
            new StringWriter();

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                null,
                error);

        Assert.Equal(
            WorkerExitCodes.InvalidInvocation,
            exitCode);

        Assert.Contains(
            "Usage:",
            error.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task RunAsync_ReturnsInvalidInvocation_ForWrongArgumentCount(
        int argumentCount)
    {
        var args =
            new string[argumentCount];

        Array.Fill(
            args,
            "argument");

        var error =
            new StringWriter();

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                args,
                error);

        Assert.Equal(
            WorkerExitCodes.InvalidInvocation,
            exitCode);

        Assert.Contains(
            "Usage:",
            error.ToString());
    }

    [Theory]
    [InlineData("", "result.json")]
    [InlineData("request.json", " ")]
    public async Task RunAsync_ReturnsInvalidInvocation_ForBlankPath(
        string requestPath,
        string resultPath)
    {
        var error =
            new StringWriter();

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                new[]
                {
                    requestPath,
                    resultPath
                },
                error);

        Assert.Equal(
            WorkerExitCodes.InvalidInvocation,
            exitCode);

        Assert.Contains(
            "Usage:",
            error.ToString());
    }

    [Fact]
    public async Task RunAsync_WritesFailureResult_ForMalformedJson()
    {
        using var scope =
            new TemporaryFileScope();

        await File.WriteAllTextAsync(
            scope.RequestPath,
            "{ malformed json");

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                new[]
                {
                    scope.RequestPath,
                    scope.ResultPath
                },
                TextWriter.Null);

        var result =
            await ReadResultAsync(
                scope.ResultPath);

        var diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerExitCodes.RequestReadFailed,
            exitCode);

        Assert.Equal(
            WorkerOperationStatus.Failed,
            result.Status);

        Assert.Equal(
            WorkerProcessRunner.RequestReadFailedCode,
            diagnostic.Code);
    }

    [Fact]
    public async Task RunAsync_WritesRejectedResult_ForInvalidRequest()
    {
        using var scope =
            new TemporaryFileScope();

        var request =
            new WorkerOperationRequest(
                "2.0",
                "invalid",
                (WorkerOperationKind)999,
                " ");

        await WriteRequestAsync(
            scope.RequestPath,
            request);

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                new[]
                {
                    scope.RequestPath,
                    scope.ResultPath
                },
                TextWriter.Null);

        var result =
            await ReadResultAsync(
                scope.ResultPath);

        Assert.Equal(
            WorkerExitCodes.RequestRejected,
            exitCode);

        Assert.Equal(
            WorkerOperationStatus.Rejected,
            result.Status);

        Assert.Equal(
            4,
            result.Diagnostics.Count);
    }

    [Fact]
    public async Task RunAsync_WritesFailedResult_ForValidRequest()
    {
        using var scope =
            new TemporaryFileScope();

        var requestId =
            Guid.NewGuid().ToString("D");

        var request =
            new WorkerOperationRequest(
                ContractVersion.Current,
                requestId,
                WorkerOperationKind.AnalyzeModel,
                scope.InputArtifactPath);

        await WriteRequestAsync(
            scope.RequestPath,
            request);

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                new[]
                {
                    scope.RequestPath,
                    scope.ResultPath
                },
                TextWriter.Null);

        var result =
            await ReadResultAsync(
                scope.ResultPath);

        var diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerExitCodes.OperationFailed,
            exitCode);

        Assert.Equal(
            requestId,
            result.RequestId);

        Assert.Equal(
            WorkerOperationStatus.Failed,
            result.Status);

        Assert.False(
            string.IsNullOrWhiteSpace(
                diagnostic.Code));
    }

    [Fact]
    public async Task RunAsync_ReturnsResultWriteFailed_WhenTargetIsDirectory()
    {
        using var scope =
            new TemporaryFileScope();

        var request =
            new WorkerOperationRequest(
                ContractVersion.Current,
                Guid.NewGuid().ToString("D"),
                WorkerOperationKind.AnalyzeModel,
                scope.InputArtifactPath);

        await WriteRequestAsync(
            scope.RequestPath,
            request);

        var error =
            new StringWriter();

        var runner =
            new WorkerProcessRunner();

        var exitCode =
            await runner.RunAsync(
                new[]
                {
                    scope.RequestPath,
                    scope.DirectoryPath
                },
                error);

        Assert.Equal(
            WorkerExitCodes.ResultWriteFailed,
            exitCode);

        Assert.Contains(
            "could not be written",
            error.ToString());
    }

    [Fact]
    public void ProcessExitCodes_KeepPublishedNumericAssignments()
    {
        // Host and acceptance tools consume these numbers across a process boundary.
        // Exercise semantics with existing runner/process tests; this pins the ABI.
        Assert.Equal(0, WorkerExitCodes.Succeeded);
        Assert.Equal(2, WorkerExitCodes.RequestRejected);
        Assert.Equal(3, WorkerExitCodes.OperationFailed);
        Assert.Equal(10, WorkerExitCodes.InvalidInvocation);
        Assert.Equal(11, WorkerExitCodes.RequestReadFailed);
        Assert.Equal(12, WorkerExitCodes.ResultWriteFailed);
    }

    private static Task WriteRequestAsync(
        string path,
        WorkerOperationRequest request)
    {
        var json =
            JsonSerializer.Serialize(request);

        return File.WriteAllTextAsync(
            path,
            json);
    }

    private static async Task<WorkerOperationResult> ReadResultAsync(
        string path)
    {
        var json =
            await File.ReadAllTextAsync(path);

        var result =
            JsonSerializer.Deserialize<WorkerOperationResult>(
                json);

        return Assert.IsType<WorkerOperationResult>(
            result);
    }

    private sealed class TemporaryFileScope : IDisposable
    {
        public TemporaryFileScope()
        {
            DirectoryPath =
                Path.Combine(
                    Path.GetTempPath(),
                    "MPC.Worker.Tests",
                    Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                DirectoryPath);

            RequestPath =
                Path.Combine(
                    DirectoryPath,
                    "request.json");

            ResultPath =
                Path.Combine(
                    DirectoryPath,
                    "result.json");

            InputArtifactPath =
                Path.Combine(
                    DirectoryPath,
                    "input.model");
        }

        public string DirectoryPath { get; }

        public string RequestPath { get; }

        public string ResultPath { get; }

        public string InputArtifactPath { get; }

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