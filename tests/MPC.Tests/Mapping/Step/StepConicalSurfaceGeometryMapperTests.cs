using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepConicalSurfaceGeometryMapperTests
{
    private const string LocationReferenceId =
        "#10";

    private const string AxisReferenceId =
        "#11";

    private const string ReferenceDirectionReferenceId =
        "#12";

    private const string PlacementReferenceId =
        "#20";

    private const string ConicalSurfaceReferenceId =
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
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        MappingContext context =
            CreateContext();

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    null!,
                    context));
    }

    [Fact]
    public void Map_WithResolvedPlacement_ReturnsSuccessfulCone3DResult()
    {
        MappingContext context =
            CreateValidContext(
                out StepConicalSurfaceEntity conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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
            result.Value.Apex.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Value.Apex.Y,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Value.Apex.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.AxisDirection.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.AxisDirection.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            result.Value.AxisDirection.Z,
            precision:
                10);

        Assert.Equal(
            Math.PI / 4.0,
            result.Value.SemiAngle,
            precision:
                10);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithNegativeSemiAngle_NormalizesAngleAndReversesAxis()
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

        var conicalSurface =
            new StepConicalSurfaceEntity(
                sourceEntityId:
                    ConicalSurfaceReferenceId,
                name:
                    string.Empty,
                placementReferenceId:
                    PlacementReferenceId,
                radius:
                    1.0,
                semiAngle:
                    -Math.PI / 4.0);

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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
            result.Value.Apex.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Value.Apex.Y,
            precision:
                10);

        Assert.Equal(
            4.0,
            result.Value.Apex.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.AxisDirection.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.AxisDirection.Y,
            precision:
                10);

        Assert.Equal(
            -1.0,
            result.Value.AxisDirection.Z,
            precision:
                10);

        Assert.Equal(
            Math.PI / 4.0,
            result.Value.SemiAngle,
            precision:
                10);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithoutPlacementAxis_UsesDefaultPositiveZAxis()
    {
        StepCartesianPointEntity location =
            CreateLocation(
                ValidLocationCoordinates);

        StepDirectionEntity referenceDirection =
            CreateReferenceDirection(
                ValidReferenceDirectionRatios);

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
                    ReferenceDirectionReferenceId);

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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
            0.0,
            result.Value.AxisDirection.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            result.Value.AxisDirection.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            result.Value.AxisDirection.Z,
            precision:
                10);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithPreExistingWarning_DoesNotIncludeItInSuccessfulResult()
    {
        MappingContext context =
            CreateValidContext(
                out StepConicalSurfaceEntity conicalSurface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CONE-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WithPreExistingError_DoesNotFailSuccessfulOperation()
    {
        MappingContext context =
            CreateValidContext(
                out StepConicalSurfaceEntity conicalSurface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CONE-ERROR-001",
                "Error from a previous mapping operation.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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

        Assert.True(
            context.HasErrors);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            context,
            "MAP-STEP-CONE-001");
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            context,
            "MAP-STEP-CONE-002");
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            context,
            "MAP-STEP-CONE-003");
    }

    [Fact]
    public void Map_WhenAxisIsZeroLength_ReturnsInvalidConeFailure()
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        AssertFailureWithDiagnostic(
            result,
            context,
            "MAP-STEP-CONE-004");
    }

    [Fact]
    public void Map_WhenPlacementCannotBeResolved_ReturnsFailure()
    {
        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        AssertResolutionFailure(
            result,
            context);
    }

    [Fact]
    public void Map_WhenPlacementCannotBeResolvedWithPreExistingDiagnostic_ReturnsOnlyCurrentOperationFailure()
    {
        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                conicalSurface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CONE-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "STEP_REFERENCE_RESOLUTION_FAILED",
            resultDiagnostic.Code);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            existingDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            resultDiagnostic,
            context.Diagnostics[1]);
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                axis,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                referenceDirection,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
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

        StepConicalSurfaceEntity conicalSurface =
            CreateConicalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                placement,
                conicalSurface);

        var mapper =
            new StepConicalSurfaceGeometryMapper();

        MappingResult<Cone3D> result =
            mapper.Map(
                conicalSurface,
                context);

        AssertResolutionFailure(
            result,
            context);
    }

    private static MappingContext CreateValidContext(
        out StepConicalSurfaceEntity conicalSurface)
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

        conicalSurface =
            CreateConicalSurface();

        return CreateContext(
            location,
            axis,
            referenceDirection,
            placement,
            conicalSurface);
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

    private static StepConicalSurfaceEntity CreateConicalSurface()
    {
        return new StepConicalSurfaceEntity(
            sourceEntityId:
                ConicalSurfaceReferenceId,
            name:
                string.Empty,
            placementReferenceId:
                PlacementReferenceId,
            radius:
                1.0,
            semiAngle:
                Math.PI / 4.0);
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
        MappingResult<Cone3D> result,
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

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            expectedDiagnosticCode,
            resultDiagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            resultDiagnostic.Severity);

        Assert.Same(
            resultDiagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    private static void AssertResolutionFailure(
        MappingResult<Cone3D> result,
        MappingContext context)
    {
        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "STEP_REFERENCE_RESOLUTION_FAILED",
            resultDiagnostic.Code);

        Assert.Same(
            resultDiagnostic,
            Assert.Single(
                context.Diagnostics));
    }
}