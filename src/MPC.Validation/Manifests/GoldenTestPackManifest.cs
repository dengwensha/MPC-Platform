namespace MPC.Validation.Manifests;

public sealed class GoldenTestPackManifest
{
    private readonly IReadOnlyList<ModelManifest> _models;

    public string PackName { get; }

    public string ManifestVersion { get; }

    public IReadOnlyList<ModelManifest> Models => _models;

    public int TotalModelCount => _models.Count;

    public int TotalSurfaceCount =>
        _models.Sum(
            model => model.SurfaceStatistics.TotalCount);

    public int TotalCurveCount =>
        _models.Sum(
            model => model.CurveStatistics.TotalCount);

    public int TotalPrimitiveCount =>
        TotalSurfaceCount + TotalCurveCount;

    public GoldenTestPackManifest(
        string packName,
        string manifestVersion,
        IEnumerable<ModelManifest> models)
    {
        if (string.IsNullOrWhiteSpace(packName))
        {
            throw new ArgumentException(
                "Pack name cannot be null, empty, or whitespace.",
                nameof(packName));
        }

        if (string.IsNullOrWhiteSpace(manifestVersion))
        {
            throw new ArgumentException(
                "Manifest version cannot be null, empty, or whitespace.",
                nameof(manifestVersion));
        }

        ArgumentNullException.ThrowIfNull(models);

        var modelList = models.ToList();

        if (modelList.Any(model => model is null))
        {
            throw new ArgumentException(
                "Models cannot contain null entries.",
                nameof(models));
        }

        EnsureUniqueSourceFileNames(modelList);

        PackName = packName.Trim();
        ManifestVersion = manifestVersion.Trim();
        _models = modelList.AsReadOnly();
    }

    private static void EnsureUniqueSourceFileNames(
        IEnumerable<ModelManifest> models)
    {
        var duplicate = models
            .GroupBy(
                model => model.SourceFileName,
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Duplicate source file name detected: " +
                $"'{duplicate.Key}'.",
                nameof(models));
        }
    }
}