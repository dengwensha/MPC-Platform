namespace MPC.Validation.Manifests;

public sealed class ModelManifest
{
    public string ModelName { get; }

    public string SourceFileName { get; }

    public PrimitiveStatistics SurfaceStatistics { get; }

    public PrimitiveStatistics CurveStatistics { get; }

    public int TotalPrimitiveCount =>
        SurfaceStatistics.TotalCount +
        CurveStatistics.TotalCount;

    public ModelManifest(
        string modelName,
        string sourceFileName,
        PrimitiveStatistics surfaceStatistics,
        PrimitiveStatistics curveStatistics)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException(
                "Model name cannot be null, empty, or whitespace.",
                nameof(modelName));
        }

        if (string.IsNullOrWhiteSpace(sourceFileName))
        {
            throw new ArgumentException(
                "Source file name cannot be null, empty, or whitespace.",
                nameof(sourceFileName));
        }

        ModelName = modelName.Trim();
        SourceFileName = sourceFileName.Trim();

        SurfaceStatistics = surfaceStatistics ??
            throw new ArgumentNullException(
                nameof(surfaceStatistics));

        CurveStatistics = curveStatistics ??
            throw new ArgumentNullException(
                nameof(curveStatistics));
    }
}