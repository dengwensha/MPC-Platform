using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepCartesianPointHandlerTests
{
    [Fact]
    public void Read_WithValidCartesianPointRecord_ReturnsSemanticCartesianPoint()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#42",
                sourceEntityType:
                    "CARTESIAN_POINT",
                rawStatement:
                    "#42=CARTESIAN_POINT('Origin',(0.0,0.0,0.0));",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepCartesianPointHandler()
                    });

        var result =
            reader.Read(
                record);

        var cartesianPoint =
            Assert.IsType<StepCartesianPoint>(
                result);

        Assert.Equal(
            "#42",
            cartesianPoint.SourceEntityId);

        Assert.Equal(
            "CARTESIAN_POINT",
            cartesianPoint.SourceEntityType);

        Assert.Equal(
            "Origin",
            cartesianPoint.Name);

        Assert.Collection(
            cartesianPoint.Coordinates,
            coordinate =>
                Assert.Equal(
                    0.0,
                    coordinate,
                    precision:
                        10),
            coordinate =>
                Assert.Equal(
                    0.0,
                    coordinate,
                    precision:
                        10),
            coordinate =>
                Assert.Equal(
                    0.0,
                    coordinate,
                    precision:
                        10));
    }
}