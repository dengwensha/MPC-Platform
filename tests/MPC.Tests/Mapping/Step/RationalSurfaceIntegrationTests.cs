using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class RationalSurfaceIntegrationTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Geometry_ExistingConstructors_MapBSplineWithoutCanonicalResolver(int constructorSize)
    {
        StepRationalBSplineSurfaceEntity surface = CreateSurface();
        var context = CreateContext(surface, CreatePoints());
        MappingResult<ISurface3D> result = CreateGeometryMapper(constructorSize)
            .Map(CreateFace(), context);

        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.True(result.IsSuccessful);
        AssertGeometry(Assert.IsType<RationalBSplineSurface3D>(result.Value));
        Assert.Empty(result.Diagnostics);
        Assert.Empty(context.Diagnostics);
        Assert.Null(context.CanonicalReferenceResolver);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Face_ValidCanonicalBoundary_PreservesSurfaceAndSameSense(bool sameSense)
    {
        StepRationalBSplineSurfaceEntity surface = CreateSurface();
        var bound = new StepFaceOuterBoundEntity("#90", "outer", "#91", true);
        MappingContext context = CreateContext(surface, CreatePoints().Append(bound));
        Loop loop = CreateTriangleLoop();
        var boundaryMapper = new BoundaryMapper(loop);
        var mapper = new StepAdvancedFaceMapper(
            new StepAdvancedFaceBuilderMapper(CreateGeometryMapper()), boundaryMapper);

        MappingResult<Face> result = mapper.Map(CreateFace(sameSense), context);

        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.True(result.IsSuccessful);
        Face face = Assert.IsType<Face>(result.Value);
        AssertGeometry(Assert.IsType<RationalBSplineSurface3D>(face.Surface));
        Assert.Equal(sameSense ? FaceOrientation.Forward : FaceOrientation.Reversed, face.Orientation);
        Assert.Same(loop, face.OuterLoop);
        Assert.Empty(face.InnerLoops);
        Assert.Equal(1, boundaryMapper.CallCount);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Face_InvalidSurfacePoint_StopsBeforeBoundaryResolutionOrMapping()
    {
        StepRationalBSplineSurfaceEntity surface = CreateSurface();
        StepSemanticEntity[] points = CreatePoints().ToArray();
        points[3] = new StepCartesianPointEntity("#4", string.Empty, [1.0, 2.0]);
        // #90 is intentionally missing: its diagnostic must never be produced.
        MappingContext context = CreateContext(surface, points);
        var boundaryMapper = new BoundaryMapper(CreateTriangleLoop());
        var mapper = new StepAdvancedFaceMapper(
            new StepAdvancedFaceBuilderMapper(CreateGeometryMapper()), boundaryMapper);

        MappingResult<Face> result = mapper.Map(CreateFace(), context);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        MappingDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("MAP-STEP-BSPLINE-SURFACE-001", diagnostic.Code);
        Assert.Contains("#4", diagnostic.Message);
        Assert.Contains("#75", diagnostic.Message);
        Assert.Equal(0, boundaryMapper.CallCount);
        Assert.Same(diagnostic, Assert.Single(context.Diagnostics));
    }

    [Theory]
    [InlineData(false, "STEP_REFERENCE_RESOLUTION_FAILED")]
    [InlineData(true, "STEP_REFERENCE_TYPE_MISMATCH")]
    public void Builder_ControlPointFailure_PreservesResolverDiagnostic(
        bool wrongType, string expectedCode)
    {
        StepRationalBSplineSurfaceEntity surface = CreateSurface();
        var points = CreatePoints().Where(p => p.SourceEntityId != "#4").ToList();
        if (wrongType)
        {
            points.Add(new StepDirectionEntity("#4", string.Empty, [1.0, 0.0, 0.0]));
        }
        MappingContext context = CreateContext(surface, points);
        MappingDiagnostic expected = Assert.Single(
            context.ReferenceResolver.Resolve<StepCartesianPointEntity>("#4").Diagnostics);

        var result = new StepAdvancedFaceBuilderMapper(CreateGeometryMapper())
            .Map(CreateFace(), context);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        MappingDiagnostic actual = Assert.Single(result.Diagnostics);
        Assert.Equal(expectedCode, actual.Code);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.Severity, actual.Severity);
        Assert.Same(actual, Assert.Single(context.Diagnostics));
    }

    [Fact]
    public void Geometry_PreExistingError_DoesNotContaminateSuccessfulResult()
    {
        MappingContext context = CreateContext(CreateSurface(), CreatePoints());
        var prior = new MappingDiagnostic("TEST-PRIOR", "Earlier failure", MappingDiagnosticSeverity.Error);
        context.AddDiagnostic(prior);

        MappingResult<ISurface3D> result = CreateGeometryMapper().Map(CreateFace(), context);

        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.IsType<RationalBSplineSurface3D>(result.Value);
        Assert.Empty(result.Diagnostics);
        Assert.Same(prior, Assert.Single(context.Diagnostics));
    }

    [Fact]
    public void Geometry_PreExistingError_DoesNotContaminateCurrentFailure()
    {
        MappingContext context = CreateContext(CreateSurface(), CreatePoints().Skip(1));
        var prior = new MappingDiagnostic("TEST-PRIOR", "Earlier failure", MappingDiagnosticSeverity.Error);
        context.AddDiagnostic(prior);

        MappingResult<ISurface3D> result = CreateGeometryMapper().Map(CreateFace(), context);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.Null(result.Value);
        MappingDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("STEP_REFERENCE_RESOLUTION_FAILED", diagnostic.Code);
        Assert.Equal(2, context.Diagnostics.Count);
        Assert.Same(prior, context.Diagnostics[0]);
        Assert.Same(diagnostic, context.Diagnostics[1]);
    }

    [Fact]
    public void ProductionOperation_ValidBSpline_ReachesMissingBoundary()
    {
        // Deliberately incomplete topology verifies production wiring, not solid success.
        StepRationalBSplineSurfaceEntity surface = CreateSurface();
        StepAdvancedFaceEntity face = CreateFace();
        var shell = new StepClosedShellEntity("#100", "shell", ["#80"]);
        var solid = new StepManifoldSolidBrepEntity("#110", "solid", "#100");
        var resolver = new StepSemanticEntityResolver(
            CreatePoints().Concat(new StepSemanticEntity[] { surface, face, shell, solid }));
        StepCanonicalMappingOperation operation = StepCanonicalMappingOperation.Create(resolver);

        MappingResult<Solid> result = operation.Map(solid);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "STEP_REFERENCE_RESOLUTION_FAILED" &&
            diagnostic.Message.Contains("#90", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "MAP-STEP-FACE-001");
    }

    [Fact]
    public void ProductionOperation_InvalidBSpline_PropagatesPointFailureBeforeBoundary()
    {
        StepRationalBSplineSurfaceEntity surface = CreateSurface();
        StepSemanticEntity[] points = CreatePoints().ToArray();
        points[3] = new StepCartesianPointEntity("#4", string.Empty, [1.0, 2.0]);
        var shell = new StepClosedShellEntity("#100", "shell", ["#80"]);
        var solid = new StepManifoldSolidBrepEntity("#110", "solid", "#100");
        var resolver = new StepSemanticEntityResolver(
            points.Concat(new StepSemanticEntity[] { surface, CreateFace(), shell, solid }));
        StepCanonicalMappingOperation operation = StepCanonicalMappingOperation.Create(resolver);

        MappingResult<Solid> result = operation.Map(solid);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "MAP-STEP-BSPLINE-SURFACE-001" &&
            diagnostic.Message.Contains("#4", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "MAP-STEP-FACE-001");
        Assert.DoesNotContain(result.Diagnostics, diagnostic =>
            diagnostic.Message.Contains("#90", StringComparison.Ordinal));
    }

    private static StepAdvancedFaceGeometryMapper CreateGeometryMapper(int size = 6)
    {
        var plane = new StepPlaneGeometryMapper();
        var cylinder = new StepCylindricalSurfaceGeometryMapper();
        var cone = new StepConicalSurfaceGeometryMapper();
        var sphere = new StepSphericalSurfaceGeometryMapper();
        return size switch
        {
            4 => new StepAdvancedFaceGeometryMapper(plane, cylinder, cone, sphere),
            5 => new StepAdvancedFaceGeometryMapper(plane, cylinder, cone, sphere,
                new StepDegenerateToroidalSurfaceGeometryMapper()),
            6 => new StepAdvancedFaceGeometryMapper(plane, cylinder, cone, sphere,
                new StepDegenerateToroidalSurfaceGeometryMapper(), new StepToroidalSurfaceGeometryMapper()),
            _ => throw new ArgumentOutOfRangeException(nameof(size))
        };
    }

    private static MappingContext CreateContext(
        StepRationalBSplineSurfaceEntity surface, IEnumerable<StepSemanticEntity> points)
    {
        return new MappingContext(new StepReferenceResolver(
            new StepSemanticEntityResolver(points.Append(surface))));
    }

    private static StepAdvancedFaceEntity CreateFace(bool sameSense = true)
    {
        return new StepAdvancedFaceEntity("#80", "face", ["#90"], "#75", sameSense);
    }

    private static StepRationalBSplineSurfaceEntity CreateSurface()
    {
        var basis = new StepBSplineSurfaceWithKnotsEntity(
            "#75", "surface", 3, 1,
            new string[][] { ["#1", "#2"], ["#3", "#4"], ["#5", "#6"], ["#7", "#8"] },
            "UNSPECIFIED", StepLogicalValue.False, StepLogicalValue.False, StepLogicalValue.Unknown,
            [4L, 4L], [2L, 2L], [-2.0, 5.0], [-3.0, 7.0], "UNSPECIFIED");
        return new StepRationalBSplineSurfaceEntity(basis,
            new double[][] { [1, 2], [3, 4], [5, 6], [7, 8] });
    }

    private static IEnumerable<StepSemanticEntity> CreatePoints()
    {
        for (int u = 0; u < 4; u++)
        {
            for (int v = 0; v < 2; v++)
            {
                yield return new StepCartesianPointEntity(
                    $"#{u * 2 + v + 1}", string.Empty, [u / 3.0, (double)v, 0.0]);
            }
        }
    }

    private static void AssertGeometry(RationalBSplineSurface3D rational)
    {
        Assert.Equal(Enumerable.Range(1, 8).Select(x => (double)x), rational.Weights.SelectMany(row => row));
        BSplineSurface3D surface = rational.Basis;
        Assert.Equal(3, surface.UDegree);
        Assert.Equal(1, surface.VDegree);
        Assert.Equal(new[] { 4, 4 }, surface.UMultiplicities.ToArray());
        Assert.Equal(new[] { 2, 2 }, surface.VMultiplicities.ToArray());
        Assert.Equal(new[] { -2.0, 5.0 }, surface.UKnots.ToArray());
        Assert.Equal(new[] { -3.0, 7.0 }, surface.VKnots.ToArray());
        Assert.Equal(4, surface.ControlPointGrid.Count);
        for (int u = 0; u < 4; u++)
        {
            Assert.Equal(2, surface.ControlPointGrid[u].Count);
            for (int v = 0; v < 2; v++)
            {
                Assert.Equal(new Point3D(u / 3.0, v, 0.0), surface.ControlPointGrid[u][v]);
            }
        }
    }

    private static Loop CreateTriangleLoop()
    {
        var a = new Vertex(new Point3D(0.0, 0.0, 0.0));
        var b = new Vertex(new Point3D(1.0, 0.0, 0.0));
        var c = new Vertex(new Point3D(0.0, 1.0, 0.0));
        return new Loop(new[]
        {
            new EdgeUse(new Edge(a, b, new TestCurve3D()), EdgeUseDirection.Forward),
            new EdgeUse(new Edge(b, c, new TestCurve3D()), EdgeUseDirection.Forward),
            new EdgeUse(new Edge(c, a, new TestCurve3D()), EdgeUseDirection.Forward)
        });
    }

    private sealed class TestCurve3D : ICurve3D { }

    private sealed class BoundaryMapper : IEntityMapper<StepFaceBoundEntity, FaceLoopMapping>
    {
        private readonly Loop _loop;
        public int CallCount { get; private set; }

        public BoundaryMapper(Loop loop) { _loop = loop; }

        public MappingResult<FaceLoopMapping> Map(StepFaceBoundEntity source, MappingContext context)
        {
            CallCount++;
            return MappingResult<FaceLoopMapping>.Success(
                new FaceLoopMapping(_loop, true, source.Orientation));
        }
    }
}
