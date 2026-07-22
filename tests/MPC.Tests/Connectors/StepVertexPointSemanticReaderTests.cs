using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepVertexPointSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateVertexPointSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('Vertex 1',#100);");

        var result =
            reader.Read(
                record);

        var vertexPoint =
            Assert.IsType<StepVertexPointEntity>(
                result);

        Assert.Equal(
            "#200",
            vertexPoint.SourceEntityId);

        Assert.Equal(
            "VERTEX_POINT",
            vertexPoint.SourceEntityType);

        Assert.Equal(
            "Vertex 1",
            vertexPoint.Name);

        Assert.Equal(
            "#100",
            vertexPoint.VertexGeometryReferenceId);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('',#100);");

        var result =
            reader.Read(
                record);

        var vertexPoint =
            Assert.IsType<StepVertexPointEntity>(
                result);

        Assert.Equal(
            string.Empty,
            vertexPoint.Name);
    }

    [Fact]
    public void Read_ShouldPreserveVertexGeometryReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('Vertex',#987);");

        var result =
            reader.Read(
                record);

        var vertexPoint =
            Assert.IsType<StepVertexPointEntity>(
                result);

        Assert.Equal(
            "#987",
            vertexPoint.VertexGeometryReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenReferenceIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('Vertex');");

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
                "#200=VERTEX_POINT('Vertex',#100,#101);");

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
                "#200=VERTEX_POINT(#1,#100);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullVertexGeometryReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('Vertex',$);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectVertexGeometryThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('Vertex',100);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepVertexPointEntityHandler();

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
                        "'Vertex'",
                    value:
                        "Vertex"),

                new StepEntityReferenceParameter(
                    rawText:
                        "#100",
                    entityId:
                        "#100")
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
            new StepVertexPointEntityHandler();

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
            new StepVertexPointEntityHandler();

        var record =
            CreateRecord(
                "#200=VERTEX_POINT('Vertex',#100);");

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
                    new StepVertexPointEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#200",
            sourceEntityType:
                "VERTEX_POINT",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}