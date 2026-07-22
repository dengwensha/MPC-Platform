using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepCircleGeometryMapperTests
{
    [Fact]
    public void Map_ShouldCreateCircleFromSemanticEntities()
    {
        var location =
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

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                ratios:
                    new[]
                    {
                        0.0,
                        1.0,
                        0.0
                    });

        var referenceDirection =
            new StepDirectionEntity(
                sourceEntityId:
                    "#12",
                name:
                    string.Empty,
                ratios:
                    new[]
                    {
                        1.0,
                        0.0,
                        0.0
                    });

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    "#20",
                name:
                    string.Empty,
                locationReferenceId:
                    "#10",
                axisReferenceId:
                    "#11",
                refDirectionReferenceId:
                    "#12");

        var circle =
            new StepCircleEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20",
                radius:
                    4.5);

        StepSemanticEntity[] entities =
        {
            location,
            axis,
            referenceDirection,
            placement,
            circle
        };

        var mapper =
            new StepCircleGeometryMapper(
                entities);

        var result =
            mapper.Map(
                circle);

        Assert.Equal(
            new Point3D(
                1.0,
                2.0,
                3.0),
            result.Center);

        Assert.Equal(
            new Vector3D(
                0.0,
                1.0,
                0.0),
            result.Normal);

        Assert.Equal(
            4.5,
            result.Radius);
    }

    [Fact]
    public void Map_ShouldUsePositiveZNormal_WhenPlacementAxisIsMissing()
    {
        var location =
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

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    "#20",
                name:
                    string.Empty,
                locationReferenceId:
                    "#10",
                axisReferenceId:
                    null,
                refDirectionReferenceId:
                    null);

        var circle =
            new StepCircleEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20",
                radius:
                    2.0);

        StepSemanticEntity[] entities =
        {
            location,
            placement,
            circle
        };

        var mapper =
            new StepCircleGeometryMapper(
                entities);

        var result =
            mapper.Map(
                circle);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            result.Normal);
    }

    [Fact]
    public void Map_ShouldThrow_WhenLocationDoesNotContainThreeCoordinates()
    {
        var location =
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
                    });

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    "#20",
                name:
                    string.Empty,
                locationReferenceId:
                    "#10",
                axisReferenceId:
                    null,
                refDirectionReferenceId:
                    null);

        var circle =
            new StepCircleEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20",
                radius:
                    2.0);

        StepSemanticEntity[] entities =
        {
            location,
            placement,
            circle
        };

        var mapper =
            new StepCircleGeometryMapper(
                entities);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    mapper.Map(
                        circle));

        Assert.Contains(
            "exactly three coordinates",
            exception.Message);
    }

    [Fact]
    public void Map_ShouldThrow_WhenAxisDoesNotContainThreeRatios()
    {
        var location =
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

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                ratios:
                    new[]
                    {
                        0.0,
                        1.0
                    });

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    "#20",
                name:
                    string.Empty,
                locationReferenceId:
                    "#10",
                axisReferenceId:
                    "#11",
                refDirectionReferenceId:
                    null);

        var circle =
            new StepCircleEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20",
                radius:
                    2.0);

        StepSemanticEntity[] entities =
        {
            location,
            axis,
            placement,
            circle
        };

        var mapper =
            new StepCircleGeometryMapper(
                entities);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    mapper.Map(
                        circle));

        Assert.Contains(
            "exactly three direction ratios",
            exception.Message);
    }

    [Fact]
    public void Map_ShouldThrow_WhenPlacementCannotBeResolved()
    {
        var circle =
            new StepCircleEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#999",
                radius:
                    2.0);

        StepSemanticEntity[] entities =
        {
            circle
        };

        var mapper =
            new StepCircleGeometryMapper(
                entities);

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    circle));
    }

    [Fact]
    public void Map_ShouldThrow_WhenSourceIsNull()
    {
        var mapper =
            new StepCircleGeometryMapper(
                Array.Empty<StepSemanticEntity>());

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    null!));
    }
}