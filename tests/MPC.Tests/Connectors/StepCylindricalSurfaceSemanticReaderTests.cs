using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepCylindricalSurfaceSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateCylindricalSurfaceSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#100,12.5);");

        var result =
            reader.Read(
                record);

        var cylindricalSurface =
            Assert.IsType<StepCylindricalSurfaceEntity>(
                result);

        Assert.Equal(
            "#140",
            cylindricalSurface.SourceEntityId);

        Assert.Equal(
            "CYLINDRICAL_SURFACE",
            cylindricalSurface.SourceEntityType);

        Assert.Equal(
            "Hole Surface",
            cylindricalSurface.Name);

        Assert.Equal(
            "#100",
            cylindricalSurface.PlacementReferenceId);

        Assert.Equal(
            12.5,
            cylindricalSurface.Radius);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('',#100,12);");

        var result =
            reader.Read(
                record);

        var cylindricalSurface =
            Assert.IsType<StepCylindricalSurfaceEntity>(
                result);

        Assert.Equal(
            12.0,
            cylindricalSurface.Radius);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('',#100,12.5);");

        var result =
            reader.Read(
                record);

        var cylindricalSurface =
            Assert.IsType<StepCylindricalSurfaceEntity>(
                result);

        Assert.Equal(
            string.Empty,
            cylindricalSurface.Name);
    }

    [Fact]
    public void Read_ShouldPreservePlacementReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#987,12.5);");

        var result =
            reader.Read(
                record);

        var cylindricalSurface =
            Assert.IsType<StepCylindricalSurfaceEntity>(
                result);

        Assert.Equal(
            "#987",
            cylindricalSurface.PlacementReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenRadiusIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#100);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenExtraParameterExists()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#100,12.5,#101);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNameThatIsNotString()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE(#1,#100,12.5);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullPlacementReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',$,12.5);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectPlacementThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',100,12.5);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectRadiusThatIsNotNumeric()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#100,'12.5');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectZeroRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#100,0.0);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNegativeRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#140=CYLINDRICAL_SURFACE('Hole Surface',#100,-12.5);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    private static StepSemanticEntityReader CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepCylindricalSurfaceEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#140",
            sourceEntityType:
                "CYLINDRICAL_SURFACE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}