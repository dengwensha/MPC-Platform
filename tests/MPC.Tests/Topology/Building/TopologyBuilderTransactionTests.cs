using System.Collections;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderTransactionTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    [Fact]
    public void Publish_WithValidGraph_ReturnsPublishedTopology()
    {
        TopologyBuilder builder = new();

        Vertex startVertex =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex endVertex =
            CreateVertex(1.0, 0.0, 0.0);

        Edge edge =
            CreateEdge(
                startVertex,
                endVertex);

        builder.RegisterEdge(edge);

        PublishedTopology result =
            builder.Publish();

        Assert.Equal(
            TopologyBuilderState.Published,
            builder.State);

        Assert.Equal(
            2,
            result.Vertices.Count);

        Assert.Single(
            result.Edges);

        Assert.Empty(
            result.Loops);

        Assert.Contains(
            result.Vertices,
            vertex =>
                ReferenceEquals(
                    vertex,
                    startVertex));

        Assert.Contains(
            result.Vertices,
            vertex =>
                ReferenceEquals(
                    vertex,
                    endVertex));

        Assert.Same(
            edge,
            result.Edges[0]);
    }

    [Fact]
    public void Publish_WithLoop_ReturnsCompletePublishedGraph()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        PublishedTopology result =
            builder.Publish();

        Assert.Equal(
            3,
            result.Vertices.Count);

        Assert.Equal(
            3,
            result.Edges.Count);

        Assert.Single(
            result.Loops);

        Assert.Same(
            loop,
            result.Loops[0]);
    }

    [Fact]
    public void Publish_SetsBuilderStateToPublished()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Assert.Equal(
            TopologyBuilderState.Published,
            builder.State);
    }

    [Fact]
    public void Publish_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Assert.Throws<InvalidOperationException>(
            builder.Publish);
    }

    [Fact]
    public void RegisterVertex_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterVertex(
                CreateVertex(
                    0.0,
                    0.0,
                    0.0)));
    }

    [Fact]
    public void RegisterEdge_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Vertex startVertex =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex endVertex =
            CreateVertex(1.0, 0.0, 0.0);

        Edge edge =
            CreateEdge(
                startVertex,
                endVertex);

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterEdge(edge));
    }

    [Fact]
    public void RegisterLoop_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Loop loop =
            CreateTriangleLoop();

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterLoop(loop));
    }

    [Fact]
    public void Verify_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Assert.Throws<InvalidOperationException>(
            builder.Verify);
    }

    [Fact]
    public void Rollback_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Publish();

        Assert.Throws<InvalidOperationException>(
            builder.Rollback);
    }

    [Fact]
    public void Rollback_ClearsStagedGraph()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        builder.Rollback();

        Assert.Empty(builder.Vertices);
        Assert.Empty(builder.Edges);
        Assert.Empty(builder.Loops);

        Assert.Equal(
            TopologyBuilderState.RolledBack,
            builder.State);
    }

    [Fact]
    public void Rollback_DoesNotMutateVertices()
    {
        TopologyBuilder builder = new();

        Vertex startVertex =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex endVertex =
            CreateVertex(1.0, 0.0, 0.0);

        Edge edge =
            CreateEdge(
                startVertex,
                endVertex);

        builder.RegisterEdge(edge);
        builder.Rollback();

        Assert.Empty(
            startVertex.IncidentEdges);

        Assert.Empty(
            endVertex.IncidentEdges);
    }

    [Fact]
    public void RegisterVertex_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Rollback();

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterVertex(
                CreateVertex(
                    0.0,
                    0.0,
                    0.0)));
    }

    [Fact]
    public void RegisterEdge_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Rollback();

        Vertex startVertex =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex endVertex =
            CreateVertex(1.0, 0.0, 0.0);

        Edge edge =
            CreateEdge(
                startVertex,
                endVertex);

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterEdge(edge));
    }

    [Fact]
    public void RegisterLoop_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Rollback();

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterLoop(
                CreateTriangleLoop()));
    }

    [Fact]
    public void Verify_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Rollback();

        Assert.Throws<InvalidOperationException>(
            builder.Verify);
    }

    [Fact]
    public void Publish_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Rollback();

        Assert.Throws<InvalidOperationException>(
            builder.Publish);
    }

    [Fact]
    public void Rollback_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder = new();

        builder.Rollback();

        Assert.Throws<InvalidOperationException>(
            builder.Rollback);
    }

    [Fact]
    public void PublishedTopology_CollectionsAreNotMutableLists()
    {
        TopologyBuilder builder = new();

        builder.RegisterVertex(
            CreateVertex(
                0.0,
                0.0,
                0.0));

        PublishedTopology result =
            builder.Publish();

        Assert.False(
            result.Vertices is List<Vertex>);

        Assert.False(
            result.Edges is List<Edge>);

        Assert.False(
            result.Loops is List<Loop>);
    }

    [Fact]
    public void PublishedTopology_VertexCollectionRejectsMutation()
    {
        TopologyBuilder builder = new();

        builder.RegisterVertex(
            CreateVertex(
                0.0,
                0.0,
                0.0));

        PublishedTopology result =
            builder.Publish();

        IList vertices =
            Assert.IsAssignableFrom<IList>(
                result.Vertices);

        Assert.True(vertices.IsReadOnly);

        Assert.Throws<NotSupportedException>(
            () => vertices.Add(
                CreateVertex(
                    1.0,
                    0.0,
                    0.0)));
    }

    [Fact]
    public void PublishedTopology_PreservesPublishTimeCollection()
    {
        TopologyBuilder builder = new();

        Vertex vertex =
            CreateVertex(
                0.0,
                0.0,
                0.0);

        builder.RegisterVertex(vertex);

        PublishedTopology result =
            builder.Publish();

        Assert.Single(
            result.Vertices);

        Assert.Same(
            vertex,
            result.Vertices[0]);
    }

    private static Loop CreateTriangleLoop()
    {
        Vertex vertex1 =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex vertex2 =
            CreateVertex(1.0, 0.0, 0.0);

        Vertex vertex3 =
            CreateVertex(0.0, 1.0, 0.0);

        Edge edge1 =
            CreateEdge(
                vertex1,
                vertex2);

        Edge edge2 =
            CreateEdge(
                vertex2,
                vertex3);

        Edge edge3 =
            CreateEdge(
                vertex3,
                vertex1);

        return new Loop(
            new[]
            {
                new EdgeUse(edge1),
                new EdgeUse(edge2),
                new EdgeUse(edge3)
            });
    }

    private static Vertex CreateVertex(
        double x,
        double y,
        double z)
    {
        return new Vertex(
            new Point3D(
                x,
                y,
                z));
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
}