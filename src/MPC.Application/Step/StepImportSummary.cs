namespace MPC.Application.Step;

public sealed class StepImportSummary
{
    public string SourceSolidEntityId { get; }

    public int SolidCount { get; }

    public int OuterShellCount { get; }

    public int InnerShellCount { get; }

    public int FaceCount { get; }

    public int LoopCount { get; }

    public int EdgeCount { get; }

    public int VertexCount { get; }

    public StepImportSummary(
        string sourceSolidEntityId,
        int solidCount,
        int outerShellCount,
        int innerShellCount,
        int faceCount,
        int loopCount,
        int edgeCount,
        int vertexCount)
    {
        if (string.IsNullOrWhiteSpace(sourceSolidEntityId))
        {
            throw new ArgumentException(
                "Source solid entity identifier cannot be null, empty, or whitespace.",
                nameof(sourceSolidEntityId));
        }

        EnsureNonNegative(solidCount, nameof(solidCount));
        EnsureNonNegative(outerShellCount, nameof(outerShellCount));
        EnsureNonNegative(innerShellCount, nameof(innerShellCount));
        EnsureNonNegative(faceCount, nameof(faceCount));
        EnsureNonNegative(loopCount, nameof(loopCount));
        EnsureNonNegative(edgeCount, nameof(edgeCount));
        EnsureNonNegative(vertexCount, nameof(vertexCount));

        SourceSolidEntityId = sourceSolidEntityId.Trim();
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
                "STEP import summary counts cannot be negative.");
        }
    }
}
