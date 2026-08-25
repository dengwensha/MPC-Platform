using System;
using System.Text.Json;
using MPC.Contracts;

namespace MPC.Contracts.Tests;

public sealed class WorkerOperationSummaryTests
{
    [Fact]
    public void Constructor_PreservesCanonicalCounts()
    {
        var summary =
            CreateSummary();

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
    public void Constructor_RejectsNegativeCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WorkerOperationSummary(
                "#63",
                solidCount: 1,
                outerShellCount: 1,
                innerShellCount: 0,
                faceCount: -1,
                loopCount: 6,
                edgeCount: 12,
                vertexCount: 8));
    }

    [Fact]
    public void ResultJsonRoundTrip_PreservesSummary()
    {
        var result =
            new WorkerOperationResult(
                ContractVersion.Current,
                Guid.NewGuid().ToString("D"),
                WorkerOperationStatus.Succeeded,
                outputArtifactPath: null,
                Array.Empty<ContractDiagnostic>(),
                CreateSummary());

        string json =
            JsonSerializer.Serialize(result);

        WorkerOperationResult? restored =
            JsonSerializer.Deserialize<WorkerOperationResult>(
                json);

        Assert.NotNull(restored);

        WorkerOperationSummary summary =
            Assert.IsType<WorkerOperationSummary>(
                restored.Summary);

        Assert.Equal("#63", summary.SourceSolidEntityId);
        Assert.Equal(1, summary.SolidCount);
        Assert.Equal(1, summary.OuterShellCount);
        Assert.Equal(0, summary.InnerShellCount);
        Assert.Equal(6, summary.FaceCount);
        Assert.Equal(6, summary.LoopCount);
        Assert.Equal(12, summary.EdgeCount);
        Assert.Equal(8, summary.VertexCount);
    }

    private static WorkerOperationSummary CreateSummary()
    {
        return new WorkerOperationSummary(
            "#63",
            solidCount: 1,
            outerShellCount: 1,
            innerShellCount: 0,
            faceCount: 6,
            loopCount: 6,
            edgeCount: 12,
            vertexCount: 8);
    }
}
