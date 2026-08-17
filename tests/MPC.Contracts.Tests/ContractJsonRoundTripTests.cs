using System;
using System.Text.Json;
using MPC.Contracts;

namespace MPC.Contracts.Tests;

public sealed class ContractJsonRoundTripTests
{
    [Fact]
    public void Request_RoundTrip_PreservesTransportFields()
    {
        var request = new WorkerOperationRequest(
            ContractVersion.Current,
            Guid.NewGuid().ToString("D"),
            WorkerOperationKind.AnalyzeModel,
            @"C:\MPC\input.model");

        var json = JsonSerializer.Serialize(request);
        var restored =
            JsonSerializer.Deserialize<WorkerOperationRequest>(json);

        Assert.NotNull(restored);
        Assert.Equal(
            request.ContractVersion,
            restored.ContractVersion);
        Assert.Equal(request.RequestId, restored.RequestId);
        Assert.Equal(request.Operation, restored.Operation);
        Assert.Equal(
            request.InputArtifactPath,
            restored.InputArtifactPath);
    }

    [Fact]
    public void Result_RoundTrip_PreservesStatusOutputAndDiagnostics()
    {
        var requestId = Guid.NewGuid().ToString("D");

        var result = new WorkerOperationResult(
            ContractVersion.Current,
            requestId,
            WorkerOperationStatus.Failed,
            @"C:\MPC\result.json",
            new[]
            {
                new ContractDiagnostic(
                    "WORKER-001",
                    ContractDiagnosticSeverity.Error,
                    "Worker operation failed.")
            });

        var json = JsonSerializer.Serialize(result);
        var restored =
            JsonSerializer.Deserialize<WorkerOperationResult>(json);

        Assert.NotNull(restored);
        Assert.Equal(
            ContractVersion.Current,
            restored.ContractVersion);
        Assert.Equal(requestId, restored.RequestId);
        Assert.Equal(
            WorkerOperationStatus.Failed,
            restored.Status);
        Assert.Equal(
            @"C:\MPC\result.json",
            restored.OutputArtifactPath);

        var diagnostic = Assert.Single(restored.Diagnostics);

        Assert.Equal("WORKER-001", diagnostic.Code);
        Assert.Equal(
            ContractDiagnosticSeverity.Error,
            diagnostic.Severity);
        Assert.Equal(
            "Worker operation failed.",
            diagnostic.Message);
    }
}
