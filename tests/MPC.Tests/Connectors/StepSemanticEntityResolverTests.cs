using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepSemanticEntityResolverTests
{
    [Fact]
    public void Resolve_WithExistingEntityId_ReturnsRequestedSemanticEntity()
    {
        var point =
            new StepCartesianPoint(
                sourceEntityId:
                    "#10",
                name:
                    string.Empty,
                coordinates:
                    new[]
                    {
                        1.0,
                        2.0,
                        3.0
                    });

        var direction =
            new StepDirection(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                directionRatios:
                    new[]
                    {
                        0.0,
                        0.0,
                        1.0
                    });

        var semanticEntities =
            new StepSemanticEntity[]
            {
                point,
                direction
            };

        var resolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var result =
            resolver.Resolve<StepCartesianPoint>(
                "#10");

        Assert.Same(
            point,
            result);
    }
}