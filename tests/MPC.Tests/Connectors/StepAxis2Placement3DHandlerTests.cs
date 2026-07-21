using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepAxis2Placement3DHandlerTests
{
    [Fact]
    public void Read_WithValidAxis2Placement3DRecord_ReturnsSemanticPlacement()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#50",
                sourceEntityType:
                    "AXIS2_PLACEMENT_3D",
                rawStatement:
                    "#50=AXIS2_PLACEMENT_3D('Placement',#42,#43,#44);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepAxis2Placement3DHandler()
                    });

        var result =
            reader.Read(
                record);

        var placement =
            Assert.IsType<StepAxis2Placement3D>(
                result);

        Assert.Equal(
            "#50",
            placement.SourceEntityId);

        Assert.Equal(
            "AXIS2_PLACEMENT_3D",
            placement.SourceEntityType);

        Assert.Equal(
            "Placement",
            placement.Name);

        Assert.Equal(
            "#42",
            placement.LocationReferenceId);

        Assert.Equal(
            "#43",
            placement.AxisReferenceId);

        Assert.Equal(
            "#44",
            placement.ReferenceDirectionId);
    }
}