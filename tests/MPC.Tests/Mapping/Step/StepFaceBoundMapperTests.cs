using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepFaceBoundMapperTests
{
    [Fact]
    public void Constructor_WithNullEdgeLoopMapper_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepFaceBoundMapper(
                null!));
    }

    [Fact]
    public void Map_WithFaceBound_ReturnsInnerBoundaryMapping()
    {
        Loop loop =
            CreateTriangleLoop();

        var edgeLoop =
            new StepEdgeLoopEntity(
                "#20",
                "Inner boundary loop",
                new[]
                {
                    "#11",
                    "#12",
                    "#13"
                });

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepEdgeLoopEntity>
                {
                    ["#20"] =
                        edgeLoop
                });

        var loopMapper =
            new StubEdgeLoopMapper(
                MappingResult<Loop>.Success(
                    loop));

        var mapper =
            new StepFaceBoundMapper(
                loopMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Inner boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        FaceLoopMapping mapping =
            Assert.IsType<FaceLoopMapping>(
                result.Value);

        Assert.Same(
            loop,
            mapping.Loop);

        Assert.True(
            mapping.Orientation);

        Assert.False(
            mapping.IsOuter);

        Assert.Same(
            edgeLoop,
            loopMapper.LastSource);

        Assert.Same(
            context,
            loopMapper.LastContext);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            resolver.ResolutionOrder);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithFaceOuterBound_ReturnsOuterBoundaryMapping()
    {
        Loop loop =
            CreateTriangleLoop();

        var edgeLoop =
            new StepEdgeLoopEntity(
                "#20",
                "Outer boundary loop",
                new[]
                {
                    "#11",
                    "#12",
                    "#13"
                });

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepEdgeLoopEntity>
                {
                    ["#20"] =
                        edgeLoop
                });

        var mapper =
            new StepFaceBoundMapper(
                new StubEdgeLoopMapper(
                    MappingResult<Loop>.Success(
                        loop)));

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepFaceOuterBoundEntity(
                "#30",
                "Outer boundary",
                "#20",
                false);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        FaceLoopMapping mapping =
            Assert.IsType<FaceLoopMapping>(
                result.Value);

        Assert.Same(
            loop,
            mapping.Loop);

        Assert.False(
            mapping.Orientation);

        Assert.True(
            mapping.IsOuter);

        Assert.Empty(
            result.Diagnostics);
    }

    [Fact]
    public void Map_PreservesTrueOrientation()
    {
        Loop loop =
            CreateTriangleLoop();

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            CreateSuccessfulMapper(
                loop,
                out MappingContext context)
                .Map(
                    source,
                    context);

        Assert.True(
            Assert.IsType<FaceLoopMapping>(
                result.Value)
                .Orientation);
    }

    [Fact]
    public void Map_PreservesFalseOrientation()
    {
        Loop loop =
            CreateTriangleLoop();

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                false);

        MappingResult<FaceLoopMapping> result =
            CreateSuccessfulMapper(
                loop,
                out MappingContext context)
                .Map(
                    source,
                    context);

        Assert.False(
            Assert.IsType<FaceLoopMapping>(
                result.Value)
                .Orientation);
    }

    [Fact]
    public void Map_WithUnresolvedEdgeLoop_ReturnsFailure()
    {
        var diagnostic =
            new MappingDiagnostic(
                "MAP-REF-EDGE-LOOP-001",
                "Referenced EDGE_LOOP could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepEdgeLoopEntity>(),
                new Dictionary<string, MappingDiagnostic>
                {
                    ["#20"] =
                        diagnostic
                });

        var loopMapper =
            new StubEdgeLoopMapper(
                MappingResult<Loop>.Success(
                    CreateTriangleLoop()));

        var mapper =
            new StepFaceBoundMapper(
                loopMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));

        Assert.Null(
            loopMapper.LastSource);
    }

    [Fact]
    public void Map_WhenEdgeLoopMappingFails_PropagatesDiagnostics()
    {
        var diagnostic =
            new MappingDiagnostic(
                "MAP-STEP-EDGE-LOOP-001",
                "EDGE_LOOP could not be mapped.",
                MappingDiagnosticSeverity.Error);

        var edgeLoop =
            new StepEdgeLoopEntity(
                "#20",
                "Invalid loop",
                new[]
                {
                    "#11",
                    "#12"
                });

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepEdgeLoopEntity>
                {
                    ["#20"] =
                        edgeLoop
                });

        var loopMapper =
            new StubEdgeLoopMapper(
                MappingResult<Loop>.Failure(
                    new[]
                    {
                        diagnostic
                    }));

        var mapper =
            new StepFaceBoundMapper(
                loopMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));

        Assert.Same(
            edgeLoop,
            loopMapper.LastSource);
    }

    [Fact]
    public void Map_WithPreExistingInformation_DoesNotIncludeItInSuccessfulResult()
    {
        Loop loop =
            CreateTriangleLoop();

        StepFaceBoundMapper mapper =
            CreateSuccessfulMapper(
                loop,
                out MappingContext context);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-INFO-001",
                "Existing mapping information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            diagnostic);

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
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

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingError_DoesNotFailSuccessfulOperation()
    {
        Loop loop =
            CreateTriangleLoop();

        StepFaceBoundMapper mapper =
            CreateSuccessfulMapper(
                loop,
                out MappingContext context);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-ERROR-001",
                "A previous mapping operation failed.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            diagnostic);

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
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

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithUnresolvedEdgeLoopAndPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        var currentDiagnostic =
            new MappingDiagnostic(
                "MAP-REF-EDGE-LOOP-001",
                "Referenced EDGE_LOOP could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepEdgeLoopEntity>(),
                new Dictionary<string, MappingDiagnostic>
                {
                    ["#20"] =
                        currentDiagnostic
                });

        var context =
            new MappingContext(
                resolver);

        var priorDiagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-INFO-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            priorDiagnostic);

        var loopMapper =
            new StubEdgeLoopMapper(
                MappingResult<Loop>.Success(
                    CreateTriangleLoop()));

        var mapper =
            new StepFaceBoundMapper(
                loopMapper);

        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        MappingResult<FaceLoopMapping> result =
            mapper.Map(
                source,
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

        Assert.Same(
            currentDiagnostic,
            resultDiagnostic);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            priorDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            currentDiagnostic,
            context.Diagnostics[1]);

        Assert.Null(
            loopMapper.LastSource);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        Loop loop =
            CreateTriangleLoop();

        StepFaceBoundMapper mapper =
            CreateSuccessfulMapper(
                loop,
                out MappingContext context);

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var source =
            new StepFaceBoundEntity(
                "#30",
                "Boundary",
                "#20",
                true);

        var mapper =
            new StepFaceBoundMapper(
                new StubEdgeLoopMapper(
                    MappingResult<Loop>.Success(
                        CreateTriangleLoop())));

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static StepFaceBoundMapper CreateSuccessfulMapper(
        Loop loop,
        out MappingContext context)
    {
        var edgeLoop =
            new StepEdgeLoopEntity(
                "#20",
                "Boundary loop",
                new[]
                {
                    "#11",
                    "#12",
                    "#13"
                });

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepEdgeLoopEntity>
                {
                    ["#20"] =
                        edgeLoop
                });

        context =
            new MappingContext(
                resolver);

        return new StepFaceBoundMapper(
            new StubEdgeLoopMapper(
                MappingResult<Loop>.Success(
                    loop)));
    }

    private static Loop CreateTriangleLoop()
    {
        Vertex first =
            CreateVertex(
                0.0,
                0.0);

        Vertex second =
            CreateVertex(
                1.0,
                0.0);

        Vertex third =
            CreateVertex(
                0.0,
                1.0);

        var edgeUses =
            new[]
            {
                new EdgeUse(
                    CreateEdge(
                        first,
                        second),
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    CreateEdge(
                        second,
                        third),
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    CreateEdge(
                        third,
                        first),
                    EdgeUseDirection.Forward)
            };

        return new Loop(
            edgeUses);
    }

    private static Vertex CreateVertex(
        double x,
        double y)
    {
        return new Vertex(
            new Point3D(
                x,
                y,
                0.0));
    }

    private static Edge CreateEdge(
        Vertex startVertex,
        Vertex endVertex)
    {
        return new Edge(
            startVertex,
            endVertex,
            new TestCurve3D());
    }

    private sealed class TestCurve3D :
        ICurve3D
    {
    }

    private sealed class StubEdgeLoopMapper :
        IEntityMapper<
            StepEdgeLoopEntity,
            Loop>
    {
        private readonly MappingResult<Loop>
            _result;

        public StepEdgeLoopEntity? LastSource { get; private set; }

        public MappingContext? LastContext { get; private set; }

        public StubEdgeLoopMapper(
            MappingResult<Loop> result)
        {
            ArgumentNullException.ThrowIfNull(
                result);

            _result =
                result;
        }

        public MappingResult<Loop> Map(
            StepEdgeLoopEntity source,
            MappingContext context)
        {
            LastSource =
                source;

            LastContext =
                context;

            return _result;
        }
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly IReadOnlyDictionary<
            string,
            StepEdgeLoopEntity> _edgeLoops;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failedReferences;

        private readonly List<string>
            _resolutionOrder = new();

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepEdgeLoopEntity> edgeLoops,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic>? failedReferences = null)
        {
            _edgeLoops =
                edgeLoops
                ?? throw new ArgumentNullException(
                    nameof(edgeLoops));

            _failedReferences =
                failedReferences
                ?? new Dictionary<
                    string,
                    MappingDiagnostic>();
        }

        public IReadOnlyList<string> ResolutionOrder =>
            _resolutionOrder;

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (typeof(T) !=
                typeof(StepEdgeLoopEntity))
            {
                throw new NotSupportedException(
                    $"Resolution of '{typeof(T).Name}' is not supported by this test resolver.");
            }

            _resolutionOrder.Add(
                referenceId);

            if (_failedReferences.TryGetValue(
                    referenceId,
                    out MappingDiagnostic? diagnostic))
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    new[]
                    {
                        diagnostic
                    });
            }

            if (!_edgeLoops.TryGetValue(
                    referenceId,
                    out StepEdgeLoopEntity? edgeLoop))
            {
                throw new InvalidOperationException(
                    $"No EDGE_LOOP test entity was configured for '{referenceId}'.");
            }

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                Assert.IsAssignableFrom<T>(
                    edgeLoop));
        }
    }
}