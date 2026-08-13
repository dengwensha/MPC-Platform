using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepFaceOuterBoundEntityHandlerTests
{
    [Fact]
    public void Read_WithValidFaceOuterBoundRecord_ReturnsSemanticFaceOuterBound()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=FACE_OUTER_BOUND('Outer Boundary',#220,.T.);");

        var result =
            reader.Read(
                record);

        var outerBound =
            Assert.IsType<StepFaceOuterBoundEntity>(
                result);

        Assert.Equal(
            "#300",
            outerBound.SourceEntityId);

        Assert.Equal(
            "FACE_OUTER_BOUND",
            outerBound.SourceEntityType);

        Assert.Equal(
            "Outer Boundary",
            outerBound.Name);

        Assert.Equal(
            "#220",
            outerBound.LoopReferenceId);

        Assert.True(
            outerBound.Orientation);
    }

    [Fact]
    public void Read_WithFalseOrientation_PreservesFalseOrientation()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=FACE_OUTER_BOUND('',#220,.F.);");

        var result =
            reader.Read(
                record);

        var outerBound =
            Assert.IsType<StepFaceOuterBoundEntity>(
                result);

        Assert.False(
            outerBound.Orientation);
    }

    [Fact]
    public void Read_WithLoopThatIsNotEntityReference_ThrowsFormatException()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=FACE_OUTER_BOUND('',220,.T.);");

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "FACE_OUTER_BOUND second parameter must be a STEP entity reference.",
            exception.Message);
    }

    [Fact]
    public void Read_WithWrongParameterCount_ThrowsFormatException()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#300=FACE_OUTER_BOUND('',#220);");

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "FACE_OUTER_BOUND must contain exactly three parameters: name, loop reference, and orientation.",
            exception.Message);
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepFaceOuterBoundEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#300",
                sourceEntityType:
                    "FACE_BOUND",
                rawStatement:
                    "#300=FACE_BOUND('',#220,.T.);",
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
            "STEP semantic handler for 'FACE_OUTER_BOUND' cannot read entity type 'FACE_BOUND'. (Parameter 'record')",
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
                    new StepFaceOuterBoundEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#300",
            sourceEntityType:
                "FACE_OUTER_BOUND",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}