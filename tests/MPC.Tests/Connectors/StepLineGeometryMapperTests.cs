using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepLineGeometryMapperTests
{
    [Fact]
    public void Map_ShouldCreateLineFromSemanticEntities()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        "Line Origin",
                    coordinates:
                        new[]
                        {
                            1.0,
                            2.0,
                            3.0
                        }),

                new StepDirectionEntity(
                    sourceEntityId:
                        "#20",
                    name:
                        "Line Direction",
                    ratios:
                        new[]
                        {
                            1.0,
                            0.0,
                            0.0
                        }),

                new StepVectorEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        "Line Vector",
                    orientationReferenceId:
                        "#20",
                    magnitude:
                        25.0),

                new StepLineEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        "Main Line",
                    pointReferenceId:
                        "#10",
                    vectorReferenceId:
                        "#30")
            };

        var mapper =
            CreateMapper(
                entities);

        var source =
            Assert.IsType<StepLineEntity>(
                entities[3]);

        var result =
            mapper.Map(
                source);

        Assert.Equal(
            new Point3D(
                1.0,
                2.0,
                3.0),
            result.Origin);

        Assert.Equal(
            new Vector3D(
                1.0,
                0.0,
                0.0),
            result.Direction);
    }

    [Fact]
    public void Map_ShouldIgnoreVectorMagnitude()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        string.Empty,
                    coordinates:
                        new[]
                        {
                            0.0,
                            0.0,
                            0.0
                        }),

                new StepDirectionEntity(
                    sourceEntityId:
                        "#20",
                    name:
                        string.Empty,
                    ratios:
                        new[]
                        {
                            0.0,
                            1.0,
                            0.0
                        }),

                new StepVectorEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    orientationReferenceId:
                        "#20",
                    magnitude:
                        1000.0),

                new StepLineEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    pointReferenceId:
                        "#10",
                    vectorReferenceId:
                        "#30")
            };

        var mapper =
            CreateMapper(
                entities);

        var result =
            mapper.Map(
                (StepLineEntity)entities[3]);

        Assert.Equal(
            new Vector3D(
                0.0,
                1.0,
                0.0),
            result.Direction);
    }

    [Fact]
    public void Map_ShouldThrow_WhenPointDoesNotContainThreeCoordinates()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        string.Empty,
                    coordinates:
                        new[]
                        {
                            1.0,
                            2.0
                        }),

                new StepDirectionEntity(
                    sourceEntityId:
                        "#20",
                    name:
                        string.Empty,
                    ratios:
                        new[]
                        {
                            1.0,
                            0.0,
                            0.0
                        }),

                new StepVectorEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    orientationReferenceId:
                        "#20",
                    magnitude:
                        1.0),

                new StepLineEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    pointReferenceId:
                        "#10",
                    vectorReferenceId:
                        "#30")
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepLineEntity)entities[3]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenDirectionDoesNotContainThreeRatios()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        string.Empty,
                    coordinates:
                        new[]
                        {
                            0.0,
                            0.0,
                            0.0
                        }),

                new StepDirectionEntity(
                    sourceEntityId:
                        "#20",
                    name:
                        string.Empty,
                    ratios:
                        new[]
                        {
                            1.0,
                            0.0
                        }),

                new StepVectorEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    orientationReferenceId:
                        "#20",
                    magnitude:
                        1.0),

                new StepLineEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    pointReferenceId:
                        "#10",
                    vectorReferenceId:
                        "#30")
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepLineEntity)entities[3]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenVectorCannotBeResolved()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        string.Empty,
                    coordinates:
                        new[]
                        {
                            0.0,
                            0.0,
                            0.0
                        }),

                new StepLineEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    pointReferenceId:
                        "#10",
                    vectorReferenceId:
                        "#30")
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepLineEntity)entities[1]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenDirectionCannotBeResolved()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        string.Empty,
                    coordinates:
                        new[]
                        {
                            0.0,
                            0.0,
                            0.0
                        }),

                new StepVectorEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    orientationReferenceId:
                        "#20",
                    magnitude:
                        1.0),

                new StepLineEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    pointReferenceId:
                        "#10",
                    vectorReferenceId:
                        "#30")
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepLineEntity)entities[2]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenSourceIsNull()
    {
        var mapper =
            CreateMapper(
                Array.Empty<StepSemanticEntity>());

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    null!));
    }

    private static StepLineGeometryMapper CreateMapper(
        IReadOnlyList<StepSemanticEntity> entities)
    {
        var resolver =
            new StepSemanticEntityResolver(
                entities);

        return new StepLineGeometryMapper(
            resolver);
    }
}