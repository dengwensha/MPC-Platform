using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepToroidalSurfaceSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateToroidalSurfaceSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus Surface',#100,50.5,10.25);");

        var result =
            reader.Read(
                record);

        var toroidalSurface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            "#170",
            toroidalSurface.SourceEntityId);

        Assert.Equal(
            "TOROIDAL_SURFACE",
            toroidalSurface.SourceEntityType);

        Assert.Equal(
            "Torus Surface",
            toroidalSurface.Name);

        Assert.Equal(
            "#100",
            toroidalSurface.PlacementReferenceId);

        Assert.Equal(
            50.5,
            toroidalSurface.MajorRadius);

        Assert.Equal(
            10.25,
            toroidalSurface.MinorRadius);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerRadii()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('',#100,50,10);");

        var result =
            reader.Read(
                record);

        var toroidalSurface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            50.0,
            toroidalSurface.MajorRadius);

        Assert.Equal(
            10.0,
            toroidalSurface.MinorRadius);
    }

    [Fact]
    public void Read_ShouldAcceptMixedIntegerAndRealRadii()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,50,10.5);");

        var result =
            reader.Read(
                record);

        var toroidalSurface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            50.0,
            toroidalSurface.MajorRadius);

        Assert.Equal(
            10.5,
            toroidalSurface.MinorRadius);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('',#100,50.0,10.0);");

        var result =
            reader.Read(
                record);

        var toroidalSurface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            string.Empty,
            toroidalSurface.Name);
    }

    [Fact]
    public void Read_ShouldPreservePlacementReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#987,50.0,10.0);");

        var result =
            reader.Read(
                record);

        var toroidalSurface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            "#987",
            toroidalSurface.PlacementReferenceId);
    }

    [Fact]
    public void Read_ShouldAcceptMajorRadiusSmallerThanMinorRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,5.0,10.0);");

        var result =
            reader.Read(
                record);

        var toroidalSurface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            5.0,
            toroidalSurface.MajorRadius);

        Assert.Equal(
            10.0,
            toroidalSurface.MinorRadius);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenMinorRadiusIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,50.0);");

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
                "#170=TOROIDAL_SURFACE('Torus',#100,50.0,10.0,#101);");

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
                "#170=TOROIDAL_SURFACE(#1,#100,50.0,10.0);");

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
                "#170=TOROIDAL_SURFACE('Torus',$,50.0,10.0);");

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
                "#170=TOROIDAL_SURFACE('Torus',100,50.0,10.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectMajorRadiusThatIsNotNumeric()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,'50.0',10.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectMinorRadiusThatIsNotNumeric()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,50.0,'10.0');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectZeroMajorRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,0.0,10.0);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNegativeMajorRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,-50.0,10.0);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectZeroMinorRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,50.0,0.0);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNegativeMinorRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#170=TOROIDAL_SURFACE('Torus',#100,50.0,-10.0);");

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
                    new StepToroidalSurfaceEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#170",
            sourceEntityType:
                "TOROIDAL_SURFACE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}