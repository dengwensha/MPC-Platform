using MPC.Connectors.Snapshots.Units;

namespace MPC.Tests.Connectors;

public sealed class CadUnitInfoTests
{
    [Fact]
    public void Constructor_WithMillimetreAndDegree_PreservesSourceUnits()
    {
        var units = new CadUnitInfo(
            sourceLengthUnit: CadLengthUnit.Millimetre,
            sourceAngleUnit: CadAngleUnit.Degree);

        Assert.Equal(
            CadLengthUnit.Millimetre,
            units.SourceLengthUnit);

        Assert.Equal(
            CadAngleUnit.Degree,
            units.SourceAngleUnit);
    }

    [Fact]
    public void InternalUnits_AreAlwaysMetreAndRadian()
    {
        var units = new CadUnitInfo(
            sourceLengthUnit: CadLengthUnit.Inch,
            sourceAngleUnit: CadAngleUnit.Degree);

        Assert.Equal(
            CadLengthUnit.Metre,
            units.InternalLengthUnit);

        Assert.Equal(
            CadAngleUnit.Radian,
            units.InternalAngleUnit);
    }

    [Fact]
    public void Constructor_AllowsUnknownSourceUnits()
    {
        var units = new CadUnitInfo(
            sourceLengthUnit: CadLengthUnit.Unknown,
            sourceAngleUnit: CadAngleUnit.Unknown);

        Assert.Equal(
            CadLengthUnit.Unknown,
            units.SourceLengthUnit);

        Assert.Equal(
            CadAngleUnit.Unknown,
            units.SourceAngleUnit);
    }

    [Fact]
    public void Constructor_WithUndefinedLengthUnit_ThrowsArgumentOutOfRangeException()
    {
        var undefinedUnit =
            (CadLengthUnit)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CadUnitInfo(
                sourceLengthUnit: undefinedUnit,
                sourceAngleUnit: CadAngleUnit.Radian));
    }

    [Fact]
    public void Constructor_WithUndefinedAngleUnit_ThrowsArgumentOutOfRangeException()
    {
        var undefinedUnit =
            (CadAngleUnit)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CadUnitInfo(
                sourceLengthUnit: CadLengthUnit.Metre,
                sourceAngleUnit: undefinedUnit));
    }
}
