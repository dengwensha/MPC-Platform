using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepCartesianPointSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateCartesianPointSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('',(1.0,2,3.0));");

        var result =
            reader.Read(
                record);

        var point =
            Assert.IsType<
                StepCartesianPointEntity>(
                result);

        Assert.Equal(
            "#15",
            point.SourceEntityId);

        Assert.Equal(
            "CARTESIAN_POINT",
            point.SourceEntityType);

        Assert.Equal(
            string.Empty,
            point.Name);

        Assert.Equal(
            new[]
            {
                1.0,
                2.0,
                3.0
            },
            point.Coordinates);
    }

    [Fact]
    public void Read_ShouldPreserveCartesianPointName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('Origin',(0,0,0));");

        var result =
            reader.Read(
                record);

        var point =
            Assert.IsType<
                StepCartesianPointEntity>(
                result);

        Assert.Equal(
            "Origin",
            point.Name);
    }

    [Fact]
    public void Read_ShouldPreserveCoordinateDimension()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('',(10.0,20.0));");

        var result =
            reader.Read(
                record);

        var point =
            Assert.IsType<
                StepCartesianPointEntity>(
                result);

        Assert.Equal(
            2,
            point.Coordinates.Count);

        Assert.Equal(
            10.0,
            point.Coordinates[0]);

        Assert.Equal(
            20.0,
            point.Coordinates[1]);
    }

    [Fact]
    public void Read_ShouldReturnNullForUnsupportedEntityType()
    {
        var reader =
            CreateReader();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#20",
                sourceEntityType:
                    "LINE",
                rawStatement:
                    "#20=LINE('',#1,#2);",
                readStatus:
                    CadReadStatus.Complete);

        var result =
            reader.Read(
                record);

        Assert.Null(
            result);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectCoordinateParameterThatIsNotAggregate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('',1.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectEmptyCoordinateAggregate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('',());");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNonNumericCoordinate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#15=CARTESIAN_POINT('',(#10));");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Constructor_ShouldRejectDuplicateHandlers()
    {
        var parser =
            new StepParameterParser();

        var handlers =
            new IStepSemanticEntityHandler[]
            {
                new StepCartesianPointEntityHandler(),
                new StepCartesianPointEntityHandler()
            };

        Assert.Throws<InvalidOperationException>(
            () =>
                new StepSemanticEntityReader(
                    parser,
                    handlers));
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
                    new StepCartesianPointEntityHandler()
                });
    }

    private static StepDataEntityRecord
        CreateRecord(
            string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#15",
            sourceEntityType:
                "CARTESIAN_POINT",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}