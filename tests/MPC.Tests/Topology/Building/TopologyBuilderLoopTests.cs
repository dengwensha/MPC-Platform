using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderLoopTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    [Fact]
    public void Constructor_CreatesBuilderWithEmptyLoopCollection()
    {
        TopologyBuilder builder = new();

        Assert.Empty(builder.Loops);
    }

    [Fact]
    public void RegisterLoop_WithValidLoop_RegistersLoop()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        Assert.Single(builder.Loops);

        Assert.Same(
            loop,
            builder.Loops.Single());
    }

    [Fact]
    public void RegisterLoop_RegistersAllEdges()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        Assert.Equal(
            3,
            builder.Edges.Count);

        foreach (EdgeUse edgeUse in loop.EdgeUses)
        {
            Assert.Contains(
                builder.Edges,
                registeredEdge =>
                    ReferenceEquals(
                        registeredEdge,
                        edgeUse.Edge));
        }
    }

    [Fact]
    public void RegisterLoop_RegistersAllVertices()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        Assert.Equal(
            3,
            builder.Vertices.Count);

        foreach (EdgeUse edgeUse in loop.EdgeUses)
        {
            Assert.Contains(
                builder.Vertices,
                registeredVertex =>
                    ReferenceEquals(
                        registeredVertex,
                        edgeUse.StartVertex));

            Assert.Contains(
                builder.Vertices,
                registeredVertex =>
                    ReferenceEquals(
                        registeredVertex,
                        edgeUse.EndVertex));
        }
    }

    [Fact]
    public void RegisterLoop_DoesNotAttachEdgesBeforePublish()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        foreach (Vertex vertex in builder.Vertices)
        {
            Assert.Empty(
                vertex.IncidentEdges);
        }
    }

    [Fact]
    public void Publish_AttachesLoopEdgesToVertices()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);
        builder.Publish();

        foreach (EdgeUse edgeUse in loop.EdgeUses)
        {
            Edge edge = edgeUse.Edge;

            Assert.Contains(
                edge.StartVertex.IncidentEdges,
                incidentEdge =>
                    ReferenceEquals(
                        incidentEdge,
                        edge));

            Assert.Contains(
                edge.EndVertex.IncidentEdges,
                incidentEdge =>
                    ReferenceEquals(
                        incidentEdge,
                        edge));
        }
    }

    [Fact]
    public void RegisterLoop_WithSameLoopTwice_DoesNotDuplicateLoop()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);
        builder.RegisterLoop(loop);

        Assert.Single(builder.Loops);
        Assert.Equal(3, builder.Edges.Count);
        Assert.Equal(3, builder.Vertices.Count);
    }

    [Fact]
    public void RegisterLoop_WithNullLoop_ThrowsArgumentNullException()
    {
        TopologyBuilder builder = new();

        Assert.Throws<ArgumentNullException>(
            () => builder.RegisterLoop(null!));
    }

    [Fact]
    public void RegisterLoop_WithReversedEdgeUse_RegistersContinuousLoop()
    {
        TopologyBuilder builder = new();

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
                vertex3,
                vertex2);

        Edge edge3 =
            CreateEdge(
                vertex3,
                vertex1);

        Loop loop = new(
            new[]
            {
                new EdgeUse(
                    edge1,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    edge2,
                    EdgeUseDirection.Reversed),

                new EdgeUse(
                    edge3,
                    EdgeUseDirection.Forward)
            });

        builder.RegisterLoop(loop);

        Assert.Single(builder.Loops);
        Assert.Equal(3, builder.Edges.Count);

        Exception? exception =
            Record.Exception(builder.Verify);

        Assert.Null(exception);
    }

    [Fact]
    public void Verify_WithRegisteredLoop_DoesNotThrow()
    {
        TopologyBuilder builder = new();
        Loop loop = CreateTriangleLoop();

        builder.RegisterLoop(loop);

        Exception? exception =
            Record.Exception(builder.Verify);

        Assert.Null(exception);
    }

    [Fact]
    public void RegisterMultipleLoops_WithSharedEdge_DoesNotDuplicateSharedEdge()
    {
        TopologyBuilder builder = new();

        Vertex vertex1 =
            CreateVertex(0.0, 0.0, 0.0);

        Vertex vertex2 =
            CreateVertex(1.0, 0.0, 0.0);

        Vertex vertex3 =
            CreateVertex(0.0, 1.0, 0.0);

        Vertex vertex4 =
            CreateVertex(1.0, 1.0, 0.0);

        Edge sharedEdge =
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

        Loop firstLoop = new(
            new[]
            {
                new EdgeUse(sharedEdge),
                new EdgeUse(edge2),
                new EdgeUse(edge3)
            });

        Edge edge4 =
            CreateEdge(
                vertex1,
                vertex4);

        Edge edge5 =
            CreateEdge(
                vertex4,
                vertex2);

        Loop secondLoop = new(
            new[]
            {
                new EdgeUse(
                    sharedEdge,
                    EdgeUseDirection.Reversed),

                new EdgeUse(
                    edge4,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    edge5,
                    EdgeUseDirection.Forward)
            });

        builder.RegisterLoop(firstLoop);
        builder.RegisterLoop(secondLoop);

        Assert.Equal(2, builder.Loops.Count);
        Assert.Equal(5, builder.Edges.Count);
        Assert.Equal(4, builder.Vertices.Count);

        Assert.Single(
            builder.Edges.Where(
                edge =>
                    ReferenceEquals(
                        edge,
                        sharedEdge)));

        Exception? exception =
            Record.Exception(builder.Verify);

        Assert.Null(exception);
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