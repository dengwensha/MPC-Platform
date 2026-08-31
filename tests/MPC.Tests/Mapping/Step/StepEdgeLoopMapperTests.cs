using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepEdgeLoopMapperTests
{
    [Fact]
    public void Constructor_WithNullOrientedEdgeMapper_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepEdgeLoopMapper(
                null!));
    }

    [Fact]
    public void Map_WithClosedTriangle_ReturnsCanonicalLoop()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out StubReferenceResolver referenceResolver,
                out _);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Loop loop =
            Assert.IsType<Loop>(
                result.Value);

        Assert.Equal(
            3,
            loop.EdgeUses.Count);

        Assert.Same(
            fixture.Edges["#21"],
            loop.EdgeUses[0].Edge);

        Assert.Same(
            fixture.Edges["#22"],
            loop.EdgeUses[1].Edge);

        Assert.Same(
            fixture.Edges["#23"],
            loop.EdgeUses[2].Edge);

        Assert.All(
            loop.EdgeUses,
            edgeUse =>
                Assert.Equal(
                    EdgeUseDirection.Forward,
                    edgeUse.Direction));

        Assert.Equal(
            [
                "#11",
                "#12",
                "#13"
            ],
            referenceResolver.OrientedEdgeResolutionOrder);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_PreservesOrientedEdgeDirections()
    {
        TriangleFixture fixture =
            CreateTriangleWithReversedSecondEdgeFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Loop loop =
            Assert.IsType<Loop>(
                result.Value);

        Assert.Equal(
            EdgeUseDirection.Forward,
            loop.EdgeUses[0].Direction);

        Assert.Equal(
            EdgeUseDirection.Reversed,
            loop.EdgeUses[1].Direction);

        Assert.Equal(
            EdgeUseDirection.Forward,
            loop.EdgeUses[2].Direction);

        Assert.Same(
            loop.EdgeUses[0].EndVertex,
            loop.EdgeUses[1].StartVertex);

        Assert.Same(
            loop.EdgeUses[1].EndVertex,
            loop.EdgeUses[2].StartVertex);

        Assert.Same(
            loop.EdgeUses[2].EndVertex,
            loop.EdgeUses[0].StartVertex);
    }

    [Fact]
    public void Map_PreservesSourceReferenceOrder()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out StubReferenceResolver referenceResolver,
                out _);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#13",
                "#11",
                "#12");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Loop loop =
            Assert.IsType<Loop>(
                result.Value);

        Assert.Equal(
            [
                "#13",
                "#11",
                "#12"
            ],
            referenceResolver.OrientedEdgeResolutionOrder);

        Assert.Same(
            fixture.Edges["#23"],
            loop.EdgeUses[0].Edge);

        Assert.Same(
            fixture.Edges["#21"],
            loop.EdgeUses[1].Edge);

        Assert.Same(
            fixture.Edges["#22"],
            loop.EdgeUses[2].Edge);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithUnresolvedOrientedEdge_ReturnsFailure()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var diagnostic =
            new MappingDiagnostic(
                "MAP-REF-ORIENTED-001",
                "Referenced ORIENTED_EDGE could not be resolved.",
                MappingDiagnosticSeverity.Error);

        MappingContext context =
            CreateContext(
                fixture,
                out StubReferenceResolver referenceResolver,
                out _,
                failedOrientedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#12"] =
                            diagnostic
                    });

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
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

        Assert.Equal(
            [
                "#11",
                "#12",
                "#13"
            ],
            referenceResolver.OrientedEdgeResolutionOrder);
    }

    [Fact]
    public void Map_WithMultipleUnresolvedReferences_AggregatesDiagnosticsInSourceOrder()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var firstDiagnostic =
            new MappingDiagnostic(
                "MAP-REF-ORIENTED-001",
                "First ORIENTED_EDGE could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var secondDiagnostic =
            new MappingDiagnostic(
                "MAP-REF-ORIENTED-002",
                "Second ORIENTED_EDGE could not be resolved.",
                MappingDiagnosticSeverity.Error);

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _,
                failedOrientedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#11"] =
                            firstDiagnostic,

                        ["#13"] =
                            secondDiagnostic
                    });

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Equal(
            2,
            result.Diagnostics.Count);

        Assert.Same(
            firstDiagnostic,
            result.Diagnostics[0]);

        Assert.Same(
            secondDiagnostic,
            result.Diagnostics[1]);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            firstDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            secondDiagnostic,
            context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WithOrientedEdgeWhoseCanonicalEdgeCannotResolve_ReturnsFailure()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var diagnostic =
            new MappingDiagnostic(
                "MAP-REF-EDGE-001",
                "Referenced canonical edge could not be resolved.",
                MappingDiagnosticSeverity.Error);

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out StubCanonicalReferenceResolver canonicalResolver,
                failedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#22"] =
                            diagnostic
                    });

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
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

        Assert.Equal(
            [
                "#21",
                "#22",
                "#23"
            ],
            canonicalResolver.EdgeResolutionOrder);
    }

    [Fact]
    public void Map_WithOpenTwoEdgeChain_ReturnsInvalidLoopFailure()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-EDGE-LOOP-001",
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Contains(
            "closed topological cycle",
            diagnostic.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithDiscontinuousEdgeUses_ReturnsInvalidLoopFailure()
    {
        TriangleFixture fixture =
            CreateDiscontinuousFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-EDGE-LOOP-001",
            diagnostic.Code);

        Assert.Contains(
            "continuity",
            diagnostic.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithOpenEdgeChain_ReturnsInvalidLoopFailure()
    {
        TriangleFixture fixture =
            CreateOpenChainFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-EDGE-LOOP-001",
            diagnostic.Code);

        Assert.Contains(
            "closed",
            diagnostic.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingInformation_DoesNotIncludeItInSuccessfulResult()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-INFO-001",
                "Existing mapping information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            diagnostic);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
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

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WithPreExistingError_DoesNotFailSuccessfulOperation()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-ERROR-001",
                "A previous mapping operation failed.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            diagnostic);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
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

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WithUnresolvedOrientedEdgeAndPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var currentDiagnostic =
            new MappingDiagnostic(
                "MAP-REF-ORIENTED-001",
                "Referenced ORIENTED_EDGE could not be resolved.",
                MappingDiagnosticSeverity.Error);

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _,
                failedOrientedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#12"] =
                            currentDiagnostic
                    });

        var priorDiagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-INFO-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            priorDiagnostic);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        MappingResult<Loop> result =
            CreateMapper().Map(
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
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        MappingContext context =
            CreateContext(
                fixture,
                out _,
                out _);

        Assert.Throws<ArgumentNullException>(
            () => CreateMapper().Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        Assert.Throws<ArgumentNullException>(
            () => CreateMapper().Map(
                source,
                null!));
    }

    private static StepEdgeLoopMapper CreateMapper()
    {
        return new StepEdgeLoopMapper(
            new StepOrientedEdgeMapper());
    }

    private static StepEdgeLoopEntity CreateEdgeLoopEntity(
        string sourceEntityId,
        params string[] referenceIds)
    {
        return new StepEdgeLoopEntity(
            sourceEntityId,
            "Triangle loop",
            referenceIds);
    }

    private static MappingContext CreateContext(
        TriangleFixture fixture,
        out StubReferenceResolver referenceResolver,
        out StubCanonicalReferenceResolver canonicalResolver,
        IReadOnlyDictionary<
            string,
            MappingDiagnostic>? failedOrientedEdgeReferences = null,
        IReadOnlyDictionary<
            string,
            MappingDiagnostic>? failedEdgeReferences = null)
    {
        referenceResolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                failedOrientedEdgeReferences);

        canonicalResolver =
            new StubCanonicalReferenceResolver(
                fixture.Edges,
                failedEdgeReferences);

        return new MappingContext(
            referenceResolver,
            canonicalResolver);
    }

    private static TriangleFixture CreateTriangleFixture()
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

        var edges =
            new Dictionary<string, Edge>
            {
                ["#21"] =
                    CreateEdge(
                        first,
                        second),

                ["#22"] =
                    CreateEdge(
                        second,
                        third),

                ["#23"] =
                    CreateEdge(
                        third,
                        first)
            };

        var orientedEdges =
            new Dictionary<string, StepOrientedEdgeEntity>
            {
                ["#11"] =
                    CreateOrientedEdge(
                        "#11",
                        "#21",
                        true),

                ["#12"] =
                    CreateOrientedEdge(
                        "#12",
                        "#22",
                        true),

                ["#13"] =
                    CreateOrientedEdge(
                        "#13",
                        "#23",
                        true)
            };

        return new TriangleFixture(
            orientedEdges,
            edges);
    }

    private static TriangleFixture
        CreateTriangleWithReversedSecondEdgeFixture()
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

        var edges =
            new Dictionary<string, Edge>
            {
                ["#21"] =
                    CreateEdge(
                        first,
                        second),

                ["#22"] =
                    CreateEdge(
                        third,
                        second),

                ["#23"] =
                    CreateEdge(
                        third,
                        first)
            };

        var orientedEdges =
            new Dictionary<string, StepOrientedEdgeEntity>
            {
                ["#11"] =
                    CreateOrientedEdge(
                        "#11",
                        "#21",
                        true),

                ["#12"] =
                    CreateOrientedEdge(
                        "#12",
                        "#22",
                        false),

                ["#13"] =
                    CreateOrientedEdge(
                        "#13",
                        "#23",
                        true)
            };

        return new TriangleFixture(
            orientedEdges,
            edges);
    }

    private static TriangleFixture CreateDiscontinuousFixture()
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

        Vertex disconnected =
            CreateVertex(
                2.0,
                2.0);

        var edges =
            new Dictionary<string, Edge>
            {
                ["#21"] =
                    CreateEdge(
                        first,
                        second),

                ["#22"] =
                    CreateEdge(
                        disconnected,
                        third),

                ["#23"] =
                    CreateEdge(
                        third,
                        first)
            };

        return CreateForwardFixture(
            edges);
    }

    private static TriangleFixture CreateOpenChainFixture()
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

        Vertex fourth =
            CreateVertex(
                -1.0,
                0.0);

        var edges =
            new Dictionary<string, Edge>
            {
                ["#21"] =
                    CreateEdge(
                        first,
                        second),

                ["#22"] =
                    CreateEdge(
                        second,
                        third),

                ["#23"] =
                    CreateEdge(
                        third,
                        fourth)
            };

        return CreateForwardFixture(
            edges);
    }

    private static TriangleFixture CreateForwardFixture(
        IReadOnlyDictionary<string, Edge> edges)
    {
        var orientedEdges =
            new Dictionary<string, StepOrientedEdgeEntity>
            {
                ["#11"] =
                    CreateOrientedEdge(
                        "#11",
                        "#21",
                        true),

                ["#12"] =
                    CreateOrientedEdge(
                        "#12",
                        "#22",
                        true),

                ["#13"] =
                    CreateOrientedEdge(
                        "#13",
                        "#23",
                        true)
            };

        return new TriangleFixture(
            orientedEdges,
            edges);
    }

    private static StepOrientedEdgeEntity CreateOrientedEdge(
        string sourceEntityId,
        string edgeReferenceId,
        bool orientation)
    {
        return new StepOrientedEdgeEntity(
            sourceEntityId,
            "Oriented edge",
            edgeReferenceId,
            orientation);
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

    private sealed record TriangleFixture(
        IReadOnlyDictionary<
            string,
            StepOrientedEdgeEntity> OrientedEdges,
        IReadOnlyDictionary<
            string,
            Edge> Edges);

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly IReadOnlyDictionary<
            string,
            StepOrientedEdgeEntity> _orientedEdges;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failures;

        private readonly List<string>
            _resolutionOrder = [];

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepOrientedEdgeEntity> orientedEdges,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic>? failures = null)
        {
            _orientedEdges =
                orientedEdges
                ?? throw new ArgumentNullException(
                    nameof(orientedEdges));

            _failures =
                failures
                ?? new Dictionary<
                    string,
                    MappingDiagnostic>();
        }

        public IReadOnlyList<string>
            OrientedEdgeResolutionOrder =>
                _resolutionOrder;

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (typeof(T) !=
                typeof(StepOrientedEdgeEntity))
            {
                throw new InvalidOperationException(
                    $"Semantic resolver must not resolve canonical type '{typeof(T).Name}'.");
            }

            _resolutionOrder.Add(
                referenceId);

            if (_failures.TryGetValue(
                    referenceId,
                    out MappingDiagnostic? diagnostic))
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    [
                        diagnostic
                    ]);
            }

            if (!_orientedEdges.TryGetValue(
                    referenceId,
                    out StepOrientedEdgeEntity? entity))
            {
                throw new InvalidOperationException(
                    $"No ORIENTED_EDGE test entity was configured for '{referenceId}'.");
            }

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                Assert.IsAssignableFrom<T>(
                    entity));
        }
    }

    private sealed class StubCanonicalReferenceResolver :
        ICanonicalReferenceResolver
    {
        private readonly IReadOnlyDictionary<
            string,
            Edge> _edges;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failures;

        private readonly List<string>
            _edgeResolutionOrder = [];

        public StubCanonicalReferenceResolver(
            IReadOnlyDictionary<
                string,
                Edge> edges,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic>? failures = null)
        {
            _edges =
                edges
                ?? throw new ArgumentNullException(
                    nameof(edges));

            _failures =
                failures
                ?? new Dictionary<
                    string,
                    MappingDiagnostic>();
        }

        public IReadOnlyList<string> EdgeResolutionOrder =>
            _edgeResolutionOrder;

        public MappingResult<T> Resolve<T>(
            string referenceId,
            MappingContext context)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(
                context);

            if (typeof(T) !=
                typeof(Edge))
            {
                throw new InvalidOperationException(
                    $"Canonical resolver does not support '{typeof(T).Name}' in this test.");
            }

            _edgeResolutionOrder.Add(
                referenceId);

            if (_failures.TryGetValue(
                    referenceId,
                    out MappingDiagnostic? diagnostic))
            {
                context.AddDiagnostic(
                    diagnostic);

                return MappingResult<T>.Failure(
                    [
                        diagnostic
                    ]);
            }

            if (!_edges.TryGetValue(
                    referenceId,
                    out Edge? edge))
            {
                throw new InvalidOperationException(
                    $"No canonical Edge test object was configured for '{referenceId}'.");
            }

            return MappingResult<T>.Success(
                Assert.IsAssignableFrom<T>(
                    edge));
        }
    }
}