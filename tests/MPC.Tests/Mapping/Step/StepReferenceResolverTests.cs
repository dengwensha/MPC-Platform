using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepReferenceResolverTests
{
    private static readonly double[] PointCoordinates =
    [
        1.0,
        2.0,
        3.0
    ];

    [Fact]
    public void Resolve_WithExistingEntityAndCorrectType_ReturnsResolvedResult()
    {
        var point =
            new StepCartesianPointEntity(
                sourceEntityId:
                    "#10",
                name:
                    string.Empty,
                coordinates:
                    PointCoordinates);

        StepSemanticEntity[] semanticEntities =
        [
            point
        ];

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var resolver =
            new StepReferenceResolver(
                semanticResolver);

        ReferenceResolutionResult<StepCartesianPointEntity> result =
            resolver.Resolve<StepCartesianPointEntity>(
                "#10");

        Assert.Equal(
            ReferenceResolutionStatus.Resolved,
            result.Status);

        Assert.True(
            result.IsResolved);

        Assert.Same(
            point,
            result.Value);

        Assert.Empty(
            result.Diagnostics);
    }
}