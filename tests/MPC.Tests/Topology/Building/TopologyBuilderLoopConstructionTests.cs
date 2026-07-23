using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderLoopConstructionTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    [Fact]
    public void BuildLoop_WithValidTriangle_ReturnsLoop()
    {
        TopologyBuilder builder =
            new();

        EdgeUse[] edgeUses =
            CreateTriangleEdgeUses();

        Loop loop =
            builder.BuildLoop(edgeUses);

        Assert.NotNull(loop);

        Assert.Equal(
            3,
            loop.EdgeUses.Count);
    }

    [Fact]
    public void BuildLoop_WithValidTriangle_RegistersLoop()
    {
        TopologyBuilder builder =
            new();

        EdgeUse[] edgeUses =
            CreateTriangleEdgeUses();

        Loop loop =
            builder.BuildLoop(edgeUses);

        Assert.Single(
            builder.Loops);

        Assert.Same(
            loop,
            builder.Loops.Single());
    }

    [Fact]
    public void BuildLoop_WithValidTriangle_RegistersEdges()
    {
        TopologyBuilder builder =
            new();

        EdgeUse[] edgeUses =
            CreateTriangleEdgeUses();

        builder.BuildLoop(
            edgeUses);

        Assert.Equal(
            3,
            builder.Edges.Count);

        foreach (EdgeUse edgeUse in edgeUses)
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
    public void BuildLoop_WithValidTriangle_RegistersVertices()
    {
        TopologyBuilder builder =
            new();

        EdgeUse[] edgeUses =
            CreateTriangleEdgeUses();

        builder.BuildLoop(
            edgeUses);

        Assert.Equal(
            3,
            builder.Vertices.Count);

        foreach (EdgeUse edgeUse in edgeUses)
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
    public void BuildLoop_WithValidTriangle_PreservesEdgeUses()
    {
        TopologyBuilder builder =
            new();

        EdgeUse[] edgeUses =
            CreateTriangleEdgeUses();

        Loop loop =
            builder.BuildLoop(edgeUses);

        Assert.Equal(
            edgeUses.Length,
            loop.EdgeUses.Count);

        for (int index = 0;
             index < edgeUses.Length;
             index++)
        {
            Assert.Same(
                edgeUses[index],
                loop.EdgeUses[index]);
        }
    }

    [Fact]
    public void BuildLoop_WithNullEdgeUses_ThrowsArgumentNullException()
    {
        TopologyBuilder builder =
            new();

        Exception? exception =
            Record.Exception(
                () =>
                    builder.BuildLoop(
                        null!));

        Assert.IsType<ArgumentNullException>(
            exception);
    }

    [Fact]
    public void BuildLoop_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder =
            new();

        builder.BuildLoop(
            CreateTriangleEdgeUses());

        builder.Publish();

        Exception? exception =
            Record.Exception(
                () =>
                    builder.BuildLoop(
                        CreateTriangleEdgeUses()));

        Assert.IsType<InvalidOperationException>(
            exception);
    }

    [Fact]
    public void BuildLoop_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder =
            new();

        builder.Rollback();

        Exception? exception =
            Record.Exception(
                () =>
                    builder.BuildLoop(
                        CreateTriangleEdgeUses()));

        Assert.IsType<InvalidOperationException>(
            exception);
    }

    [Fact]
    public void BuildLoop_WithReversedEdgeUse_RegistersContinuousLoop()
    {
        TopologyBuilder builder =
            new();

        Vertex vertex1 =
            CreateVertex(
                0.0,
                0.0,
                0.0);

        Vertex vertex2 =
            CreateVertex(
                1.0,
                0.0,
                0.0);

        Vertex vertex3 =
            CreateVertex(
                0.0,
                1.0,
                0.0);

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

        EdgeUse[] edgeUses =
        [
            new EdgeUse(
                edge1,
                EdgeUseDirection.Forward),

            new EdgeUse(
                edge2,
                EdgeUseDirection.Reversed),

            new EdgeUse(
                edge3,
                EdgeUseDirection.Forward)
        ];

        Loop loop =
            builder.BuildLoop(edgeUses);

        Assert.Single(
            builder.Loops);

        Assert.Same(
            loop,
            builder.Loops.Single());

        Assert.Equal(
            3,
            builder.Edges.Count);

        Assert.Equal(
            3,
            builder.Vertices.Count);

        Exception? exception =
            Record.Exception(
                builder.Verify);

        Assert.Null(
            exception);
    }

    private static EdgeUse[] CreateTriangleEdgeUses()
    {
        Vertex vertex1 =
            CreateVertex(
                0.0,
                0.0,
                0.0);

        Vertex vertex2 =
            CreateVertex(
                1.0,
                0.0,
                0.0);

        Vertex vertex3 =
            CreateVertex(
                0.0,
                1.0,
                0.0);

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

        return
        [
            new EdgeUse(
                edge1,
                EdgeUseDirection.Forward),

            new EdgeUse(
                edge2,
                EdgeUseDirection.Forward),

            new EdgeUse(
                edge3,
                EdgeUseDirection.Forward)
        ];
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