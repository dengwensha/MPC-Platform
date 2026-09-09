using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepBSplineSurfaceWithKnotsGeometryMapperTests
{
    private const string SurfaceId = "#75";

    [Fact]
    public void Map_WithRealSemanticResolver_PreservesAsymmetricSurfaceExactly()
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        MappingContext context = CreateContext(source, CreatePoints());
        StepBSplineSurfaceWithKnotsEntity resolvedSource =
            context.ReferenceResolver
                .Resolve<StepBSplineSurfaceWithKnotsEntity>(SurfaceId).Value
            ?? throw new InvalidOperationException("Test surface must resolve.");

        IEntityMapper<StepBSplineSurfaceWithKnotsEntity, BSplineSurface3D> mapper =
            new StepBSplineSurfaceWithKnotsGeometryMapper();
        MappingResult<BSplineSurface3D> result = mapper.Map(resolvedSource, context);
        BSplineSurface3D surface = AssertSuccess(result);

        Assert.Null(context.CanonicalReferenceResolver);
        Assert.Empty(context.Diagnostics);
        Assert.IsAssignableFrom<ISurface3D>(surface);
        Assert.Equal(2, surface.UDegree);
        Assert.Equal(1, surface.VDegree);
        Assert.Equal(new[] { 3, 1, 3 }, surface.UMultiplicities.ToArray());
        Assert.Equal(new[] { 2, 2 }, surface.VMultiplicities.ToArray());
        Assert.Equal(new[] { -2.0, 0.5, 5.0 }, surface.UKnots.ToArray());
        Assert.Equal(new[] { -3.0, 7.0 }, surface.VKnots.ToArray());
        Assert.Equal(4, surface.ControlPointGrid.Count);

        for (int u = 0; u < 4; u++)
        {
            Assert.Equal(2, surface.ControlPointGrid[u].Count);
            for (int v = 0; v < 2; v++)
            {
                int index = u * 2 + v;
                Assert.Equal(ExpectedPoint(index), surface.ControlPointGrid[u][v]);
            }
        }
    }

    [Fact]
    public void Map_WithRepeatedReference_PreservesEveryGridPosition()
    {
        string[][] grid = CreateGrid();
        grid[3][1] = "#1";
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface(grid);
        MappingContext context = CreateContext(source, CreatePoints());

        BSplineSurface3D surface = AssertSuccess(Map(source, context));

        Assert.Equal(4, surface.ControlPointGrid.Count);
        Assert.All(surface.ControlPointGrid, row => Assert.Equal(2, row.Count));
        Assert.Equal(ExpectedPoint(0), surface.ControlPointGrid[0][0]);
        Assert.Equal(ExpectedPoint(0), surface.ControlPointGrid[3][1]);
        Assert.Equal(ExpectedPoint(6), surface.ControlPointGrid[3][0]);
        Assert.Equal("#1", source.ControlPointReferenceGrid[3][1]);
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Map_WithDifferentReferencesHavingEqualCoordinates_PreservesGridShape()
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        StepSemanticEntity[] points = CreatePoints();
        points[7] = new StepCartesianPointEntity("#8", string.Empty, [0.125, -0.25, 0.5]);
        MappingContext context = CreateContext(source, points);

        BSplineSurface3D surface = AssertSuccess(Map(source, context));

        Assert.Equal(4, surface.ControlPointGrid.Count);
        Assert.All(surface.ControlPointGrid, row => Assert.Equal(2, row.Count));
        Assert.Equal(surface.ControlPointGrid[0][0], surface.ControlPointGrid[3][1]);
        Assert.Equal("#1", source.ControlPointReferenceGrid[0][0]);
        Assert.Equal("#8", source.ControlPointReferenceGrid[3][1]);
        Assert.Empty(context.Diagnostics);
    }

    [Theory]
    [InlineData(StepLogicalValue.False)]
    [InlineData(StepLogicalValue.True)]
    [InlineData(StepLogicalValue.Unknown)]
    public void Map_PreservesSemanticMetadataWithoutChangingCanonicalMathematics(
        StepLogicalValue logicalValue)
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface(logicalValue: logicalValue);
        StepBSplineSurfaceWithKnotsEntity baseline = CreateSurface();

        BSplineSurface3D actual = AssertSuccess(Map(source, CreateContext(source, CreatePoints())));
        BSplineSurface3D expected = AssertSuccess(Map(baseline, CreateContext(baseline, CreatePoints())));

        Assert.Equal(expected, actual);
        Assert.Equal(SurfaceId, source.SourceEntityId);
        Assert.Equal("B_SPLINE_SURFACE_WITH_KNOTS", source.SourceEntityType);
        Assert.Equal("source metadata", source.Name);
        Assert.Equal("UNSPECIFIED", source.SurfaceForm);
        Assert.Equal("UNSPECIFIED", source.KnotSpecification);
        Assert.Equal(logicalValue, source.UClosed);
        Assert.Equal(logicalValue, source.VClosed);
        Assert.Equal(logicalValue, source.SelfIntersect);
    }

    [Theory]
    [InlineData(0, "#1")]
    [InlineData(3, "#4")]
    [InlineData(7, "#8")]
    public void Map_WithMissingControlPoint_ReturnsResolverFailureWithoutPartialSurface(
        int missingIndex, string missingReference)
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        StepSemanticEntity[] points = CreatePoints()
            .Where((_, index) => index != missingIndex).ToArray();
        MappingContext context = CreateContext(source, points);

        MappingDiagnostic diagnostic = AssertFailure(
            Map(source, context), context, "STEP_REFERENCE_RESOLUTION_FAILED");

        Assert.Contains(missingReference, diagnostic.Message);
    }

    [Fact]
    public void Map_WithWrongControlPointEntityType_PreservesResolverDiagnostic()
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        StepSemanticEntity[] points = CreatePoints();
        points[3] = new StepDirectionEntity("#4", string.Empty, [1.0, 0.0, 0.0]);
        MappingContext context = CreateContext(source, points);
        MappingDiagnostic expected = Assert.Single(
            context.ReferenceResolver.Resolve<StepCartesianPointEntity>("#4").Diagnostics);

        MappingDiagnostic actual = AssertFailure(
            Map(source, context), context, "STEP_REFERENCE_TYPE_MISMATCH");

        Assert.Equal(expected.Code, actual.Code);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.Severity, actual.Severity);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void Map_WithNonThreeDimensionalControlPoint_ReturnsIdentifiableFailure(
        int coordinateCount)
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        StepSemanticEntity[] points = CreatePoints();
        points[3] = new StepCartesianPointEntity(
            "#4", string.Empty, Enumerable.Repeat(1.0, coordinateCount));
        MappingContext context = CreateContext(source, points);

        MappingDiagnostic diagnostic = AssertFailure(
            Map(source, context), context, "MAP-STEP-BSPLINE-SURFACE-001");

        Assert.Contains(SurfaceId, diagnostic.Message);
        Assert.Contains("#4", diagnostic.Message);
        Assert.Contains("U[1]", diagnostic.Message);
        Assert.Contains("V[1]", diagnostic.Message);
    }

    [Fact]
    public void Map_WithPreExistingError_ReturnsCleanSuccessForCurrentOperation()
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        MappingContext context = CreateContext(source, CreatePoints());
        MappingDiagnostic prior = CreatePriorError();
        context.AddDiagnostic(prior);

        AssertSuccess(Map(source, context));

        Assert.Same(prior, Assert.Single(context.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingErrorAndMissingPoint_ReturnsOnlyCurrentFailure()
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        MappingContext context = CreateContext(source, CreatePoints().Skip(1).ToArray());
        MappingDiagnostic prior = CreatePriorError();
        context.AddDiagnostic(prior);

        MappingResult<BSplineSurface3D> result = Map(source, context);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        MappingDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("STEP_REFERENCE_RESOLUTION_FAILED", diagnostic.Code);
        Assert.Equal(MappingDiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(2, context.Diagnostics.Count);
        Assert.Same(prior, context.Diagnostics[0]);
        Assert.Same(diagnostic, context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WhenSameFailureRepeatsInContext_ReturnsCurrentDiagnosticEachTime()
    {
        StepBSplineSurfaceWithKnotsEntity source = CreateSurface();
        MappingContext context = CreateContext(source, CreatePoints().Skip(1).ToArray());

        MappingResult<BSplineSurface3D> first = Map(source, context);
        MappingResult<BSplineSurface3D> second = Map(source, context);

        Assert.Equal(MappingStatus.Failed, first.Status);
        Assert.Equal(MappingStatus.Failed, second.Status);
        Assert.Null(first.Value);
        Assert.Null(second.Value);
        MappingDiagnostic firstDiagnostic = Assert.Single(first.Diagnostics);
        MappingDiagnostic secondDiagnostic = Assert.Single(second.Diagnostics);
        Assert.Equal("STEP_REFERENCE_RESOLUTION_FAILED", firstDiagnostic.Code);
        Assert.Equal(firstDiagnostic.Code, secondDiagnostic.Code);
        Assert.Equal(firstDiagnostic.Message, secondDiagnostic.Message);
        Assert.Equal(firstDiagnostic.Severity, secondDiagnostic.Severity);
        Assert.Equal(2, context.Diagnostics.Count);
        Assert.Same(firstDiagnostic, context.Diagnostics[0]);
        Assert.Same(secondDiagnostic, context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        MappingContext context = CreateContext(CreateSurface(), CreatePoints());
        Assert.Throws<ArgumentNullException>(() => Map(null!, context));
    }

    [Fact]
    public void Map_WhenContextIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Map(CreateSurface(), null!));
    }

    private static MappingResult<BSplineSurface3D> Map(
        StepBSplineSurfaceWithKnotsEntity source, MappingContext context)
    {
        return new StepBSplineSurfaceWithKnotsGeometryMapper().Map(source, context);
    }

    private static BSplineSurface3D AssertSuccess(MappingResult<BSplineSurface3D> result)
    {
        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Diagnostics);
        return Assert.IsType<BSplineSurface3D>(result.Value);
    }

    private static MappingDiagnostic AssertFailure(
        MappingResult<BSplineSurface3D> result, MappingContext context, string code)
    {
        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        MappingDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(code, diagnostic.Code);
        Assert.Equal(MappingDiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Same(diagnostic, Assert.Single(context.Diagnostics));
        return diagnostic;
    }

    private static MappingDiagnostic CreatePriorError()
    {
        return new MappingDiagnostic(
            "TEST-PRIOR-001", "Error from an earlier operation.", MappingDiagnosticSeverity.Error);
    }

    private static MappingContext CreateContext(
        StepBSplineSurfaceWithKnotsEntity source, StepSemanticEntity[] points)
    {
        var resolver = new StepSemanticEntityResolver(points.Append(source));
        return new MappingContext(new StepReferenceResolver(resolver));
    }

    private static Point3D ExpectedPoint(int index)
    {
        return new Point3D(index + 0.125, -index - 0.25, index * 2.0 + 0.5);
    }

    private static StepSemanticEntity[] CreatePoints()
    {
        return Enumerable.Range(0, 8).Select(index =>
        {
            Point3D point = ExpectedPoint(index);
            return (StepSemanticEntity)new StepCartesianPointEntity(
                $"#{index + 1}", string.Empty, [point.X, point.Y, point.Z]);
        }).ToArray();
    }

    private static string[][] CreateGrid()
    {
        return [["#1", "#2"], ["#3", "#4"], ["#5", "#6"], ["#7", "#8"]];
    }

    private static StepBSplineSurfaceWithKnotsEntity CreateSurface(
        string[][]? grid = null, StepLogicalValue logicalValue = StepLogicalValue.False)
    {
        return new StepBSplineSurfaceWithKnotsEntity(
            sourceEntityId: SurfaceId,
            name: "source metadata",
            uDegree: 2,
            vDegree: 1,
            controlPointReferenceGrid: grid ?? CreateGrid(),
            surfaceForm: "UNSPECIFIED",
            uClosed: logicalValue,
            vClosed: logicalValue,
            selfIntersect: logicalValue,
            uMultiplicities: [3L, 1L, 3L],
            vMultiplicities: [2L, 2L],
            uKnots: [-2.0, 0.5, 5.0],
            vKnots: [-3.0, 7.0],
            knotSpecification: "UNSPECIFIED");
    }
}
