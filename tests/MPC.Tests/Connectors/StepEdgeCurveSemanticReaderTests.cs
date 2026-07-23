using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepEdgeCurveSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateEdgeCurveSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge 1',#10,#11,#12,.T.);");

        var result =
            reader.Read(
                record);

        var edgeCurve =
            Assert.IsType<StepEdgeCurveEntity>(
                result);

        Assert.Equal(
            "#200",
            edgeCurve.SourceEntityId);

        Assert.Equal(
            "EDGE_CURVE",
            edgeCurve.SourceEntityType);

        Assert.Equal(
            "Edge 1",
            edgeCurve.Name);

        Assert.Equal(
            "#10",
            edgeCurve.EdgeStartReferenceId);

        Assert.Equal(
            "#11",
            edgeCurve.EdgeEndReferenceId);

        Assert.Equal(
            "#12",
            edgeCurve.EdgeGeometryReferenceId);

        Assert.True(
            edgeCurve.SameSense);
    }

    [Fact]
    public void Read_ShouldMapFalseEnumerationToFalseSameSense()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('',#10,#11,#12,.F.);");

        var result =
            reader.Read(
                record);

        var edgeCurve =
            Assert.IsType<StepEdgeCurveEntity>(
                result);

        Assert.False(
            edgeCurve.SameSense);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('',#10,#11,#12,.T.);");

        var result =
            reader.Read(
                record);

        var edgeCurve =
            Assert.IsType<StepEdgeCurveEntity>(
                result);

        Assert.Equal(
            string.Empty,
            edgeCurve.Name);
    }

    [Fact]
    public void Read_ShouldPreserveEntityReferencesExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#987,#654,#321,.T.);");

        var result =
            reader.Read(
                record);

        var edgeCurve =
            Assert.IsType<StepEdgeCurveEntity>(
                result);

        Assert.Equal(
            "#987",
            edgeCurve.EdgeStartReferenceId);

        Assert.Equal(
            "#654",
            edgeCurve.EdgeEndReferenceId);

        Assert.Equal(
            "#321",
            edgeCurve.EdgeGeometryReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenSameSenseIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#10,#11,#12);");

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
                "#200=EDGE_CURVE('Edge',#10,#11,#12,.T.,#13);");

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
                "#200=EDGE_CURVE(#1,#10,#11,#12,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectEdgeStartThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',10,#11,#12,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectEdgeEndThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#10,11,#12,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectEdgeGeometryThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#10,#11,12,.T.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectSameSenseThatIsNotEnumeration()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#10,#11,#12,'T');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectUnsupportedSameSenseEnumeration()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#10,#11,#12,.UNKNOWN.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepEdgeCurveEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#200",
                sourceEntityType:
                    "VERTEX_POINT",
                rawStatement:
                    "#200=VERTEX_POINT('',#10);",
                readStatus:
                    CadReadStatus.Complete);

        var parameters =
            new StepParameter[]
            {
                new StepStringParameter(
                    rawText:
                        "'Edge'",
                    value:
                        "Edge"),

                new StepEntityReferenceParameter(
                    rawText:
                        "#10",
                    entityId:
                        "#10"),

                new StepEntityReferenceParameter(
                    rawText:
                        "#11",
                    entityId:
                        "#11"),

                new StepEntityReferenceParameter(
                    rawText:
                        "#12",
                    entityId:
                        "#12"),

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
            new StepEdgeCurveEntityHandler();

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
            new StepEdgeCurveEntityHandler();

        var record =
            CreateRecord(
                "#200=EDGE_CURVE('Edge',#10,#11,#12,.T.);");

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
                    new StepEdgeCurveEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#200",
            sourceEntityType:
                "EDGE_CURVE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}