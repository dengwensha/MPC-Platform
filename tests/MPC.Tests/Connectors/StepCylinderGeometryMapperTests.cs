using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepCylinderGeometryMapperTests
{
    [Fact]
    public void Map_ShouldCreateCylinderFromSemanticEntities()
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

        var cylindricalSurface =
            new StepCylindricalSurfaceEntity(
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
            cylindricalSurface
        };

        var mapper =
            new StepCylinderGeometryMapper(
                entities);

        var result =
            mapper.Map(
                cylindricalSurface);

        Assert.Equal(
            1.0,
            result.AxisOrigin.X);

        Assert.Equal(
            2.0,
            result.AxisOrigin.Y);

        Assert.Equal(
            3.0,
            result.AxisOrigin.Z);

        Assert.Equal(
            0.0,
            result.AxisDirection.X);

        Assert.Equal(
            1.0,
            result.AxisDirection.Y);

        Assert.Equal(
            0.0,
            result.AxisDirection.Z);

        Assert.Equal(
            4.5,
            result.Radius);
    }

    [Fact]
    public void Map_ShouldUsePositiveZAxis_WhenPlacementAxisIsMissing()
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

        var cylindricalSurface =
            new StepCylindricalSurfaceEntity(
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
            cylindricalSurface
        };

        var mapper =
            new StepCylinderGeometryMapper(
                entities);

        var result =
            mapper.Map(
                cylindricalSurface);

        Assert.Equal(
            0.0,
            result.AxisDirection.X);

        Assert.Equal(
            0.0,
            result.AxisDirection.Y);

        Assert.Equal(
            1.0,
            result.AxisDirection.Z);
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

        var cylindricalSurface =
            new StepCylindricalSurfaceEntity(
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
            cylindricalSurface
        };

        var mapper =
            new StepCylinderGeometryMapper(
                entities);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    mapper.Map(
                        cylindricalSurface));

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

        var cylindricalSurface =
            new StepCylindricalSurfaceEntity(
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
            cylindricalSurface
        };

        var mapper =
            new StepCylinderGeometryMapper(
                entities);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    mapper.Map(
                        cylindricalSurface));

        Assert.Contains(
            "exactly three direction ratios",
            exception.Message);
    }
}