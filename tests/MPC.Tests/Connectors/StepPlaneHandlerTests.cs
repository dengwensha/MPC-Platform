using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepPlaneHandlerTests
{
    [Fact]
    public void Read_WithValidPlaneRecord_ReturnsSemanticPlane()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#70",
                sourceEntityType:
                    "PLANE",
                rawStatement:
                    "#70=PLANE('Base Plane',#50);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepPlaneHandler()
                    });

        var result =
            reader.Read(
                record);

        var plane =
            Assert.IsType<StepPlane>(
                result);

        Assert.Equal(
            "#70",
            plane.SourceEntityId);

        Assert.Equal(
            "PLANE",
            plane.SourceEntityType);

        Assert.Equal(
            "Base Plane",
            plane.Name);

        Assert.Equal(
            "#50",
            plane.PositionReferenceId);
    }
}