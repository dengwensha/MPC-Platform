using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepSphericalSurfaceSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateSphericalSurfaceSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#160=SPHERICAL_SURFACE('Sphere Surface',#100,25.5);");

        var result =
            reader.Read(
                record);

        var sphericalSurface =
            Assert.IsType<StepSphericalSurfaceEntity>(
                result);

        Assert.Equal(
            "#160",
            sphericalSurface.SourceEntityId);

        Assert.Equal(
            "SPHERICAL_SURFACE",
            sphericalSurface.SourceEntityType);

        Assert.Equal(
            "Sphere Surface",
            sphericalSurface.Name);

        Assert.Equal(
            "#100",
            sphericalSurface.PlacementReferenceId);

        Assert.Equal(
            25.5,
            sphericalSurface.Radius);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#160=SPHERICAL_SURFACE('',#100,25);");

        var result =
            reader.Read(
                record);

        var sphericalSurface =
            Assert.IsType<StepSphericalSurfaceEntity>(
                result);

        Assert.Equal(
            25.0,
            sphericalSurface.Radius);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#160=SPHERICAL_SURFACE('',#100,25.5);");

        var result =
            reader.Read(
                record);

        var sphericalSurface =
            Assert.IsType<StepSphericalSurfaceEntity>(
                result);

        Assert.Equal(
            string.Empty,
            sphericalSurface.Name);
    }

    [Fact]
    public void Read_ShouldPreservePlacementReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#160=SPHERICAL_SURFACE('Sphere',#987,25.5);");

        var result =
            reader.Read(
                record);

        var sphericalSurface =
            Assert.IsType<StepSphericalSurfaceEntity>(
                result);

        Assert.Equal(
            "#987",
            sphericalSurface.PlacementReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenRadiusIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#160=SPHERICAL_SURFACE('Sphere',#100);");

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
                "#160=SPHERICAL_SURFACE('Sphere',#100,25.5,#101);");

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
                "#160=SPHERICAL_SURFACE(#1,#100,25.5);");

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
                "#160=SPHERICAL_SURFACE('Sphere',$,25.5);");

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
                "#160=SPHERICAL_SURFACE('Sphere',100,25.5);");

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
                "#160=SPHERICAL_SURFACE('Sphere',#100,'25.5');");

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
                "#160=SPHERICAL_SURFACE('Sphere',#100,0.0);");

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
                "#160=SPHERICAL_SURFACE('Sphere',#100,-25.5);");

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
                    new StepSphericalSurfaceEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#160",
            sourceEntityType:
                "SPHERICAL_SURFACE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}