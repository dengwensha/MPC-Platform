using System;
using MPC.Contracts;
using MPC.Host.SolidWorks.Worker;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Worker;

public sealed class WorkerContractJsonSerializerTests
{
    private readonly WorkerContractJsonSerializer
        _serializer = new WorkerContractJsonSerializer();

    [Fact]
    public void SerializeRequest_PreservesTransportFields()
    {
        string requestId =
            Guid.NewGuid().ToString("D");

        var request =
            new WorkerOperationRequest(
                ContractVersion.Current,
                requestId,
                WorkerOperationKind.AnalyzeModel,
                @"C:\MPC\part.step");

        string json =
            _serializer.SerializeRequest(request);

        Assert.Contains(
            ContractVersion.Current,
            json);

        Assert.Contains(requestId, json);
        Assert.Contains("part.step", json);
    }

    [Fact]
    public void DeserializeResult_PreservesCanonicalSummary()
    {
        var source =
            new WorkerOperationResult(
                ContractVersion.Current,
                Guid.NewGuid().ToString("D"),
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

        string json =
            System.Text.Json.JsonSerializer.Serialize(
                source);

        WorkerOperationResult result =
            _serializer.DeserializeResult(json);

        Assert.Equal(
            WorkerOperationStatus.Succeeded,
            result.Status);

        WorkerOperationSummary summary =
            Assert.IsType<WorkerOperationSummary>(
                result.Summary);

        Assert.Equal("#63", summary.SourceSolidEntityId);
        Assert.Equal(6, summary.FaceCount);
        Assert.Equal(12, summary.EdgeCount);
        Assert.Equal(8, summary.VertexCount);
    }

    [Fact]
    public void DeserializeResult_RejectsWhitespace()
    {
        Assert.Throws<ArgumentException>(
            () => _serializer.DeserializeResult(" "));
    }
}
