using System;
using System.IO;
using MPC.Contracts;
using MPC.Host.SolidWorks.Commands;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Commands;

public sealed class StepImportHostCommandIntegrationTests
{
    [Fact]
    public void Execute_T001Prism_CrossesPhysicalWorkerBoundary()
    {
        string repositoryRoot =
            FindRepositoryRoot();

#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif

        string workerExecutablePath =
            Path.Combine(
                repositoryRoot,
                "src",
                "MPC.Worker",
                "bin",
                "x64",
                configuration,
                "net8.0",
                "MPC.Worker.exe");

        string stepFilePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "Step",
                "Golden",
                "T001_PRISM.step");

        Assert.True(
            File.Exists(workerExecutablePath),
            $"Worker executable was not found: " +
            $"'{workerExecutablePath}'.");

        Assert.True(
            File.Exists(stepFilePath),
            $"Golden STEP file was not found: " +
            $"'{stepFilePath}'.");

        var command =
            new StepImportHostCommand();

        StepImportHostCommandResult result =
            command.Execute(
                new StepImportHostCommandRequest(
                    workerExecutablePath,
                    stepFilePath,
                    timeoutMilliseconds: 30000));

        Assert.True(
            result.IsSuccessful,
            CreateFailureMessage(result));

        WorkerOperationResult workerResult =
            Assert.IsType<WorkerOperationResult>(
                result.WorkerResult);

        Assert.Equal(
            WorkerOperationStatus.Succeeded,
            workerResult.Status);

        Assert.Empty(workerResult.Diagnostics);

        WorkerOperationSummary summary =
            Assert.IsType<WorkerOperationSummary>(
                workerResult.Summary);

        Assert.Equal("#63", summary.SourceSolidEntityId);
        Assert.Equal(1, summary.SolidCount);
        Assert.Equal(1, summary.OuterShellCount);
        Assert.Equal(0, summary.InnerShellCount);
        Assert.Equal(6, summary.FaceCount);
        Assert.Equal(6, summary.LoopCount);
        Assert.Equal(12, summary.EdgeCount);
        Assert.Equal(8, summary.VertexCount);
    }

    private static string FindRepositoryRoot()
    {
        var currentDirectory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string solutionPath =
                Path.Combine(
                    currentDirectory.FullName,
                    "src",
                    "MPC.sln");

            if (File.Exists(solutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory =
                currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException(
            "MPC repository root could not be located.");
    }

    private static string CreateFailureMessage(
        StepImportHostCommandResult result)
    {
        if (result.WorkerResult is not null &&
            result.WorkerResult.Diagnostics.Count > 0)
        {
            return string.Join(
                Environment.NewLine,
                System.Linq.Enumerable.Select(
                    result.WorkerResult.Diagnostics,
                    diagnostic =>
                        $"{diagnostic.Code}: " +
                        $"{diagnostic.Message}"));
        }

        return
            $"Host command ended with status " +
            $"'{result.Status}': " +
            $"{result.FailureMessage}";
    }
}
