using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepOrientedEdgeMapperTests
{
    [Fact]
    public void Map_WithForwardOrientation_ReturnsForwardEdgeUse()
    {
        Edge edge =
            CreateEdge();

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    edge));

        var context =
            CreateContext(
                canonicalResolver);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        EdgeUse edgeUse =
            Assert.IsType<EdgeUse>(
                result.Value);

        Assert.Same(
            edge,
            edgeUse.Edge);

        Assert.Equal(
            EdgeUseDirection.Forward,
            edgeUse.Direction);

        Assert.Same(
            edge.StartVertex,
            edgeUse.StartVertex);

        Assert.Same(
            edge.EndVertex,
            edgeUse.EndVertex);

        Assert.Equal(
            "#42",
            canonicalResolver.LastReferenceId);

        Assert.Equal(
            typeof(Edge),
            canonicalResolver.LastRequestedType);

        Assert.Same(
            context,
            canonicalResolver.LastContext);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithReversedOrientation_ReturnsReversedEdgeUse()
    {
        Edge edge =
            CreateEdge();

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    edge));

        var context =
            CreateContext(
                canonicalResolver);

        var source =
            CreateSource(
                orientation:
                    false);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        EdgeUse edgeUse =
            Assert.IsType<EdgeUse>(
                result.Value);

        Assert.Same(
            edge,
            edgeUse.Edge);

        Assert.Equal(
            EdgeUseDirection.Reversed,
            edgeUse.Direction);

        Assert.Same(
            edge.EndVertex,
            edgeUse.StartVertex);

        Assert.Same(
            edge.StartVertex,
            edgeUse.EndVertex);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithFailedCanonicalEdgeResolution_ReturnsFailure()
    {
        var diagnostic =
            new MappingDiagnostic(
                "MAP-REF-EDGE-001",
                "Referenced canonical edge could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Failure(
                    [
                        diagnostic
                    ]));

        var context =
            CreateContext(
                canonicalResolver);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
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
            diagnostic,
            resultDiagnostic);

        Assert.Equal(
            "#42",
            canonicalResolver.LastReferenceId);

        Assert.Equal(
            typeof(Edge),
            canonicalResolver.LastRequestedType);

        Assert.Same(
            context,
            canonicalResolver.LastContext);
    }

    [Fact]
    public void Map_WithPreExistingInformation_DoesNotIncludeItInSuccessfulResult()
    {
        Edge edge =
            CreateEdge();

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    edge));

        var context =
            CreateContext(
                canonicalResolver);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-INFO-001",
                "Mapping context contains prior information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            diagnostic);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        EdgeUse edgeUse =
            Assert.IsType<EdgeUse>(
                result.Value);

        Assert.Same(
            edge,
            edgeUse.Edge);

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
        Edge edge =
            CreateEdge();

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    edge));

        var context =
            CreateContext(
                canonicalResolver);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-ERROR-PRIOR-001",
                "A previous mapping operation failed.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            diagnostic);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
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

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WhenCanonicalResolverAddsCurrentDiagnostic_ReturnsSuccessWithOnlyCurrentDiagnostic()
    {
        Edge edge =
            CreateEdge();

        var currentDiagnostic =
            new MappingDiagnostic(
                "MAP-REF-EDGE-INFO-001",
                "Canonical edge resolution produced current-operation information.",
                MappingDiagnosticSeverity.Information);

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    edge),
                currentDiagnostic);

        var context =
            CreateContext(
                canonicalResolver);

        var priorDiagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-INFO-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            priorDiagnostic);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
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
    public void Map_DoesNotCreateAnotherCanonicalEdge()
    {
        Edge edge =
            CreateEdge();

        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    edge));

        var context =
            CreateContext(
                canonicalResolver);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
            mapper.Map(
                source,
                context);

        EdgeUse edgeUse =
            Assert.IsType<EdgeUse>(
                result.Value);

        Assert.Same(
            edge,
            edgeUse.Edge);

        Assert.Same(
            edge.StartVertex,
            edgeUse.Edge.StartVertex);

        Assert.Same(
            edge.EndVertex,
            edgeUse.Edge.EndVertex);

        Assert.Same(
            edge.Curve,
            edgeUse.Edge.Curve);
    }

    [Fact]
    public void Map_WithoutCanonicalReferenceResolver_ReturnsFailure()
    {
        var context =
            new MappingContext(
                new StubReferenceResolver());

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
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

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-ORIENTED-EDGE-001",
            diagnostic.Code);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithoutCanonicalReferenceResolverAndWithPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        var context =
            new MappingContext(
                new StubReferenceResolver());

        var priorDiagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-INFO-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            priorDiagnostic);

        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
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

        Assert.Equal(
            "MAP-STEP-ORIENTED-EDGE-001",
            resultDiagnostic.Code);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            priorDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            resultDiagnostic,
            context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        var canonicalResolver =
            new StubCanonicalReferenceResolver(
                "#42",
                MappingResult<Edge>.Success(
                    CreateEdge()));

        var context =
            CreateContext(
                canonicalResolver);

        var mapper =
            new StepOrientedEdgeMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var source =
            CreateSource(
                orientation:
                    true);

        var mapper =
            new StepOrientedEdgeMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static StepOrientedEdgeEntity CreateSource(
        bool orientation)
    {
        return new StepOrientedEdgeEntity(
            sourceEntityId:
                "#100",
            name:
                "Canonical edge use",
            edgeElementReferenceId:
                "#42",
            orientation:
                orientation);
    }

    private static MappingContext CreateContext(
        ICanonicalReferenceResolver canonicalResolver)
    {
        return new MappingContext(
            new StubReferenceResolver(),
            canonicalResolver);
    }

    private static Edge CreateEdge()
    {
        var startVertex =
            new Vertex(
                new Point3D(
                    0.0,
                    0.0,
                    0.0));

        var endVertex =
            new Vertex(
                new Point3D(
                    1.0,
                    0.0,
                    0.0));

        return new Edge(
            startVertex,
            endVertex,
            new TestCurve3D());
    }

    private sealed class TestCurve3D :
        ICurve3D
    {
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            throw new InvalidOperationException(
                "Semantic reference resolution must not be used by StepOrientedEdgeMapper.");
        }
    }

    private sealed class StubCanonicalReferenceResolver :
        ICanonicalReferenceResolver
    {
        private readonly string _expectedReferenceId;
        private readonly MappingResult<Edge> _result;
        private readonly MappingDiagnostic? _diagnosticToAddToContext;

        public StubCanonicalReferenceResolver(
            string expectedReferenceId,
            MappingResult<Edge> result,
            MappingDiagnostic? diagnosticToAddToContext = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                expectedReferenceId);

            ArgumentNullException.ThrowIfNull(
                result);

            _expectedReferenceId =
                expectedReferenceId;

            _result =
                result;

            _diagnosticToAddToContext =
                diagnosticToAddToContext;
        }

        public string? LastReferenceId { get; private set; }

        public Type? LastRequestedType { get; private set; }

        public MappingContext? LastContext { get; private set; }

        public MappingResult<T> Resolve<T>(
            string referenceId,
            MappingContext context)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(
                context);

            LastReferenceId =
                referenceId;

            LastRequestedType =
                typeof(T);

            LastContext =
                context;

            Assert.Equal(
                _expectedReferenceId,
                referenceId);

            Assert.Equal(
                typeof(Edge),
                typeof(T));

            if (_diagnosticToAddToContext is not null)
            {
                context.AddDiagnostic(
                    _diagnosticToAddToContext);
            }

            if (!_result.IsSuccessful ||
                _result.Value is null)
            {
                return MappingResult<T>.Failure(
                    _result.Diagnostics);
            }

            T resolvedValue =
                Assert.IsAssignableFrom<T>(
                    _result.Value);

            if (_result.Status ==
                MappingStatus.SucceededWithDiagnostics)
            {
                return MappingResult<T>.SuccessWithDiagnostics(
                    resolvedValue,
                    _result.Diagnostics);
            }

            return MappingResult<T>.Success(
                resolvedValue);
        }
    }
}