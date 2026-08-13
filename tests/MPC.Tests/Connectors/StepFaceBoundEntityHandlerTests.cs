using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepFaceBoundEntityHandlerTests
{
    [Fact]
    public void Read_WithValidFaceBoundRecord_ReturnsSemanticFaceBound()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#250=FACE_BOUND('Inner Boundary',#220,.T.);");

        var result =
            reader.Read(
                record);

        var faceBound =
            Assert.IsType<StepFaceBoundEntity>(
                result);

        Assert.Equal(
            "#250",
            faceBound.SourceEntityId);

        Assert.Equal(
            "FACE_BOUND",
            faceBound.SourceEntityType);

        Assert.Equal(
            "Inner Boundary",
            faceBound.Name);

        Assert.Equal(
            "#220",
            faceBound.LoopReferenceId);

        Assert.True(
            faceBound.Orientation);
    }

    [Fact]
    public void Read_WithFalseOrientation_PreservesFalseOrientation()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#250=FACE_BOUND('',#220,.F.);");

        var result =
            reader.Read(
                record);

        var faceBound =
            Assert.IsType<StepFaceBoundEntity>(
                result);

        Assert.False(
            faceBound.Orientation);
    }

    [Fact]
    public void Read_WithLoopThatIsNotEntityReference_ThrowsFormatException()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#250=FACE_BOUND('',220,.T.);");

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "FACE_BOUND second parameter must be a STEP entity reference.",
            exception.Message);
    }

    [Fact]
    public void Read_WithWrongParameterCount_ThrowsFormatException()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#250=FACE_BOUND('',#220);");

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "FACE_BOUND must contain exactly three parameters: name, loop reference, and orientation.",
            exception.Message);
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepFaceBoundEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#250",
                sourceEntityType:
                    "FACE_OUTER_BOUND",
                rawStatement:
                    "#250=FACE_OUTER_BOUND('',#220,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var parameters =
            new StepParameterParser()
                .Parse(
                    "'',#220,.T.");

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    handler.Read(
                        record,
                        parameters));

        Assert.Equal(
            "STEP semantic handler for 'FACE_BOUND' cannot read entity type 'FACE_OUTER_BOUND'. (Parameter 'record')",
            exception.Message);
    }

    private static StepSemanticEntityReader CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepFaceBoundEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#250",
            sourceEntityType:
                "FACE_BOUND",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}