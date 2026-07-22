using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepSphereGeometryMapperTests
{
    [Fact]
    public void Map_ShouldCreateSphere()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    "#10",
                    "Center",
                    new[] { 1.0, 2.0, 3.0 }),

                new StepAxis2Placement3DEntity(
                    "#20",
                    "Placement",
                    "#10",
                    null,
                    null),

                new StepSphericalSurfaceEntity(
                    "#30",
                    "Sphere",
                    "#20",
                    25.0)
            };

        var mapper =
            CreateMapper(
                entities);

        var result =
            mapper.Map(
                (StepSphericalSurfaceEntity)entities[2]);

        Assert.Equal(
            new Point3D(
                1.0,
                2.0,
                3.0),
            result.Center);

        Assert.Equal(
            25.0,
            result.Radius);
    }

    [Fact]
    public void Map_ShouldIgnorePlacementAxis()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    "#10",
                    "",
                    new[] { 5.0, 6.0, 7.0 }),

                new StepDirectionEntity(
                    "#11",
                    "",
                    new[] { 1.0, 0.0, 0.0 }),

                new StepAxis2Placement3DEntity(
                    "#20",
                    "",
                    "#10",
                    "#11",
                    null),

                new StepSphericalSurfaceEntity(
                    "#30",
                    "",
                    "#20",
                    4.5)
            };

        var mapper =
            CreateMapper(
                entities);

        var result =
            mapper.Map(
                (StepSphericalSurfaceEntity)entities[3]);

        Assert.Equal(
            new Point3D(
                5.0,
                6.0,
                7.0),
            result.Center);

        Assert.Equal(
            4.5,
            result.Radius);
    }

    [Fact]
    public void Map_ShouldThrow_WhenPlacementCannotBeResolved()
    {
        var source =
            new StepSphericalSurfaceEntity(
                "#30",
                "",
                "#999",
                5.0);

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
    public void Map_ShouldThrow_WhenLocationCannotBeResolved()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepAxis2Placement3DEntity(
                    "#20",
                    "",
                    "#999",
                    null,
                    null),

                new StepSphericalSurfaceEntity(
                    "#30",
                    "",
                    "#20",
                    5.0)
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepSphericalSurfaceEntity)entities[1]));
    }

    [Fact]
    public void Map_ShouldThrow_WhenCoordinateCountIsInvalid()
    {
        var entities =
            new StepSemanticEntity[]
            {
                new StepCartesianPointEntity(
                    "#10",
                    "",
                    new[] { 1.0, 2.0 }),

                new StepAxis2Placement3DEntity(
                    "#20",
                    "",
                    "#10",
                    null,
                    null),

                new StepSphericalSurfaceEntity(
                    "#30",
                    "",
                    "#20",
                    5.0)
            };

        var mapper =
            CreateMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    (StepSphericalSurfaceEntity)entities[2]));
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

    private static StepSphereGeometryMapper CreateMapper(
        IReadOnlyList<StepSemanticEntity> entities)
    {
        var resolver =
            new StepSemanticEntityResolver(
                entities);

        return new StepSphereGeometryMapper(
            resolver);
    }
}