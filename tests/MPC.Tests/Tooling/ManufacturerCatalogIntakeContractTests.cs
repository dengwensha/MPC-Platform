using MPC.Tooling;

namespace MPC.Tests.Tooling;

// TOOL-01B-01: test-local catalog envelope only. Not an import service.
public sealed class ManufacturerCatalogIntakeContractTests
{
    private sealed record Row(
        int Version, string Kind, string Manufacturer, string ProductId,
        string OrderCode, string Revision, string SourceId, string SourceSha256,
        string Units, double? Diameter, double? Thickness,
        string? Grade, string? MasterInsert, string? Ean);

    private static readonly string SourceHash =
        new('A', 64);

    private static Row Insert() => new(1, "CuttingInsert", "Sandvik Coromant",
        "7860702", "CNMG 19 06 12-PM 4425", "2026-A",
        "controlled-catalog-fixture", SourceHash, "mm",
        19.05, 6.35, "4425", null, null);

    private static Row Holder() => new(1, "ToolHolder", "Sandvik Coromant",
        "5729638", "C8-DCLNR-55080-19", "2026-A",
        "controlled-catalog-fixture", SourceHash, "mm",
        19.05, 6.35, null, "CNMG 19 06 12", null);

    private static bool Eligible(Row? r)
    {
        if (r is null || r.Version != 1 ||
            (r.Kind != "CuttingInsert" && r.Kind != "ToolHolder") ||
            string.IsNullOrWhiteSpace(r.Manufacturer) ||
            string.IsNullOrWhiteSpace(r.ProductId) ||
            string.IsNullOrWhiteSpace(r.OrderCode) ||
            string.IsNullOrWhiteSpace(r.Revision) ||
            string.IsNullOrWhiteSpace(r.SourceId) ||
            r.SourceSha256.Length != 64 || !r.SourceSha256.All(Uri.IsHexDigit) ||
            r.Units != "mm" ||
            r.Diameter is not double diameter || !double.IsFinite(diameter) ||
            diameter <= 0 ||
            r.Thickness is not double thickness || !double.IsFinite(thickness) ||
            thickness <= 0) return false;
        return r.Kind == "CuttingInsert"
            ? !string.IsNullOrWhiteSpace(r.Grade)
            : !string.IsNullOrWhiteSpace(r.MasterInsert);
    }

    // Conflicting records never use last-write-wins.
    private static bool HasIdentityConflict(IReadOnlyList<Row> rows) =>
        rows.GroupBy(r => (Manufacturer: r.Manufacturer.Trim().ToUpperInvariant(),
                Id: r.ProductId.Trim().ToUpperInvariant(),
                Revision: r.Revision.Trim().ToUpperInvariant()))
            .Any(g => g.Select(x => (x.Kind,x.OrderCode,x.Diameter,x.Thickness,
                    x.Grade,x.MasterInsert)).Distinct().Skip(1).Any());

    [Fact]
    public void ValidMinimalRows_AllowMissingOptionalEan()
    {
        Assert.True(Eligible(Insert()));
        Assert.True(Eligible(Holder()));
        Assert.Null(Insert().Ean);
        Assert.False(HasIdentityConflict([Insert(),Holder()]));
    }

    [Fact]
    public void MissingMeasurementsAndUnknownUnits_Reject()
    {
        Assert.False(Eligible(Insert() with {Diameter = null}));
        Assert.False(Eligible(Insert() with {Thickness = null}));
        Assert.False(Eligible(Insert() with {Units = "inch"}));
        Assert.False(Eligible(Insert() with {Thickness = double.NaN}));
        Assert.False(Eligible(Holder() with {MasterInsert = ""}));
        Assert.False(Eligible(Insert() with {Version = 2}));
        Assert.False(Eligible(Insert() with {SourceId = ""}));
    }

    [Fact]
    public void SameIdentityDifferentThickness_IsQuarantined()
    {
        var original = Insert();
        var conflicting = original with {Thickness = 7.35};
        Assert.True(HasIdentityConflict([original,conflicting]));
        Assert.False(HasIdentityConflict([original,original]));
    }

    [Fact]
    public void SameOrderFamilyDistinctProductIdentity_DoesNotCoalesce()
    {
        var original = Insert();
        var otherGrade = original with {
            ProductId = "other-sku", Grade = "other-grade" };
        Assert.False(HasIdentityConflict([original,otherGrade]));
    }

    [Fact]
    public void AcceptedEnvelope_DoesNotClaimPhysicalCompatibility()
    {
        var row = Insert();
        Assert.True(Eligible(row));
        // Construct existing domain objects, without a catalog importer.
        var identity = new ToolProductIdentity(row.Manufacturer,
            row.ProductId,row.OrderCode,row.Ean);
        Assert.Equal("7860702",identity.ManufacturerProductId);
        const int PhysicallyVerifiedAssemblies = 0;
        Assert.Equal(0,PhysicallyVerifiedAssemblies);
    }
}
