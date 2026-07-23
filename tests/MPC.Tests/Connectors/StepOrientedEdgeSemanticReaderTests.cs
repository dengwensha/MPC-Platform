using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepOrientedEdgeSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateOrientedEdgeSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('Edge Use',*,*,#100,.T.);");

        var result =
            reader.Read(
                record);

        var orientedEdge =
            Assert.IsType<StepOrientedEdgeEntity>(
                result);

        Assert.Equal(
            "#200",
            orientedEdge.SourceEntityId);

        Assert.Equal(
            "ORIENTED_EDGE",
            orientedEdge.SourceEntityType);

        Assert.Equal(
            "Edge Use",
            orientedEdge.Name);

        Assert.Equal(
            "#100",
            orientedEdge.EdgeElementReferenceId);

        Assert.True(
            orientedEdge.Orientation);
    }

    [Fact]
    public void Read_ShouldMapFalseEnumerationToFalseOrientation()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#100,.F.);");

        var result =
            reader.Read(
                record);

        var orientedEdge =
            Assert.IsType<StepOrientedEdgeEntity>(
                result);

        Assert.False(
            orientedEdge.Orientation);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#100,.T.);");

        var result =
            reader.Read(
                record);

        var orientedEdge =
            Assert.IsType<StepOrientedEdgeEntity>(
                result);

        Assert.Equal(
            string.Empty,
            orientedEdge.Name);
    }

    [Fact]
    public void Read_ShouldPreserveEdgeElementReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#987,.T.);");

        var result =
            reader.Read(
                record);

        var orientedEdge =
            Assert.IsType<StepOrientedEdgeEntity>(
                result);

        Assert.Equal(
            "#987",
            orientedEdge.EdgeElementReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#100);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectExplicitEdgeStartReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',#10,*,#100,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectExplicitEdgeEndReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,#11,#100,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullDerivedEdgeStart()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',$,*,#100,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectEdgeElementThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,100,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectOrientationThatIsNotEnumeration()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#100,'T');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectUnsupportedOrientationEnumeration()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#100,.UNKNOWN.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepOrientedEdgeEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#200",
                sourceEntityType:
                    "EDGE_CURVE",
                rawStatement:
                    "#200=EDGE_CURVE('',#1,#2,#3,.T.);",
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

                new StepOmittedParameter(
                    rawText:
                        "*"),

                new StepOmittedParameter(
                    rawText:
                        "*"),

                new StepEntityReferenceParameter(
                    rawText:
                        "#100",
                    entityId:
                        "#100"),

                new StepEnumerationParameter(
                    rawText:
                        ".T.",
                    name:
                        "T")
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
            new StepOrientedEdgeEntityHandler();

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
            new StepOrientedEdgeEntityHandler();

        var record =
            CreateRecord(
                "#200=ORIENTED_EDGE('',*,*,#100,.T.);");

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
                    new StepOrientedEdgeEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#200",
            sourceEntityType:
                "ORIENTED_EDGE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}