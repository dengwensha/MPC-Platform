using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepConicalSurfaceGeometryMapperTests
{
    [Fact]
    public void Map_ShouldCreateConeAndCalculateApex()
    {
        var semiAngle =
            Math.PI / 4.0;

        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    sourceEntityId:
                        "#10",
                    name:
                        "Placement Origin",
                    coordinates:
                        new[]
                        {
                            1.0,
                            2.0,
                            10.0
                        }),

                new StepDirectionEntity(
                    sourceEntityId:
                        "#20",
                    name:
                        "Cone Axis",
                    ratios:
                        new[]
                        {
                            0.0,
                            0.0,
                            1.0
                        }),

                new StepAxis2Placement3DEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        "Cone Placement",
                    locationReferenceId:
                        "#10",
                    axisReferenceId:
                        "#20",
                    refDirectionReferenceId:
                        null),

                new StepConicalSurfaceEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        "Cone",
                    placementReferenceId:
                        "#30",
                    radius:
                        2.0,
                    semiAngle:
                        semiAngle)
            };

        var mapper =
            CreateMapper(
                entities);

        var result =
            mapper.Map(
                (StepConicalSurfaceEntity)entities[3]);

        Assert.Equal(
            new Point3D(
                1.0,
                2.0,
                8.0),
            result.Apex);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            result.AxisDirection);

        Assert.Equal(
            semiAngle,
            result.SemiAngle);
    }

    [Fact]
    public void Map_ShouldUsePositiveZAxis_WhenPlacementAxisIsMissing()
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
                            5.0
                        }),

                new StepAxis2Placement3DEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    locationReferenceId:
                        "#10",
                    axisReferenceId:
                        null,
                    refDirectionReferenceId:
                        null),

                new StepConicalSurfaceEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    placementReferenceId:
                        "#30",
                    radius:
                        1.0,
                    semiAngle:
                        Math.PI / 4.0)
            };

        var mapper =
            CreateMapper(
                entities);

        var result =
            mapper.Map(
                (StepConicalSurfaceEntity)entities[2]);

        Assert.Equal(
            new Point3D(
                0.0,
                0.0,
                4.0),
            result.Apex);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            result.AxisDirection);
    }

    [Fact]
    public void Map_ShouldCanonicalizeNegativeSemiAngle()
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
                            5.0
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
                            0.0,
                            1.0
                        }),

                new StepAxis2Placement3DEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    locationReferenceId:
                        "#10",
                    axisReferenceId:
                        "#20",
                    refDirectionReferenceId:
                        null),

                new StepConicalSurfaceEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    placementReferenceId:
                        "#30",
                    radius:
                        2.0,
                    semiAngle:
                        -Math.PI / 4.0)
            };

        var mapper =
            CreateMapper(
                entities);

        var result =
            mapper.Map(
                (StepConicalSurfaceEntity)entities[3]);

        Assert.Equal(
            new Point3D(
                0.0,
                0.0,
                7.0),
            result.Apex);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                -1.0),
            result.AxisDirection);

        Assert.Equal(
            Math.PI / 4.0,
            result.SemiAngle);
    }

    [Fact]
    public void Map_ShouldThrow_WhenLocationDoesNotContainThreeCoordinates()
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

                new StepAxis2Placement3DEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    locationReferenceId:
                        "#10",
                    axisReferenceId:
                        null,
                    refDirectionReferenceId:
                        null),

                new StepConicalSurfaceEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    placementReferenceId:
                        "#30",
                    radius:
                        1.0,
                    semiAngle:
                        Math.PI / 4.0)
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepConicalSurfaceEntity)entities[2]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenAxisDoesNotContainThreeRatios()
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
                            1.0
                        }),

                new StepAxis2Placement3DEntity(
                    sourceEntityId:
                        "#30",
                    name:
                        string.Empty,
                    locationReferenceId:
                        "#10",
                    axisReferenceId:
                        "#20",
                    refDirectionReferenceId:
                        null),

                new StepConicalSurfaceEntity(
                    sourceEntityId:
                        "#40",
                    name:
                        string.Empty,
                    placementReferenceId:
                        "#30",
                    radius:
                        1.0,
                    semiAngle:
                        Math.PI / 4.0)
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepConicalSurfaceEntity)entities[3]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenPlacementCannotBeResolved()
    {
        var source =
            new StepConicalSurfaceEntity(
                sourceEntityId:
                    "#40",
                name:
                    string.Empty,
                placementReferenceId:
                    "#30",
                radius:
                    1.0,
                semiAngle:
                    Math.PI / 4.0);

        var mapper =
            CreateMapper(
                new StepSemanticEntity[]
                {
                    source
                });

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    source));
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

    private static StepConicalSurfaceGeometryMapper CreateMapper(
        IReadOnlyList<StepSemanticEntity> entities)
    {
        var resolver =
            new StepSemanticEntityResolver(
                entities);

        return new StepConicalSurfaceGeometryMapper(
            resolver);
    }
}