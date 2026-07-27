using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepAdvancedFaceGeometryMapperTests
{
    private const string LocationReferenceId =
        "#10";

    private const string AxisReferenceId =
        "#11";

    private const string ReferenceDirectionReferenceId =
        "#12";

    private const string PlacementReferenceId =
        "#20";

    private const string SurfaceReferenceId =
        "#30";

    private const string AdvancedFaceReferenceId =
        "#40";

    private static readonly double[] ValidLocationCoordinates =
    [
        1.0,
        2.0,
        3.0
    ];

    private static readonly double[] ValidAxisRatios =
    [
        0.0,
        0.0,
        1.0
    ];

    private static readonly double[] ValidReferenceDirectionRatios =
    [
        1.0,
        0.0,
        0.0
    ];

    [Fact]
    public void Map_WhenSurfaceIsPlane_ReturnsSuccessfulPlane3DResult()
    {
        StepCartesianPointEntity location =
            CreateLocation();

        StepDirectionEntity axis =
            CreateAxis();

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection();

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

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
            new StepAdvancedFaceGeometryMapper();

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        AssertSuccessfulResult(
            result);

        Plane3D mappedPlane =
            Assert.IsType<Plane3D>(
                result.Value);

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
    }

    [Fact]
    public void Map_WhenSurfaceIsCylinder_ReturnsSuccessfulCylinder3DResult()
    {
        StepCartesianPointEntity location =
            CreateLocation();

        StepDirectionEntity axis =
            CreateAxis();

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection();

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        var cylindricalSurface =
            new StepCylindricalSurfaceEntity(
                sourceEntityId:
                    SurfaceReferenceId,
                name:
                    string.Empty,
                placementReferenceId:
                    PlacementReferenceId,
                radius:
                    5.0);

        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                cylindricalSurface,
                advancedFace);

        var mapper =
            new StepAdvancedFaceGeometryMapper();

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        AssertSuccessfulResult(
            result);

        Cylinder3D mappedCylinder =
            Assert.IsType<Cylinder3D>(
                result.Value);

        Assert.Equal(
            5.0,
            mappedCylinder.Radius,
            precision:
                10);
    }

    [Fact]
    public void Map_WhenSurfaceIsCone_ReturnsSuccessfulCone3DResult()
    {
        StepCartesianPointEntity location =
            CreateLocation();

        StepDirectionEntity axis =
            CreateAxis();

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection();

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        double semiAngle =
            Math.PI / 6.0;

        var conicalSurface =
            new StepConicalSurfaceEntity(
                sourceEntityId:
                    SurfaceReferenceId,
                name:
                    string.Empty,
                placementReferenceId:
                    PlacementReferenceId,
                radius:
                    5.0,
                semiAngle:
                    semiAngle);

        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                conicalSurface,
                advancedFace);

        var mapper =
            new StepAdvancedFaceGeometryMapper();

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        AssertSuccessfulResult(
            result);

        Cone3D mappedCone =
            Assert.IsType<Cone3D>(
                result.Value);

        Assert.Equal(
            semiAngle,
            mappedCone.SemiAngle,
            precision:
                10);
    }

    [Fact]
    public void Map_WhenSurfaceIsSphere_ReturnsSuccessfulSphere3DResult()
    {
        StepCartesianPointEntity location =
            CreateLocation();

        StepDirectionEntity axis =
            CreateAxis();

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection();

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        var sphericalSurface =
            new StepSphericalSurfaceEntity(
                sourceEntityId:
                    SurfaceReferenceId,
                name:
                    string.Empty,
                placementReferenceId:
                    PlacementReferenceId,
                radius:
                    5.0);

        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                sphericalSurface,
                advancedFace);

        var mapper =
            new StepAdvancedFaceGeometryMapper();

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        AssertSuccessfulResult(
            result);

        Sphere3D mappedSphere =
            Assert.IsType<Sphere3D>(
                result.Value);

        Assert.Equal(
            5.0,
            mappedSphere.Radius,
            precision:
                10);
    }

    [Fact]
    public void Map_WhenSurfaceReferenceCannotBeResolved_ReturnsFailedResult()
    {
        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                advancedFace);

        var mapper =
            new StepAdvancedFaceGeometryMapper();

        MappingResult<ISurface3D> result =
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
    public void Map_WhenSurfaceTypeIsUnsupported_ReturnsUnsupportedSurfaceDiagnostic()
    {
        var unsupportedSurface =
            new StepCartesianPointEntity(
                sourceEntityId:
                    SurfaceReferenceId,
                name:
                    string.Empty,
                coordinates:
                    ValidLocationCoordinates);

        StepAdvancedFaceEntity advancedFace =
            CreateAdvancedFace();

        MappingContext context =
            CreateContext(
                unsupportedSurface,
                advancedFace);

        var mapper =
            new StepAdvancedFaceGeometryMapper();

        MappingResult<ISurface3D> result =
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

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-FACE-001",
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);
    }

    private static StepCartesianPointEntity CreateLocation()
    {
        return new StepCartesianPointEntity(
            sourceEntityId:
                LocationReferenceId,
            name:
                string.Empty,
            coordinates:
                ValidLocationCoordinates);
    }

    private static StepDirectionEntity CreateAxis()
    {
        return new StepDirectionEntity(
            sourceEntityId:
                AxisReferenceId,
            name:
                string.Empty,
            ratios:
                ValidAxisRatios);
    }

    private static StepDirectionEntity CreateReferenceDirection()
    {
        return new StepDirectionEntity(
            sourceEntityId:
                ReferenceDirectionReferenceId,
            name:
                string.Empty,
            ratios:
                ValidReferenceDirectionRatios);
    }

    private static StepAxis2Placement3DEntity CreatePlacement()
    {
        return new StepAxis2Placement3DEntity(
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

    private static void AssertSuccessfulResult(
        MappingResult<ISurface3D> result)
    {
        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
            result.Value);

        Assert.Empty(
            result.Diagnostics);
    }
}