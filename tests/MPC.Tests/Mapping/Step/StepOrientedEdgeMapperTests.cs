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

        var resolver =
            new StubReferenceResolver(
                "#42",
                edge);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepOrientedEdgeEntity(
                sourceEntityId: "#100",
                name: "Forward edge use",
                edgeElementReferenceId: "#42",
                orientation: true);

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
            resolver.LastReferenceId);

        Assert.Equal(
            typeof(Edge),
            resolver.LastRequestedType);

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

        var resolver =
            new StubReferenceResolver(
                "#42",
                edge);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepOrientedEdgeEntity(
                sourceEntityId: "#100",
                name: "Reversed edge use",
                edgeElementReferenceId: "#42",
                orientation: false);

        var mapper =
            new StepOrientedEdgeMapper();

        MappingResult<EdgeUse> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

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
    }

    [Fact]
    public void Map_WithFailedReferenceResolution_ReturnsFailure()
    {
        var diagnostic =
            new MappingDiagnostic(
                "MAP-REF-EDGE-001",
                "Referenced canonical edge could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                "#42",
                diagnostic);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepOrientedEdgeEntity(
                sourceEntityId: "#100",
                name: "Unresolved edge use",
                edgeElementReferenceId: "#42",
                orientation: true);

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

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);

        Assert.True(
            context.HasErrors);

        Assert.Equal(
            "#42",
            resolver.LastReferenceId);

        Assert.Equal(
            typeof(Edge),
            resolver.LastRequestedType);
    }

    [Fact]
    public void Map_WithExistingContextDiagnostic_ReturnsSuccessWithDiagnostics()
    {
        Edge edge =
            CreateEdge();

        var resolver =
            new StubReferenceResolver(
                "#42",
                edge);

        var context =
            new MappingContext(
                resolver);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-INFO-001",
                "Mapping context contains prior information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            diagnostic);

        var source =
            new StepOrientedEdgeEntity(
                sourceEntityId: "#100",
                name: "Forward edge use",
                edgeElementReferenceId: "#42",
                orientation: true);

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

        MappingDiagnostic actualDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Same(
            diagnostic,
            actualDiagnostic);
    }

    [Fact]
    public void Map_DoesNotCreateAnotherCanonicalEdge()
    {
        Edge edge =
            CreateEdge();

        var resolver =
            new StubReferenceResolver(
                "#42",
                edge);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepOrientedEdgeEntity(
                sourceEntityId: "#100",
                name: "Canonical identity test",
                edgeElementReferenceId: "#42",
                orientation: true);

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
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        var context =
            new MappingContext(
                new StubReferenceResolver(
                    "#42",
                    CreateEdge()));

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
            new StepOrientedEdgeEntity(
                sourceEntityId: "#100",
                name: "Forward edge use",
                edgeElementReferenceId: "#42",
                orientation: true);

        var mapper =
            new StepOrientedEdgeMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
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
        private readonly string _expectedReferenceId;
        private readonly Edge? _edge;
        private readonly MappingDiagnostic? _failureDiagnostic;

        public StubReferenceResolver(
            string expectedReferenceId,
            Edge edge)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                expectedReferenceId);

            ArgumentNullException.ThrowIfNull(
                edge);

            _expectedReferenceId =
                expectedReferenceId;

            _edge =
                edge;
        }

        public StubReferenceResolver(
            string expectedReferenceId,
            MappingDiagnostic failureDiagnostic)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                expectedReferenceId);

            ArgumentNullException.ThrowIfNull(
                failureDiagnostic);

            _expectedReferenceId =
                expectedReferenceId;

            _failureDiagnostic =
                failureDiagnostic;
        }

        public string? LastReferenceId { get; private set; }

        public Type? LastRequestedType { get; private set; }

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            LastReferenceId =
                referenceId;

            LastRequestedType =
                typeof(T);

            Assert.Equal(
                _expectedReferenceId,
                referenceId);

            Assert.Equal(
                typeof(Edge),
                typeof(T));

            if (_failureDiagnostic is not null)
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    new[]
                    {
                        _failureDiagnostic
                    });
            }

            T resolvedValue =
                Assert.IsAssignableFrom<T>(
                    _edge);

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                resolvedValue);
        }
    }
}