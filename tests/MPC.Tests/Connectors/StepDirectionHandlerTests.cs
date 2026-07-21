using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepDirectionHandlerTests
{
    [Fact]
    public void Read_WithValidDirectionRecord_ReturnsSemanticDirection()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#43",
                sourceEntityType:
                    "DIRECTION",
                rawStatement:
                    "#43=DIRECTION('Z Axis',(0.0,0.0,1.0));",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepDirectionHandler()
                    });

        var result =
            reader.Read(
                record);

        var direction =
            Assert.IsType<StepDirection>(
                result);

        Assert.Equal(
            "#43",
            direction.SourceEntityId);

        Assert.Equal(
            "DIRECTION",
            direction.SourceEntityType);

        Assert.Equal(
            "Z Axis",
            direction.Name);

        Assert.Collection(
            direction.DirectionRatios,
            ratio =>
                Assert.Equal(
                    0.0,
                    ratio,
                    precision:
                        10),
            ratio =>
                Assert.Equal(
                    0.0,
                    ratio,
                    precision:
                        10),
            ratio =>
                Assert.Equal(
                    1.0,
                    ratio,
                    precision:
                        10));
    }
}