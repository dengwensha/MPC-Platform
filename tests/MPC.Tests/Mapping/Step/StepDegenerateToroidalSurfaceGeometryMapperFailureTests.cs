using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepDegenerateToroidalSurfaceGeometryMapperFailureTests
{
    private const string LocationId = "#10";
    private const string AxisId = "#11";
    private const string ReferenceDirectionId = "#12";
    private const string PlacementId = "#20";
    private const string SurfaceId = "#30";

    [Fact]
    public void Map_WhenLocationHasInvalidCoordinateCount_ReturnsLocationFailure()
    {
        MappingContext context =
            CreateContext(
                CreateLocation([1.0, 2.0]),
                CreateAxis([0.0, 0.0, 1.0]),
                CreateReferenceDirection([1.0, 0.0, 0.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "MAP-STEP-DEGENERATE-TORUS-001");
    }

    [Fact]
    public void Map_WhenAxisHasInvalidRatioCount_ReturnsAxisFailure()
    {
        MappingContext context =
            CreateContext(
                CreateLocation([1.0, 2.0, 3.0]),
                CreateAxis([0.0, 1.0]),
                CreateReferenceDirection([1.0, 0.0, 0.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "MAP-STEP-DEGENERATE-TORUS-002");
    }

    [Fact]
    public void Map_WhenReferenceDirectionHasInvalidRatioCount_ReturnsReferenceDirectionFailure()
    {
        MappingContext context =
            CreateContext(
                CreateLocation([1.0, 2.0, 3.0]),
                CreateAxis([0.0, 0.0, 1.0]),
                CreateReferenceDirection([1.0, 0.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "MAP-STEP-DEGENERATE-TORUS-003");
    }

    [Fact]
    public void Map_WhenAxisIsZeroLength_ReturnsInvalidDegenerateTorusFailure()
    {
        MappingContext context =
            CreateContext(
                CreateLocation([1.0, 2.0, 3.0]),
                CreateAxis([0.0, 0.0, 0.0]),
                CreateReferenceDirection([1.0, 0.0, 0.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "MAP-STEP-DEGENERATE-TORUS-004");
    }

    [Fact]
    public void Map_WhenLocationCannotBeResolved_ReturnsResolutionFailure()
    {
        MappingContext context =
            CreateContext(
                CreateAxis([0.0, 0.0, 1.0]),
                CreateReferenceDirection([1.0, 0.0, 0.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "STEP_REFERENCE_RESOLUTION_FAILED");
    }

    [Fact]
    public void Map_WhenAxisCannotBeResolved_ReturnsResolutionFailure()
    {
        MappingContext context =
            CreateContext(
                CreateLocation([1.0, 2.0, 3.0]),
                CreateReferenceDirection([1.0, 0.0, 0.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "STEP_REFERENCE_RESOLUTION_FAILED");
    }

    [Fact]
    public void Map_WhenReferenceDirectionCannotBeResolved_ReturnsResolutionFailure()
    {
        MappingContext context =
            CreateContext(
                CreateLocation([1.0, 2.0, 3.0]),
                CreateAxis([0.0, 0.0, 1.0]),
                CreatePlacement(),
                CreateSurface());

        AssertFailure(
            Map(context),
            context,
            "STEP_REFERENCE_RESOLUTION_FAILED");
    }

    [Fact]
    public void Map_WhenResolutionFailsWithPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        MappingContext context =
            CreateContext(
                CreateSurface());

        var preExistingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-DEGENERATE-TORUS-FAILURE-001",
                "Diagnostic from an earlier operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            preExistingDiagnostic);

        MappingResult<DegenerateTorus3D> result =
            Map(
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

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
            preExistingDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            resultDiagnostic,
            context.Diagnostics[1]);
    }

    private static MappingResult<DegenerateTorus3D> Map(
        MappingContext context)
    {
        StepDegenerateToroidalSurfaceEntity surface =
            context.ReferenceResolver
                .Resolve<StepDegenerateToroidalSurfaceEntity>(
                    SurfaceId)
                .Value
            ?? throw new InvalidOperationException(
                "Test surface must resolve.");

        return new StepDegenerateToroidalSurfaceGeometryMapper()
            .Map(
                surface,
                context);
    }

    private static StepCartesianPointEntity CreateLocation(
        IReadOnlyList<double> coordinates)
    {
        return new StepCartesianPointEntity(
            LocationId,
            string.Empty,
            coordinates);
    }

    private static StepDirectionEntity CreateAxis(
        IReadOnlyList<double> ratios)
    {
        return new StepDirectionEntity(
            AxisId,
            string.Empty,
            ratios);
    }

    private static StepDirectionEntity CreateReferenceDirection(
        IReadOnlyList<double> ratios)
    {
        return new StepDirectionEntity(
            ReferenceDirectionId,
            string.Empty,
            ratios);
    }

    private static StepAxis2Placement3DEntity CreatePlacement()
    {
        return new StepAxis2Placement3DEntity(
            PlacementId,
            string.Empty,
            LocationId,
            AxisId,
            ReferenceDirectionId);
    }

    private static StepDegenerateToroidalSurfaceEntity CreateSurface()
    {
        return new StepDegenerateToroidalSurfaceEntity(
            SurfaceId,
            string.Empty,
            PlacementId,
            178.5,
            229.5,
            selectOuter:
                true);
    }

    private static MappingContext CreateContext(
        params StepSemanticEntity[] entities)
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        return new MappingContext(
            new StepReferenceResolver(
                semanticResolver));
    }

    private static void AssertFailure(
        MappingResult<DegenerateTorus3D> result,
        MappingContext context,
        string expectedCode)
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
            expectedCode,
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
