using System.Security.Cryptography;
using MPC.Application.Manufacturing;

namespace MPC.Application.Tests.Manufacturing;

public sealed class ManufacturingRegionContractTests
{
    [Fact]
    public void GoldenPrism_SourceFaceCreatesRegionWithExactDocumentHash()
    {
        string file = Golden("T001_PRISM.step");
        string expectedHash = Convert.ToHexString(SHA256.HashData(
            File.ReadAllBytes(file)));
        foreach (string sourceId in new[]
            { "#8", "#30", "#62", "#76", "#143", "#174" })
        {
            Assert.True(ManufacturingRegionImportService.TryCreate(
                file, sourceId, out var region));
            Assert.NotNull(region);
            Assert.Equal(sourceId, region.SourceFaceId);
            Assert.Equal(expectedHash, region.DocumentFingerprint);
            Assert.True(region.OuterBoundaryEdgeCount >= 3);
            Assert.Equal(0, region.InnerBoundaryCount);
        }
        Assert.False(ManufacturingRegionImportService.TryCreate(
            file, "#999999", out _));
    }

    [Fact]
    public void ContentChange_ChangesFingerprint_AndSessionFaceIdIsTransient()
    {
        string original = Golden("T001_PRISM.step");
        string copy = Path.Combine(Path.GetTempPath(),
            Guid.NewGuid().ToString("N") + ".step");
        try
        {
            File.Copy(original, copy);
            Assert.True(ManufacturingRegionImportService.TryCreate(
                copy, "#8", out var first));
            Assert.True(ManufacturingRegionImportService.TryCreate(
                copy, "#8", out var again));
            Assert.Equal(first!.DocumentFingerprint, again!.DocumentFingerprint);
            Assert.NotEqual(first.CanonicalFaceId, again.CanonicalFaceId);

            // Legal harmless STEP whitespace nevertheless identifies changed bytes.
            File.AppendAllText(copy, Environment.NewLine);
            Assert.True(ManufacturingRegionImportService.TryCreate(
                copy, "#8", out var changed));
            Assert.NotEqual(first.DocumentFingerprint, changed!.DocumentFingerprint);
        }
        finally
        {
            File.Delete(copy);
        }
    }

    [Fact]
    public void NoFileOrUnknownFace_DoesNotProduceRegion()
    {
        Assert.False(ManufacturingRegionImportService.TryCreate(
            "", "#8", out _));
        Assert.False(ManufacturingRegionImportService.TryCreate(
            Golden("T001_PRISM.step"), "", out _));
    }

    private static string Golden(string filename)
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName,
                "TestData", "Step", "Golden", filename);
            if (File.Exists(candidate)) return candidate;
            candidate = Path.Combine(current.FullName, "tests",
                "MPC.Tests", "TestData", "Step", "Golden", filename);
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new FileNotFoundException(filename);
    }
}
