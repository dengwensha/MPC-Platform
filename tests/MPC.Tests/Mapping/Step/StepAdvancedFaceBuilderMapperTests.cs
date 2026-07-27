using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Building;
using Xunit;

namespace MPC.Tests.Mapping.Step;

public sealed class StepAdvancedFaceBuilderMapperTests
{
    private const string LocationReferenceId = "#10";
    private const string AxisReferenceId = "#11";
    private const string ReferenceDirectionReferenceId = "#12";
    private const string PlacementReferenceId = "#20";
    private const string SurfaceReferenceId = "#30";
    private const string AdvancedFaceReferenceId = "#40";

    [Fact]
    public void Map_WhenSurfaceIsPlane_ReturnsUnbuiltFaceBuilder()
    {
        var location =
            new StepCartesianPointEntity(
                sourceEntityId:
                    LocationReferenceId,
                name:
                    string.Empty,
                coordinates:
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    AxisReferenceId,
                name:
                    string.Empty,
                ratios:
                [
                    0.0,
                    0.0,
                    1.0
                ]);

        var referenceDirection =
            new StepDirectionEntity(
                sourceEntityId:
                    ReferenceDirectionReferenceId,
                name:
                    string.Empty,
                ratios:
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    PlacementReferenceId,
                name:
                    string.Empty,
                locationReferenceId:
                    LocationReferenceId,
                axisReferenceId:
                    AxisReferenceId,
                refDirectionReferenceId:
                    ReferenceDirectionReferenceId);

        var plane =
            new StepPlaneEntity(
                sourceEntityId:
                    SurfaceReferenceId,
                name:
                    string.Empty,
                placementReferenceId:
                    PlacementReferenceId);

        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                plane,
                advancedFace);

        var mapper =
            new StepAdvancedFaceBuilderMapper();

        MappingResult<FaceBuilder> result =
            mapper.Map(
                advancedFace,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.Empty(
            result.Diagnostics);

        FaceBuilder faceBuilder =
            Assert.IsType<FaceBuilder>(
                result.Value);

        Plane3D mappedPlane =
            Assert.IsType<Plane3D>(
                faceBuilder.Surface);

        Assert.Equal(
            1.0,
            mappedPlane.Origin.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            mappedPlane.Origin.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            mappedPlane.Origin.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            mappedPlane.Normal.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            mappedPlane.Normal.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            mappedPlane.Normal.Z,
            precision:
                10);

        Assert.Null(
            faceBuilder.OuterLoop);

        Assert.Empty(
            faceBuilder.InnerLoops);

        Assert.False(
            faceBuilder.IsBuilt);
    }

    [Fact]
    public void Map_WhenSurfaceReferenceCannotBeResolved_PropagatesFailure()
    {
        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                advancedFace);

        var mapper =
            new StepAdvancedFaceBuilderMapper();

        MappingResult<FaceBuilder> result =
            mapper.Map(
                advancedFace,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        Assert.NotEmpty(
            result.Diagnostics);
    }

    [Fact]
    public void Map_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        MappingContext context =
            CreateContext();

        var mapper =
            new StepAdvancedFaceBuilderMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WhenContextIsNull_ThrowsArgumentNullException()
    {
        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        var mapper =
            new StepAdvancedFaceBuilderMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                advancedFace,
                null!));
    }

    [Fact]
    public void Constructor_WhenGeometryMapperIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepAdvancedFaceBuilderMapper(
                null!));
    }

    private static StepAdvancedFaceEntity CreateAdvancedFace()
    {
        return new StepAdvancedFaceEntity(
            sourceEntityId:
                AdvancedFaceReferenceId,
            name:
                string.Empty,
            boundReferenceIds:
            [
                "#50"
            ],
            surfaceReferenceId:
                SurfaceReferenceId,
            sameSense:
                true);
    }

    private static MappingContext CreateContext(
        params StepSemanticEntity[] semanticEntities)
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        return new MappingContext(
            referenceResolver);
    }
}