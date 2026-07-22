using MPC.Connectors.Step.Mapping;
using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Tests.Connectors;

public sealed class StepPlaneGeometryMapperTests
{
    [Fact]
    public void Map_WithResolvedPlacement_ReturnsPlane3D()
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
                        0.0,
                        1.0
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

        var plane =
            new StepPlaneEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20");

        var semanticEntities =
            new StepSemanticEntity[]
            {
                location,
                axis,
                referenceDirection,
                placement,
                plane
            };

        var mapper =
            new StepPlaneGeometryMapper(
                semanticEntities);

        Plane3D result =
            mapper.Map(
                plane);

        Assert.Equal(
            1.0,
            result.Origin.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Origin.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            result.Origin.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Normal.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Normal.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            result.Normal.Z,
            precision:
                10);
    }
}



