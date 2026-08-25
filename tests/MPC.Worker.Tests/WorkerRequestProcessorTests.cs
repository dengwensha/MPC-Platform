using System;
using MPC.Contracts;

namespace MPC.Worker.Tests;

public sealed class WorkerRequestProcessorTests
{
    private readonly WorkerRequestProcessor
        _processor = new();

    [Fact]
    public void Process_RejectsInvalidContractRequest()
    {
        var request =
            new WorkerOperationRequest(
                "2.0",
                "invalid-request-id",
                (WorkerOperationKind)999,
                " ");

        WorkerOperationResult result =
            _processor.Process(request);

        Assert.Equal(
            ContractVersion.Current,
            result.ContractVersion);

        Assert.Equal(
            WorkerOperationStatus.Rejected,
            result.Status);

        Assert.Equal(
            4,
            result.Diagnostics.Count);

        Assert.Null(result.OutputArtifactPath);
        Assert.Null(result.Summary);
    }

    [Fact]
    public void Process_RejectsNullRequest()
    {
        WorkerOperationResult result =
            _processor.Process(null);

        ContractDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerOperationStatus.Rejected,
            result.Status);

        Assert.Equal(
            "CONTRACT-REQUEST-001",
            diagnostic.Code);

        Assert.Null(result.Summary);
    }

    [Fact]
    public void Process_T001Prism_ReturnsCanonicalSummary()
    {
        string requestId =
            Guid.NewGuid().ToString("D");

        var request =
            new WorkerOperationRequest(
                ContractVersion.Current,
                requestId,
                WorkerOperationKind.AnalyzeModel,
                GetGoldenStepFilePath(
                    "T001_PRISM.step"));

        WorkerOperationResult result =
            _processor.Process(request);

        Assert.Equal(requestId, result.RequestId);

        Assert.Equal(
            WorkerOperationStatus.Succeeded,
            result.Status);

        Assert.Null(result.OutputArtifactPath);
        Assert.Empty(result.Diagnostics);

        WorkerOperationSummary summary =
            Assert.IsType<WorkerOperationSummary>(
                result.Summary);

        Assert.Equal("#63", summary.SourceSolidEntityId);
        Assert.Equal(1, summary.SolidCount);
        Assert.Equal(1, summary.OuterShellCount);
        Assert.Equal(0, summary.InnerShellCount);
        Assert.Equal(6, summary.FaceCount);
        Assert.Equal(6, summary.LoopCount);
        Assert.Equal(12, summary.EdgeCount);
        Assert.Equal(8, summary.VertexCount);
    }

    [Fact]
    public void Process_MissingStepFile_ReturnsDeterministicFailure()
    {
        string requestId =
            Guid.NewGuid().ToString("D");

        var request =
            new WorkerOperationRequest(
                ContractVersion.Current,
                requestId,
                WorkerOperationKind.AnalyzeModel,
                Path.Combine(
                    AppContext.BaseDirectory,
                    "missing-input.step"));

        WorkerOperationResult result =
            _processor.Process(request);

        Assert.Equal(requestId, result.RequestId);

        Assert.Equal(
            WorkerOperationStatus.Failed,
            result.Status);

        Assert.Null(result.OutputArtifactPath);
        Assert.Null(result.Summary);
        Assert.NotEmpty(result.Diagnostics);
    }

    private static string GetGoldenStepFilePath(
        string fileName)
    {
        string filePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "Step",
                "Golden",
                fileName);

        Assert.True(
            File.Exists(filePath),
            $"Golden STEP file was not found: '{filePath}'.");

        return filePath;
    }
}
