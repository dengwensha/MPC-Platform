using MPC.Validation.Contracts;
using MPC.Validation.Manifests;

namespace MPC.Validation.Generation;

public sealed class ManifestGenerator
{
    private readonly IModelManifestSource _source;

    public ManifestGenerator(
        IModelManifestSource source)
    {
        _source = source ??
            throw new ArgumentNullException(nameof(source));
    }

    public GoldenTestPackManifest Generate(
        string packName,
        string manifestVersion)
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

        var models = _source.ReadModels();

        if (models is null)
        {
            throw new InvalidOperationException(
                "Manifest source returned null.");
        }

        return new GoldenTestPackManifest(
            packName,
            manifestVersion,
            models);
    }
}