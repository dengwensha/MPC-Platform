using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MPC.Contracts;

namespace MPC.Worker.Tests;

public sealed class WorkerExecutableIntegrationTests
{
    [Fact]
    public async Task Executable_ProcessesRequestAcrossRealProcessBoundary()
    {
        var directoryPath =
            Path.Combine(
                Path.GetTempPath(),
                "MPC.Worker.Process.Tests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directoryPath);

        try
        {
            var requestPath =
                Path.Combine(
                    directoryPath,
                    "request.json");

            var resultPath =
                Path.Combine(
                    directoryPath,
                    "result.json");

            var inputArtifactPath =
                Path.Combine(
                    directoryPath,
                    "input.model");

            var requestId =
                Guid.NewGuid().ToString("D");

            var request =
                new WorkerOperationRequest(
                    ContractVersion.Current,
                    requestId,
                    WorkerOperationKind.AnalyzeModel,
                    inputArtifactPath);

            await File.WriteAllTextAsync(
                requestPath,
                JsonSerializer.Serialize(request));

            var workerDirectory =
                Path.GetDirectoryName(
                    typeof(WorkerProcessRunner)
                        .Assembly
                        .Location);

            Assert.False(
                string.IsNullOrWhiteSpace(workerDirectory));

            var executablePath =
                Path.Combine(
                    workerDirectory!,
                    "MPC.Worker.exe");

            Assert.True(
                File.Exists(executablePath));

            var startInfo =
                new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

            startInfo.ArgumentList.Add(requestPath);
            startInfo.ArgumentList.Add(resultPath);

            using var process =
                Process.Start(startInfo);

            Assert.NotNull(process);

            var standardOutputTask =
                process.StandardOutput.ReadToEndAsync();

            var standardErrorTask =
                process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var standardOutput =
                await standardOutputTask;

            var standardError =
                await standardErrorTask;

            Assert.Equal(
                WorkerExitCodes.OperationFailed,
                process.ExitCode);

            Assert.Empty(standardOutput);
            Assert.Empty(standardError);
            Assert.True(File.Exists(resultPath));

            var resultJson =
                await File.ReadAllTextAsync(resultPath);

            var result =
                JsonSerializer.Deserialize<WorkerOperationResult>(
                    resultJson);

            var typedResult =
                Assert.IsType<WorkerOperationResult>(result);

            var diagnostic =
                Assert.Single(typedResult.Diagnostics);

            Assert.Equal(
                requestId,
                typedResult.RequestId);

            Assert.Equal(
                WorkerOperationStatus.Failed,
                typedResult.Status);

            Assert.Equal(
                WorkerRequestProcessor.OperationNotImplementedCode,
                diagnostic.Code);
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(
                    directoryPath,
                    recursive: true);
            }
        }
    }
}