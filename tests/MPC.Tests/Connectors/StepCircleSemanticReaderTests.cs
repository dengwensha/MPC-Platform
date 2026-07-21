using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepCircleSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateCircleSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#80=CIRCLE('Outer Circle',#100,25.5);");

        var result =
            reader.Read(
                record);

        var circle =
            Assert.IsType<StepCircleEntity>(
                result);

        Assert.Equal(
            "#80",
            circle.SourceEntityId);

        Assert.Equal(
            "CIRCLE",
            circle.SourceEntityType);

        Assert.Equal(
            "Outer Circle",
            circle.Name);

        Assert.Equal(
            "#100",
            circle.PlacementReferenceId);

        Assert.Equal(
            25.5,
            circle.Radius);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerRadius()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#80=CIRCLE('',#100,25);");

        var result =
            reader.Read(
                record);

        var circle =
            Assert.IsType<StepCircleEntity>(
                result);

        Assert.Equal(
            25.0,
            circle.Radius);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#80=CIRCLE('',#100,25.0);");

        var result =
            reader.Read(
                record);

        var circle =
            Assert.IsType<StepCircleEntity>(
                result);

        Assert.Equal(
            string.Empty,
            circle.Name);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#80=CIRCLE('',#100);");

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
                "#80=CIRCLE(#1,#100,25.0);");

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
                "#80=CIRCLE('',$,25.0);");

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
                "#80=CIRCLE('',100,25.0);");

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
                "#80=CIRCLE('',#100,'25.0');");

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
                "#80=CIRCLE('',#100,0.0);");

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
                "#80=CIRCLE('',#100,-5.0);");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                reader.Read(
                    record));
    }

    private static StepSemanticEntityReader
        CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepCircleEntityHandler()
                });
    }

    private static StepDataEntityRecord
        CreateRecord(
            string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#80",
            sourceEntityType:
                "CIRCLE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}