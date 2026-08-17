using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCircleGeometryMapperTests
{
    private const string LocationReferenceId = "#10";
    private const string AxisReferenceId = "#11";
    private const string PlacementReferenceId = "#20";
    private const string CircleReferenceId = "#30";

    [Fact]
    public void Map_WithResolvedPlacement_ReturnsSuccessfulCircle3DResult()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                1.0,
                2.0,
                3.0);

        StepDirectionEntity axis =
            CreateAxis(
                0.0,
                1.0,
                0.0);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
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
            new Point3D(
                1.0,
                2.0,
                3.0),
            result.Value.Center);

        Assert.Equal(
            new Vector3D(
                0.0,
                1.0,
                0.0),
            result.Value.Normal);

        Assert.Equal(
            5.0,
            result.Value.Radius);
    }

    [Fact]
    public void Map_WithoutPlacementAxis_UsesDefaultPositiveZNormal()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                1.0,
                2.0,
                3.0);

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    PlacementReferenceId,
                name:
                    string.Empty,
                locationReferenceId:
                    LocationReferenceId,
                axisReferenceId:
                    null,
                refDirectionReferenceId:
                    null);

        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                location,
                placement,
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
            result.Value);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            result.Value.Normal);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithNonUnitAxis_NormalizesCircleNormal()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                1.0,
                2.0,
                3.0);

        StepDirectionEntity axis =
            CreateAxis(
                0.0,
                5.0,
                0.0);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
            result.Value);

        Assert.Equal(
            new Vector3D(
                0.0,
                1.0,
                0.0),
            result.Value.Normal);
    }

    [Fact]
    public void Map_WithUnresolvedPlacement_ReturnsResolutionFailure()
    {
        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        AssertFailure(
            result,
            context,
            "STEP_REFERENCE_RESOLUTION_FAILED");
    }

    [Fact]
    public void Map_WithInvalidLocationCoordinateCount_ReturnsFailure()
    {
        var location =
            new StepCartesianPointEntity(
                sourceEntityId:
                    LocationReferenceId,
                name:
                    string.Empty,
                coordinates:
                    new[]
                    {
                        1.0,
                        2.0
                    });

        StepDirectionEntity axis =
            CreateAxis(
                0.0,
                1.0,
                0.0);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        AssertFailure(
            result,
            context,
            "MAP-STEP-CIRCLE-001");
    }

    [Fact]
    public void Map_WithInvalidAxisRatioCount_ReturnsFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                1.0,
                2.0,
                3.0);

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    AxisReferenceId,
                name:
                    string.Empty,
                ratios:
                    new[]
                    {
                        0.0,
                        1.0
                    });

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        AssertFailure(
            result,
            context,
            "MAP-STEP-CIRCLE-002");
    }

    [Fact]
    public void Map_WithZeroAxis_ReturnsInvalidCircleFailure()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                1.0,
                2.0,
                3.0);

        StepDirectionEntity axis =
            CreateAxis(
                0.0,
                0.0,
                0.0);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        StepCircleEntity circle =
            CreateCircle();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                circle);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        AssertFailure(
            result,
            context,
            "MAP-STEP-CIRCLE-003");
    }

    [Fact]
    public void Map_WithPreExistingDiagnostic_DoesNotIncludeItInResult()
    {
        MappingContext context =
            CreateValidContext(
                out StepCircleEntity circle);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CIRCLE-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepCircleGeometryMapper();

        MappingResult<Circle3D> result =
            mapper.Map(
                circle,
                context);

        Assert.True(
            result.IsSuccessful);

        Assert.Empty(
            result.Diagnostics);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        MappingContext context =
            CreateContext();

        var mapper =
            new StepCircleGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    null!,
                    context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        StepCircleEntity circle =
            CreateCircle();

        var mapper =
            new StepCircleGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    circle,
                    null!));
    }

    private static MappingContext CreateValidContext(
        out StepCircleEntity circle)
    {
        StepCartesianPointEntity location =
            CreateLocation(
                1.0,
                2.0,
                3.0);

        StepDirectionEntity axis =
            CreateAxis(
                0.0,
                1.0,
                0.0);

        StepAxis2Placement3DEntity placement =
            CreatePlacement();

        circle =
            CreateCircle();

        return CreateContext(
            location,
            axis,
            placement,
            circle);
    }

    private static StepCartesianPointEntity CreateLocation(
        double x,
        double y,
        double z)
    {
        return new StepCartesianPointEntity(
            sourceEntityId:
                LocationReferenceId,
            name:
                string.Empty,
            coordinates:
                new[]
                {
                    x,
                    y,
                    z
                });
    }

    private static StepDirectionEntity CreateAxis(
        double x,
        double y,
        double z)
    {
        return new StepDirectionEntity(
            sourceEntityId:
                AxisReferenceId,
            name:
                string.Empty,
            ratios:
                new[]
                {
                    x,
                    y,
                    z
                });
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
                null);
    }

    private static StepCircleEntity CreateCircle()
    {
        return new StepCircleEntity(
            sourceEntityId:
                CircleReferenceId,
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

    private static void AssertFailure(
        MappingResult<Circle3D> result,
        MappingContext context,
        string expectedDiagnosticCode)
    {
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
            expectedDiagnosticCode,
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }
}