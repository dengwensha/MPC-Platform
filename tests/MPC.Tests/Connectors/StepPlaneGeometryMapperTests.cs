using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using ConnectorStepPlaneGeometryMapper =
    MPC.Connectors.Step.Mapping.StepPlaneGeometryMapper;

namespace MPC.Tests.Connectors;

public sealed class StepPlaneGeometryMapperTests
{
    private static readonly double[] LocationCoordinates =
    [
        1.0,
        2.0,
        3.0
    ];

    private static readonly double[] AxisRatios =
    [
        0.0,
        0.0,
        1.0
    ];

    private static readonly double[] ReferenceDirectionRatios =
    [
        1.0,
        0.0,
        0.0
    ];

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
                    LocationCoordinates);

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                ratios:
                    AxisRatios);

        var referenceDirection =
            new StepDirectionEntity(
                sourceEntityId:
                    "#12",
                name:
                    string.Empty,
                ratios:
                    ReferenceDirectionRatios);

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

        StepSemanticEntity[] semanticEntities =
        [
            location,
            axis,
            referenceDirection,
            placement,
            plane
        ];

        var mapper =
            new ConnectorStepPlaneGeometryMapper(
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