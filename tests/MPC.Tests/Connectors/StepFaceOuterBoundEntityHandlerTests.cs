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
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#300",
                sourceEntityType:
                    "FACE_OUTER_BOUND",
                rawStatement:
                    "#300=FACE_OUTER_BOUND('Outer Boundary',#220,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepFaceOuterBoundEntityHandler()
                    });

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
}