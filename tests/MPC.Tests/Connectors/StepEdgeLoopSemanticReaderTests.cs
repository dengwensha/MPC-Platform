using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepEdgeLoopSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateEdgeLoopSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('Loop 1',(#210,#211,#212));");

        var result =
            reader.Read(
                record);

        var edgeLoop =
            Assert.IsType<StepEdgeLoopEntity>(
                result);

        Assert.Equal(
            "#300",
            edgeLoop.SourceEntityId);

        Assert.Equal(
            "EDGE_LOOP",
            edgeLoop.SourceEntityType);

        Assert.Equal(
            "Loop 1",
            edgeLoop.Name);

        Assert.Equal(
            new[]
            {
                "#210",
                "#211",
                "#212"
            },
            edgeLoop.OrientedEdgeReferenceIds);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210,#211,#212));");

        var result =
            reader.Read(
                record);

        var edgeLoop =
            Assert.IsType<StepEdgeLoopEntity>(
                result);

        Assert.Equal(
            string.Empty,
            edgeLoop.Name);
    }

    [Fact]
    public void Read_ShouldPreserveOrientedEdgeReferenceOrder()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#900,#100,#500));");

        var result =
            reader.Read(
                record);

        var edgeLoop =
            Assert.IsType<StepEdgeLoopEntity>(
                result);

        Assert.Equal(
            new[]
            {
                "#900",
                "#100",
                "#500"
            },
            edgeLoop.OrientedEdgeReferenceIds);
    }

    [Fact]
    public void Read_ShouldPreserveDuplicateReferences()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210,#211,#210));");

        var result =
            reader.Read(
                record);

        var edgeLoop =
            Assert.IsType<StepEdgeLoopEntity>(
                result);

        Assert.Equal(
            new[]
            {
                "#210",
                "#211",
                "#210"
            },
            edgeLoop.OrientedEdgeReferenceIds);
    }

    [Fact]
    public void Read_ShouldAcceptSingleOrientedEdgeReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210));");

        var result =
            reader.Read(
                record);

        var edgeLoop =
            Assert.IsType<StepEdgeLoopEntity>(
                result);

        Assert.Single(
            edgeLoop.OrientedEdgeReferenceIds);

        Assert.Equal(
            "#210",
            edgeLoop.OrientedEdgeReferenceIds[0]);
    }

    [Fact]
    public void Read_ShouldRejectEmptyOrientedEdgeAggregate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',());");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenAggregateIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('Loop 1');");

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
                "#300=EDGE_LOOP('Loop 1',(#210),#999);");

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
                "#300=EDGE_LOOP(#1,(#210,#211));");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectSecondParameterThatIsNotAggregate()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',#210);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectAggregateItemThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210,211,#212));");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullAggregateItem()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210,$,#212));");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectOmittedAggregateItem()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210,*,#212));");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepEdgeLoopEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#300",
                sourceEntityType:
                    "ORIENTED_EDGE",
                rawStatement:
                    "#300=ORIENTED_EDGE('',*,*,#210,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var parameters =
            new StepParameter[]
            {
                new StepStringParameter(
                    rawText:
                        "''",
                    value:
                        string.Empty),

                new StepAggregateParameter(
                    rawText:
                        "(#210,#211)",
                    items:
                        new StepParameter[]
                        {
                            new StepEntityReferenceParameter(
                                rawText:
                                    "#210",
                                entityId:
                                    "#210"),

                            new StepEntityReferenceParameter(
                                rawText:
                                    "#211",
                                entityId:
                                    "#211")
                        })
            };

        Assert.Throws<ArgumentException>(
            () =>
                handler.Read(
                    record,
                    parameters));
    }

    [Fact]
    public void Handler_ShouldThrow_WhenRecordIsNull()
    {
        var handler =
            new StepEdgeLoopEntityHandler();

        Assert.Throws<ArgumentNullException>(
            () =>
                handler.Read(
                    null!,
                    Array.Empty<StepParameter>()));
    }

    [Fact]
    public void Handler_ShouldThrow_WhenParametersAreNull()
    {
        var handler =
            new StepEdgeLoopEntityHandler();

        var record =
            CreateRecord(
                "#300=EDGE_LOOP('',(#210,#211));");

        Assert.Throws<ArgumentNullException>(
            () =>
                handler.Read(
                    record,
                    null!));
    }

    private static StepSemanticEntityReader CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepEdgeLoopEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#300",
            sourceEntityType:
                "EDGE_LOOP",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}