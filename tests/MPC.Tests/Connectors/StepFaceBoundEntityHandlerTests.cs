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
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#250",
                sourceEntityType:
                    "FACE_BOUND",
                rawStatement:
                    "#250=FACE_BOUND('Inner Boundary',#220,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepFaceBoundEntityHandler()
                    });

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
}