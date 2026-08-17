using System;
using MPC.Contracts;

namespace MPC.Worker.Tests;

public sealed class WorkerRequestProcessorTests
{
    private readonly WorkerRequestProcessor _processor = new();

    [Fact]
    public void Process_RejectsInvalidContractRequest()
    {
        var request = new WorkerOperationRequest(
            "2.0",
            "invalid-request-id",
            (WorkerOperationKind)999,
            " ");

        var result = _processor.Process(request);

        Assert.Equal(
            ContractVersion.Current,
            result.ContractVersion);

        Assert.Equal(
            WorkerOperationStatus.Rejected,
            result.Status);

        Assert.Equal(
            4,
            result.Diagnostics.Count);

        Assert.Null(
            result.OutputArtifactPath);
    }

    [Fact]
    public void Process_RejectsNullRequest()
    {
        var result =
            _processor.Process(null);

        var diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerOperationStatus.Rejected,
            result.Status);

        Assert.Equal(
            "CONTRACT-REQUEST-001",
            diagnostic.Code);
    }

    [Fact]
    public void Process_DoesNotReportFalseSuccess_ForValidRequest()
    {
        var requestId =
            Guid.NewGuid().ToString("D");

        var request = new WorkerOperationRequest(
            ContractVersion.Current,
            requestId,
            WorkerOperationKind.AnalyzeModel,
            @"C:\MPC\input.model");

        var result =
            _processor.Process(request);

        var diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            requestId,
            result.RequestId);

        Assert.Equal(
            WorkerOperationStatus.Failed,
            result.Status);

        Assert.Equal(
            WorkerRequestProcessor.OperationNotImplementedCode,
            diagnostic.Code);

        Assert.Null(
            result.OutputArtifactPath);
    }
}