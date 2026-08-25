using System;

namespace MPC.Contracts;

public sealed class WorkerOperationSummary
{
    public string? SourceSolidEntityId { get; }

    public int SolidCount { get; }

    public int OuterShellCount { get; }

    public int InnerShellCount { get; }

    public int FaceCount { get; }

    public int LoopCount { get; }

    public int EdgeCount { get; }

    public int VertexCount { get; }

    public WorkerOperationSummary(
        string? sourceSolidEntityId,
        int solidCount,
        int outerShellCount,
        int innerShellCount,
        int faceCount,
        int loopCount,
        int edgeCount,
        int vertexCount)
    {
        EnsureNonNegative(solidCount, nameof(solidCount));
        EnsureNonNegative(outerShellCount, nameof(outerShellCount));
        EnsureNonNegative(innerShellCount, nameof(innerShellCount));
        EnsureNonNegative(faceCount, nameof(faceCount));
        EnsureNonNegative(loopCount, nameof(loopCount));
        EnsureNonNegative(edgeCount, nameof(edgeCount));
        EnsureNonNegative(vertexCount, nameof(vertexCount));

        SourceSolidEntityId =
            NormalizeOptional(sourceSolidEntityId);

        SolidCount = solidCount;
        OuterShellCount = outerShellCount;
        InnerShellCount = innerShellCount;
        FaceCount = faceCount;
        LoopCount = loopCount;
        EdgeCount = edgeCount;
        VertexCount = vertexCount;
    }

    private static void EnsureNonNegative(
        int value,
        string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Worker operation summary counts cannot be negative.");
        }
    }

    private static string? NormalizeOptional(
        string? value)
    {
        if (value is null ||
            string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
