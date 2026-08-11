using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCylindricalSurfaceGeometryMapperTests
{
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
    public void Map_WithResolvedPlacement_ReturnsSuccessfulCylinder3DResult()
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

        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
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
            result.Value.AxisOrigin.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            result.Value.AxisOrigin.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            result.Value.AxisOrigin.Z,
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
            5.0,
            result.Value.Radius,
            precision:
                10);
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
                    "#20",
                name:
                    string.Empty,
                locationReferenceId:
                    "#10",
                axisReferenceId:
                    null,
                refDirectionReferenceId:
                    "#12");

        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                location,
                referenceDirection,
                placement,
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
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
                out StepCylindricalSurfaceEntity cylindricalSurface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CYLINDER-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
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
                out StepCylindricalSurfaceEntity cylindricalSurface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CYLINDER-ERROR-001",
                "Error from a previous mapping operation.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
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
    public void Map_WithUnresolvedPlacement_ReturnsFailure()
    {
        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
                context);

        AssertFailedResult(
            result,
            context,
            "STEP_REFERENCE_RESOLUTION_FAILED");
    }

    [Fact]
    public void Map_WithUnresolvedPlacementAndPreExistingDiagnostic_ReturnsOnlyCurrentOperationFailure()
    {
        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                cylindricalSurface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-CYLINDER-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
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
    public void Map_WithLocationContainingInvalidCoordinateCount_ReturnsFailure()
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

        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
                context);

        AssertFailedResult(
            result,
            context,
            "MAP-STEP-CYLINDER-001");
    }

    [Fact]
    public void Map_WithAxisContainingInvalidRatioCount_ReturnsFailure()
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

        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
                context);

        AssertFailedResult(
            result,
            context,
            "MAP-STEP-CYLINDER-002");
    }

    [Fact]
    public void Map_WithReferenceDirectionContainingInvalidRatioCount_ReturnsFailure()
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

        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
                context);

        AssertFailedResult(
            result,
            context,
            "MAP-STEP-CYLINDER-003");
    }

    [Fact]
    public void Map_WithZeroAxis_ReturnsInvalidCylinderFailure()
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

        StepCylindricalSurfaceEntity cylindricalSurface =
            CreateCylindricalSurface();

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection,
                placement,
                cylindricalSurface);

        var mapper =
            new StepCylindricalSurfaceGeometryMapper();

        MappingResult<Cylinder3D> result =
            mapper.Map(
                cylindricalSurface,
                context);

        AssertFailedResult(
            result,
            context,
            "MAP-STEP-CYLINDER-004");
    }

    private static MappingContext CreateValidContext(
        out StepCylindricalSurfaceEntity cylindricalSurface)
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

        cylindricalSurface =
            CreateCylindricalSurface();

        return CreateContext(
            location,
            axis,
            referenceDirection,
            placement,
            cylindricalSurface);
    }

    private static StepCartesianPointEntity CreateLocation(
        IReadOnlyList<double> coordinates)
    {
        return new StepCartesianPointEntity(
            sourceEntityId:
                "#10",
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
                "#11",
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
                "#12",
            name:
                string.Empty,
            ratios:
                ratios);
    }

    private static StepAxis2Placement3DEntity CreatePlacement()
    {
        return new StepAxis2Placement3DEntity(
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
    }

    private static StepCylindricalSurfaceEntity
        CreateCylindricalSurface()
    {
        return new StepCylindricalSurfaceEntity(
            sourceEntityId:
                "#30",
            name:
                string.Empty,
            placementReferenceId:
                "#20",
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

    private static void AssertFailedResult(
        MappingResult<Cylinder3D> result,
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
}