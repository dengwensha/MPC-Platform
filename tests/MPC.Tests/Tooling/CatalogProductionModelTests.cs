using MPC.Tooling;
using MPC.Tooling.Compatibility;

namespace MPC.Tests.Tooling;

// TOOL-01C-05: production-model evidence from controlled, synthetic mapped values.
// Not a source parser, unit converter, supplier certification or provenance store.
public sealed class CatalogProductionModelTests
{
    private static ToolInterface Face(double diameter = 19.05, double thickness = 6.35,
        double included = 80, double clearance = 0, int? mounting = null) =>
        new("C", "19", diameter, thickness, included, clearance, mounting);

    [Fact]
    public void ControlledMappedValues_ReachProductionInsertWithoutLosingFields()
    {
        var identity = new ToolProductIdentity("Synthetic manufacturer", "insert-4425-PM",
            "CNMG 19 06 12-PM 4425", " 7323225046701 ");
        var face = Face();
        var grade = new CuttingGrade("4425", "controlled substrate", "CVD", "controlled coating");
        var insert = new CuttingInsert(identity, face, grade, "PM", ToolHand.Neutral);

        Assert.Same(identity, insert.Identity);
        Assert.Same(face, insert.Interface);
        Assert.Same(grade, insert.Grade);
        Assert.Equal("PM", insert.ChipBreakerDesignation);
        Assert.Equal(ToolHand.Neutral, insert.Hand);
        Assert.Equal("insert-4425-PM", insert.Identity.ManufacturerProductId);
        Assert.Equal(" 7323225046701 ", insert.Identity.Ean);
        Assert.Equal(GtinValidationStatus.ValidFormat, insert.Identity.GtinStatus);
        Assert.Equal(19.05, insert.Interface.InscribedCircleDiameterMillimeters);
        Assert.Equal(6.35, insert.Interface.ThicknessMillimeters);
    }

    [Fact]
    public void ControlledHolderFamily_RemainsDistinctFromExactInsertIdentity()
    {
        var holder = new ToolHolder(new ToolProductIdentity("Synthetic", "holder", "H"),
            Face(), "CNMG 190612", ToolHand.Right);
        var insert = new CuttingInsert(new ToolProductIdentity("Synthetic", "insert", "I"),
            Face(), new CuttingGrade("4425", "controlled"), "PM", ToolHand.Neutral);

        Assert.Equal("CNMG 190612", holder.MasterInsertIdentification);
        Assert.NotEqual(holder.Identity.ManufacturerProductId, insert.Identity.ManufacturerProductId);
        var result = new InsertHolderCompatibilityEvaluator().Evaluate(holder, insert);
        Assert.False(result.IsCompatible);
        Assert.Equal(CompatibilityCode.UnmappedMountingStyle, result.Code);
        Assert.Equal(CompatibilityScope.InterfaceOnly, result.Scope);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidMappedDimensions_CannotConstructProductionInterface(double invalid)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Face(diameter: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => Face(thickness: invalid));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(180)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidIncludedAngles_CannotConstructProductionInterface(double invalid) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Face(included: invalid));

    [Theory]
    [InlineData(-1)]
    [InlineData(90)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidClearanceAngles_CannotConstructProductionInterface(double invalid) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Face(clearance: invalid));

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(99)]
    public void UndefinedMappedHand_CannotConstructProductionProduct(int raw)
    {
        var id = new ToolProductIdentity("Synthetic", "product", "P");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ToolHolder(id, Face(), "family", (ToolHand)raw));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CuttingInsert(id, Face(), new CuttingGrade("grade", "substrate"), null, (ToolHand)raw));
    }

    [Fact]
    public void MissingOptionalFields_DoNotInventGradeCoatingOrChipbreaker()
    {
        var grade = new CuttingGrade("grade", "substrate");
        var insert = new CuttingInsert(new ToolProductIdentity("Synthetic", "insert", "I"),
            Face(), grade, null, ToolHand.Unknown);
        Assert.Null(grade.Coating);
        Assert.Null(grade.CoatingProcess);
        Assert.Null(insert.ChipBreakerDesignation);
        Assert.Null(insert.Identity.Ean);
        Assert.Equal(GtinValidationStatus.NotProvided, insert.Identity.GtinStatus);
        var holder = new ToolHolder(new ToolProductIdentity("Synthetic", "holder", "H"),
            Face(), "family", ToolHand.Right);
        Assert.Equal(CompatibilityCode.UnknownHand,
            new InsertHolderCompatibilityEvaluator().Evaluate(holder, insert).Code);
    }
}
