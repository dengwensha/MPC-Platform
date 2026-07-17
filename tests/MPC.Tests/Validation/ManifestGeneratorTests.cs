using MPC.Validation.Contracts;
using MPC.Validation.Generation;
using MPC.Validation.Manifests;

namespace MPC.Tests.Validation;

public sealed class ManifestGeneratorTests
{
    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ManifestGenerator(null!));
    }

    [Fact]
    public void Generate_WithValidSource_CreatesGoldenTestPackManifest()
    {
        var source = new TestManifestSource(
            new[]
            {
                CreateModelManifest(
                    "Plane Model",
                    "PlaneModel.SLDPRT"),

                CreateModelManifest(
                    "Cylinder Model",
                    "CylinderModel.SLDPRT")
            });

        var generator = new ManifestGenerator(source);

        var manifest = generator.Generate(
            "Geometry Foundation V1 Golden Test Pack",
            "1.0");

        Assert.Equal(
            "Geometry Foundation V1 Golden Test Pack",
            manifest.PackName);

        Assert.Equal("1.0", manifest.ManifestVersion);
        Assert.Equal(2, manifest.TotalModelCount);
        Assert.Equal(2, manifest.TotalSurfaceCount);
        Assert.Equal(8, manifest.TotalCurveCount);
    }

    [Fact]
    public void Generate_WithEmptySource_CreatesEmptyManifest()
    {
        var source = new TestManifestSource(
            Array.Empty<ModelManifest>());

        var generator = new ManifestGenerator(source);

        var manifest = generator.Generate(
            "Empty Golden Pack",
            "1.0");

        Assert.Empty(manifest.Models);
        Assert.Equal(0, manifest.TotalModelCount);
        Assert.Equal(0, manifest.TotalPrimitiveCount);
    }

    [Fact]
    public void Generate_WhenSourceReturnsNull_ThrowsInvalidOperationException()
    {
        var source = new NullManifestSource();

        var generator = new ManifestGenerator(source);

        Assert.Throws<InvalidOperationException>(
            () => generator.Generate(
                "Invalid Pack",
                "1.0"));
    }

    private static ModelManifest CreateModelManifest(
        string modelName,
        string sourceFileName)
    {
        return new ModelManifest(
            modelName,
            sourceFileName,
            new PrimitiveStatistics(
                new Dictionary<string, int>
                {
                    ["Plane"] = 1
                }),
            new PrimitiveStatistics(
                new Dictionary<string, int>
                {
                    ["Line"] = 4
                }));
    }

    private sealed class TestManifestSource
        : IModelManifestSource
    {
        private readonly IReadOnlyList<ModelManifest> _models;

        public TestManifestSource(
            IReadOnlyList<ModelManifest> models)
        {
            _models = models;
        }

        public IReadOnlyList<ModelManifest> ReadModels()
        {
            return _models;
        }
    }

    private sealed class NullManifestSource
        : IModelManifestSource
    {
        public IReadOnlyList<ModelManifest> ReadModels()
        {
            return null!;
        }
    }
}