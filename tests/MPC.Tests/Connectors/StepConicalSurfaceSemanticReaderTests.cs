using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepConicalSurfaceSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateConicalSurfaceSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('Cone Surface',#100,12.5,0.78539816339);");

        var result =
            reader.Read(
                record);

        var conicalSurface =
            Assert.IsType<StepConicalSurfaceEntity>(
                result);

        Assert.Equal(
            "#150",
            conicalSurface.SourceEntityId);

        Assert.Equal(
            "CONICAL_SURFACE",
            conicalSurface.SourceEntityType);

        Assert.Equal(
            "Cone Surface",
            conicalSurface.Name);

        Assert.Equal(
            "#100",
            conicalSurface.PlacementReferenceId);

        Assert.Equal(
            12.5,
            conicalSurface.Radius);

        Assert.Equal(
            0.78539816339,
            conicalSurface.SemiAngle);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('',#100,12,0.5);");

        var result =
            reader.Read(
                record);

        var conicalSurface =
            Assert.IsType<StepConicalSurfaceEntity>(
                result);

        Assert.Equal(
            12.0,
            conicalSurface.Radius);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerSemiAngle()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('',#100,12.5,1);");

        var result =
            reader.Read(
                record);

        var conicalSurface =
            Assert.IsType<StepConicalSurfaceEntity>(
                result);

        Assert.Equal(
            1.0,
            conicalSurface.SemiAngle);
    }

    [Fact]
    public void Read_ShouldAcceptZeroRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('Apex',#100,0.0,0.5);");

        var result =
            reader.Read(
                record);

        var conicalSurface =
            Assert.IsType<StepConicalSurfaceEntity>(
                result);

        Assert.Equal(
            0.0,
            conicalSurface.Radius);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('',#100,12.5,0.5);");

        var result =
            reader.Read(
                record);

        var conicalSurface =
            Assert.IsType<StepConicalSurfaceEntity>(
                result);

        Assert.Equal(
            string.Empty,
            conicalSurface.Name);
    }

    [Fact]
    public void Read_ShouldPreservePlacementReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('Cone',#987,12.5,0.5);");

        var result =
            reader.Read(
                record);

        var conicalSurface =
            Assert.IsType<StepConicalSurfaceEntity>(
                result);

        Assert.Equal(
            "#987",
            conicalSurface.PlacementReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenSemiAngleIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('Cone',#100,12.5);");

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
                "#150=CONICAL_SURFACE('Cone',#100,12.5,0.5,#101);");

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
                "#150=CONICAL_SURFACE(#1,#100,12.5,0.5);");

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
                "#150=CONICAL_SURFACE('Cone',$,12.5,0.5);");

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
                "#150=CONICAL_SURFACE('Cone',100,12.5,0.5);");

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
                "#150=CONICAL_SURFACE('Cone',#100,'12.5',0.5);");

        Assert.Throws<FormatException>(
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
                "#150=CONICAL_SURFACE('Cone',#100,-12.5,0.5);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectSemiAngleThatIsNotNumeric()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#150=CONICAL_SURFACE('Cone',#100,12.5,'0.5');");

        Assert.Throws<FormatException>(
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
                    new StepConicalSurfaceEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#150",
            sourceEntityType:
                "CONICAL_SURFACE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}