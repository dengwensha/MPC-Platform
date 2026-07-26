using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepSphericalSurfaceGeometryMapperTests
{
    private const string LocationReferenceId =
        "#10";

    private const string AxisReferenceId =
        "#11";

    private const string ReferenceDirectionReferenceId =
        "#12";

    private const string PlacementReferenceId =
        "#20";

    private const string SphericalSurfaceReferenceId =
        "#30";

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
    public void Map_WithResolvedPlacement_ReturnsSuccessfulSphere3DResult()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity axis =
            CreateAxis(
                ValidAxisRatios);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
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
            result.Value.Center.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Value.Center.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            result.Value.Center.Z,
            precision:
                10);

        Assert.Equal(
            5.0,
            result.Value.Radius,
            precision:
                10);
    }

    [Fact]
    public void Map_WhenLocationDoesNotContainThreeCoordinates_ReturnsFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                [
                    1.0,
                    2.0
                ]);

        StepDirectionEntity axis =
            CreateAxis(
                ValidAxisRatios);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            "MAP-STEP-SPHERE-001");

        Assert.True(
            context.HasDiagnostics);
    }

    [Fact]
    public void Map_WhenAxisDoesNotContainThreeRatios_ReturnsFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity axis =
            CreateAxis(
                [
                    0.0,
                    1.0
                ]);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            "MAP-STEP-SPHERE-002");

        Assert.True(
            context.HasDiagnostics);
    }

    [Fact]
    public void Map_WhenReferenceDirectionDoesNotContainThreeRatios_ReturnsFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity axis =
            CreateAxis(
                ValidAxisRatios);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                [
                    1.0,
                    0.0
                ]);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            "MAP-STEP-SPHERE-003");

        Assert.True(
            context.HasDiagnostics);
    }

    [Fact]
    public void Map_WhenAxisIsZeroLength_ReturnsInvalidSphereFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity axis =
            CreateAxis(
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            "MAP-STEP-SPHERE-004");

        Assert.True(
            context.HasDiagnostics);
    }

    [Fact]
    public void Map_WhenPlacementCannotBeResolved_ReturnsFailure()
    {
        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertResolutionFailure(
            result,
            context);
    }

    [Fact]
    public void Map_WhenLocationCannotBeResolved_ReturnsFailure()
    {
        StepDirectionEntity axis =
            CreateAxis(
                ValidAxisRatios);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                axis,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertResolutionFailure(
            result,
            context);
    }

    [Fact]
    public void Map_WhenAxisCannotBeResolved_ReturnsFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                referenceDirection,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertResolutionFailure(
            result,
            context);
    }

    [Fact]
    public void Map_WhenReferenceDirectionCannotBeResolved_ReturnsFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity axis =
            CreateAxis(
                ValidAxisRatios);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepSphericalSurfaceEntity sphericalSurface =
            CreateSphericalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                sphericalSurface);

        var mapper =
            new StepSphericalSurfaceGeometryMapper();

        MappingResult<Sphere3D> result =
            mapper.Map(
                sphericalSurface,
                context);

        AssertResolutionFailure(
            result,
            context);
    }

    private static StepCartesianPointEntity CreateLocation(
        IReadOnlyList<double> coordinates)
    {
        return new StepCartesianPointEntity(
            sourceEntityId:
                LocationReferenceId,
            name:
                string.Empty,
            coordinates:
                coordinates);
    }

    private static StepDirectionEntity CreateAxis(
        IReadOnlyList<double> ratios)
    {
        return new StepDirectionEntity(
            sourceEntityId:
                AxisReferenceId,
            name:
                string.Empty,
            ratios:
                ratios);
    }

    private static StepDirectionEntity CreateReferenceDirection(
        IReadOnlyList<double> ratios)
    {
        return new StepDirectionEntity(
            sourceEntityId:
                ReferenceDirectionReferenceId,
            name:
                string.Empty,
            ratios:
                ratios);
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

    private static StepSphericalSurfaceEntity CreateSphericalSurface()
    {
        return new StepSphericalSurfaceEntity(
            sourceEntityId:
                SphericalSurfaceReferenceId,
            name:
                string.Empty,
            placementReferenceId:
                PlacementReferenceId,
            radius:
                5.0);
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

    private static void AssertFailureWithDiagnostic(
        MappingResult<Sphere3D> result,
        string expectedDiagnosticCode)
    {
        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code ==
                expectedDiagnosticCode);
    }

    private static void AssertResolutionFailure(
        MappingResult<Sphere3D> result,
        MappingContext context)
    {
        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        Assert.NotEmpty(
            result.Diagnostics);

        Assert.True(
            context.HasDiagnostics);
    }
}