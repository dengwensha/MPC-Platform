using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepSemanticEntityResolverTests
{
    [Fact]
    public void Resolve_WithExistingEntityId_ReturnsRequestedSemanticEntity()
    {
        var point =
            new StepCartesianPointEntity(
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
            new StepDirectionEntity(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                ratios:
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
            resolver.Resolve<StepCartesianPointEntity>(
                "#10");

        Assert.Same(
            point,
            result);
    }

    [Fact]
    public void Resolve_WithIncorrectRequestedType_ThrowsInvalidOperationException()
    {
        var point =
            new StepCartesianPointEntity(
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

        var resolver =
            new StepSemanticEntityResolver(
                new StepSemanticEntity[]
                {
                    point
                });

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    resolver.Resolve<StepDirectionEntity>(
                        "#10"));

        Assert.Contains(
            "#10",
            exception.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            nameof(StepDirectionEntity),
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_WithUnknownEntityId_ThrowsInvalidOperationException()
    {
        var resolver =
            new StepSemanticEntityResolver(
                Array.Empty<StepSemanticEntity>());

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    resolver.Resolve<StepCartesianPointEntity>(
                        "#999"));

        Assert.Contains(
            "#999",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WithDuplicateEntityId_ThrowsInvalidOperationException()
    {
        var firstPoint =
            new StepCartesianPointEntity(
                sourceEntityId:
                    "#10",
                name:
                    "First",
                coordinates:
                    new[]
                    {
                        1.0,
                        2.0,
                        3.0
                    });

        var secondPoint =
            new StepCartesianPointEntity(
                sourceEntityId:
                    "#10",
                name:
                    "Second",
                coordinates:
                    new[]
                    {
                        4.0,
                        5.0,
                        6.0
                    });

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    new StepSemanticEntityResolver(
                        new StepSemanticEntity[]
                        {
                            firstPoint,
                            secondPoint
                        }));

        Assert.Contains(
            "#10",
            exception.Message,
            StringComparison.Ordinal);
    }
}
