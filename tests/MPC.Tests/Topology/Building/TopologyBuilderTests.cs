using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    [Fact]
    public void Constructor_CreatesEmptyOpenBuilder()
    {
        TopologyBuilder builder = new();

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);

        Assert.Empty(builder.Vertices);
        Assert.Empty(builder.Edges);
        Assert.Empty(builder.Loops);
    }

    [Fact]
    public void RegisterVertex_AddsVertex()
    {
        TopologyBuilder builder = new();
        Vertex vertex = CreateVertex(0.0, 0.0, 0.0);

        builder.RegisterVertex(vertex);

        Assert.Single(builder.Vertices);

        Assert.Same(
            vertex,
            builder.Vertices.Single());
    }

    [Fact]
    public void RegisterVertex_WithSameVertexTwice_DoesNotDuplicateVertex()
    {
        TopologyBuilder builder = new();
        Vertex vertex = CreateVertex(0.0, 0.0, 0.0);

        builder.RegisterVertex(vertex);
        builder.RegisterVertex(vertex);

        Assert.Single(builder.Vertices);
    }

    [Fact]
    public void RegisterVertex_WithDifferentVerticesAtSamePosition_RegistersBothVertices()
    {
        TopologyBuilder builder = new();

        Vertex vertex1 =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex vertex2 =
            CreateVertex(0.0, 0.0, 0.0);

        builder.RegisterVertex(vertex1);
        builder.RegisterVertex(vertex2);

        Assert.Equal(
            2,
            builder.Vertices.Count);
    }

    [Fact]
    public void RegisterVertex_WithNullVertex_ThrowsArgumentNullException()
    {
        TopologyBuilder builder = new();

        Assert.Throws<ArgumentNullException>(
            () => builder.RegisterVertex(null!));
    }

    [Fact]
    public void RegisterEdge_RegistersEdgeAndVertices()
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

        Assert.Single(builder.Edges);
        Assert.Equal(2, builder.Vertices.Count);

        Assert.Contains(
            startVertex,
            builder.Vertices);

        Assert.Contains(
            endVertex,
            builder.Vertices);

        Assert.Same(
            edge,
            builder.Edges.Single());
    }

    [Fact]
    public void RegisterEdge_DoesNotAttachEdgeBeforePublish()
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

        Assert.Empty(
            startVertex.IncidentEdges);

        Assert.Empty(
            endVertex.IncidentEdges);
    }

    [Fact]
    public void Publish_AttachesEdgeToBothVertices()
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
        builder.Publish();

        Assert.Single(
            startVertex.IncidentEdges);

        Assert.Single(
            endVertex.IncidentEdges);

        Assert.Same(
            edge,
            startVertex.IncidentEdges[0]);

        Assert.Same(
            edge,
            endVertex.IncidentEdges[0]);
    }

    [Fact]
    public void RegisterEdge_WithSameEdgeTwice_DoesNotDuplicateStagedGraph()
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
        builder.RegisterEdge(edge);

        Assert.Single(builder.Edges);
        Assert.Equal(2, builder.Vertices.Count);

        Assert.Empty(
            startVertex.IncidentEdges);

        Assert.Empty(
            endVertex.IncidentEdges);
    }

    [Fact]
    public void Publish_WithSameEdgeRegisteredTwice_AttachesOnlyOnce()
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
        builder.RegisterEdge(edge);

        builder.Publish();

        Assert.Single(
            startVertex.IncidentEdges);

        Assert.Single(
            endVertex.IncidentEdges);
    }

    [Fact]
    public void Publish_WithSelfLoop_AttachesEdgeOnlyOnce()
    {
        TopologyBuilder builder = new();

        Vertex vertex =
            CreateVertex(0.0, 0.0, 0.0);

        Edge edge =
            CreateEdge(
                vertex,
                vertex);

        builder.RegisterEdge(edge);
        builder.Publish();

        Assert.Single(builder.Vertices);
        Assert.Single(builder.Edges);
        Assert.Single(vertex.IncidentEdges);

        Assert.Same(
            edge,
            vertex.IncidentEdges[0]);
    }

    [Fact]
    public void RegisterEdge_WithNullEdge_ThrowsArgumentNullException()
    {
        TopologyBuilder builder = new();

        Assert.Throws<ArgumentNullException>(
            () => builder.RegisterEdge(null!));
    }

    [Fact]
    public void Verify_WithValidStagedGraph_DoesNotThrow()
    {
        TopologyBuilder builder = new();

        Vertex vertex1 =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex vertex2 =
            CreateVertex(1.0, 0.0, 0.0);

        Vertex vertex3 =
            CreateVertex(1.0, 1.0, 0.0);

        builder.RegisterEdge(
            CreateEdge(
                vertex1,
                vertex2));

        builder.RegisterEdge(
            CreateEdge(
                vertex2,
                vertex3));

        builder.RegisterEdge(
            CreateEdge(
                vertex3,
                vertex1));

        Exception? exception =
            Record.Exception(builder.Verify);

        Assert.Null(exception);

        Assert.Empty(vertex1.IncidentEdges);
        Assert.Empty(vertex2.IncidentEdges);
        Assert.Empty(vertex3.IncidentEdges);
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