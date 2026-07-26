using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

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
    public void Map_WithResolvedPlacement_ReturnsSuccessfulPlane3DResult()
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

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver);

        var mapper =
            new StepPlaneGeometryMapper();

        MappingResult<Plane3D> result =
            mapper.Map(
                plane,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
            result.Value);

        Assert.Empty(
            result.Diagnostics);

        Assert.Equal(
            1.0,
            result.Value.Origin.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Value.Origin.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            result.Value.Origin.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.Normal.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.Normal.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            result.Value.Normal.Z,
            precision:
                10);
    }
}
