using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepDirectionSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateDirectionSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('Z Axis',(0.0,0,1.0));");

        var result =
            reader.Read(
                record);

        var direction =
            Assert.IsType<StepDirectionEntity>(
                result);

        Assert.Equal(
            "#20",
            direction.SourceEntityId);

        Assert.Equal(
            "DIRECTION",
            direction.SourceEntityType);

        Assert.Equal(
            "Z Axis",
            direction.Name);

        Assert.Equal(
            new[]
            {
                0.0,
                0.0,
                1.0
            },
            direction.Ratios);
    }

    [Fact]
    public void Read_ShouldPreserveTwoDimensionalDirection()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('',(1.0,0.0));");

        var result =
            reader.Read(
                record);

        var direction =
            Assert.IsType<StepDirectionEntity>(
                result);

        Assert.Equal(
            2,
            direction.Ratios.Count);

        Assert.Equal(
            1.0,
            direction.Ratios[0]);

        Assert.Equal(
            0.0,
            direction.Ratios[1]);
    }

    [Fact]
    public void Read_ShouldPreserveDirectionRatiosWithoutNormalization()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('',(10.0,0.0,0.0));");

        var result =
            reader.Read(
                record);

        var direction =
            Assert.IsType<StepDirectionEntity>(
                result);

        Assert.Equal(
            10.0,
            direction.Ratios[0]);

        Assert.Equal(
            0.0,
            direction.Ratios[1]);

        Assert.Equal(
            0.0,
            direction.Ratios[2]);
    }

    [Fact]
    public void Read_ShouldPreserveZeroDirectionRatios()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('',(0.0,0.0,0.0));");

        var result =
            reader.Read(
                record);

        var direction =
            Assert.IsType<StepDirectionEntity>(
                result);

        Assert.Equal(
            new[]
            {
                0.0,
                0.0,
                0.0
            },
            direction.Ratios);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('Z Axis');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectRatioParameterThatIsNotAggregate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('',1.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectEmptyRatioAggregate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('',());");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNonNumericRatio()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#20=DIRECTION('',(#10));");

        Assert.Throws<FormatException>(
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
                    new StepDirectionEntityHandler()
                });
    }

    private static StepDataEntityRecord
        CreateRecord(
            string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#20",
            sourceEntityType:
                "DIRECTION",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}