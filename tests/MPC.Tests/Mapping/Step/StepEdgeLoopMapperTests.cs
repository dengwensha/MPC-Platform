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

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

        var source =
            CreateEdgeLoopEntity(
                "#100",
                "#11",
                "#12",
                "#13");

        var mapper =
            CreateMapper();

        MappingResult<Loop> result =
            mapper.Map(
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
            new[]
            {
                "#11",
                "#12",
                "#13"
            },
            resolver.OrientedEdgeResolutionOrder);

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

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

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
    }

    [Fact]
    public void Map_PreservesSourceReferenceOrder()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

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
            new[]
            {
                "#13",
                "#11",
                "#12"
            },
            resolver.OrientedEdgeResolutionOrder);

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

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges,
                failedOrientedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#12"] =
                            diagnostic
                    });

        var context =
            new MappingContext(
                resolver);

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
            new[]
            {
                "#11",
                "#12",
                "#13"
            },
            resolver.OrientedEdgeResolutionOrder);
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

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges,
                failedOrientedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#11"] =
                            firstDiagnostic,

                        ["#13"] =
                            secondDiagnostic
                    });

        var context =
            new MappingContext(
                resolver);

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

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges,
                failedEdgeReferences:
                    new Dictionary<string, MappingDiagnostic>
                    {
                        ["#22"] =
                            diagnostic
                    });

        var context =
            new MappingContext(
                resolver);

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

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithFewerThanThreeMappedEdgeUses_ReturnsInvalidLoopFailure()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

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

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

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
    }

    [Fact]
    public void Map_WithOpenEdgeChain_ReturnsInvalidLoopFailure()
    {
        TriangleFixture fixture =
            CreateOpenChainFixture();

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

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
    }

    [Fact]
    public void Map_WithExistingContextDiagnostic_ReturnsSuccessWithDiagnostics()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var resolver =
            new StubReferenceResolver(
                fixture.OrientedEdges,
                fixture.Edges);

        var context =
            new MappingContext(
                resolver);

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
            MappingStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.NotNull(
            result.Value);

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        TriangleFixture fixture =
            CreateTriangleFixture();

        var context =
            new MappingContext(
                new StubReferenceResolver(
                    fixture.OrientedEdges,
                    fixture.Edges));

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
            Edge> _edges;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failedOrientedEdgeReferences;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failedEdgeReferences;

        private readonly List<string>
            _orientedEdgeResolutionOrder = new();

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepOrientedEdgeEntity> orientedEdges,
            IReadOnlyDictionary<
                string,
                Edge> edges,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic>? failedOrientedEdgeReferences = null,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic>? failedEdgeReferences = null)
        {
            _orientedEdges =
                orientedEdges
                ?? throw new ArgumentNullException(
                    nameof(orientedEdges));

            _edges =
                edges
                ?? throw new ArgumentNullException(
                    nameof(edges));

            _failedOrientedEdgeReferences =
                failedOrientedEdgeReferences
                ?? new Dictionary<
                    string,
                    MappingDiagnostic>();

            _failedEdgeReferences =
                failedEdgeReferences
                ?? new Dictionary<
                    string,
                    MappingDiagnostic>();
        }

        public IReadOnlyList<string>
            OrientedEdgeResolutionOrder =>
                _orientedEdgeResolutionOrder;

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (typeof(T) ==
                typeof(StepOrientedEdgeEntity))
            {
                _orientedEdgeResolutionOrder.Add(
                    referenceId);

                if (_failedOrientedEdgeReferences.TryGetValue(
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

            if (typeof(T) ==
                typeof(Edge))
            {
                if (_failedEdgeReferences.TryGetValue(
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

                if (!_edges.TryGetValue(
                        referenceId,
                        out Edge? edge))
                {
                    throw new InvalidOperationException(
                        $"No canonical Edge test object was configured for '{referenceId}'.");
                }

                return ReferenceResolutionResult<T>.Resolved(
                    referenceId,
                    Assert.IsAssignableFrom<T>(
                        edge));
            }

            throw new NotSupportedException(
                $"Resolution of '{typeof(T).Name}' is not supported by this test resolver.");
        }
    }
}