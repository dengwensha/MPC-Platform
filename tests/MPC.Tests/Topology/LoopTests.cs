using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology;

public sealed class LoopTests
{
    [Fact]
    public void Constructor_WithClosedTriangle_CreatesLoop()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(0.0, 1.0, 0.0);

        EdgeUse edgeUse1 =
            new(CreateEdge(vertex1, vertex2));

        EdgeUse edgeUse2 =
            new(CreateEdge(vertex2, vertex3));

        EdgeUse edgeUse3 =
            new(CreateEdge(vertex3, vertex1));

        Loop loop = new(
            new[]
            {
                edgeUse1,
                edgeUse2,
                edgeUse3
            });

        Assert.NotEqual(Guid.Empty, loop.Id);
        Assert.Equal(3, loop.EdgeUses.Count);

        Assert.Same(edgeUse1, loop.EdgeUses[0]);
        Assert.Same(edgeUse2, loop.EdgeUses[1]);
        Assert.Same(edgeUse3, loop.EdgeUses[2]);
    }

    [Fact]
    public void Constructor_WithClosedRectangle_CreatesLoop()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(2.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(2.0, 1.0, 0.0);
        Vertex vertex4 = CreateVertex(0.0, 1.0, 0.0);

        Loop loop = new(
            new[]
            {
                new EdgeUse(CreateEdge(vertex1, vertex2)),
                new EdgeUse(CreateEdge(vertex2, vertex3)),
                new EdgeUse(CreateEdge(vertex3, vertex4)),
                new EdgeUse(CreateEdge(vertex4, vertex1))
            });

        Assert.Equal(4, loop.EdgeUses.Count);
    }

    [Fact]
    public void Constructor_WithReversedEdgeUse_CreatesContinuousLoop()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(0.0, 1.0, 0.0);

        Edge edge1 = CreateEdge(vertex1, vertex2);
        Edge edge2 = CreateEdge(vertex3, vertex2);
        Edge edge3 = CreateEdge(vertex3, vertex1);

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

        Assert.Equal(3, loop.EdgeUses.Count);

        Assert.Same(
            vertex2,
            loop.EdgeUses[1].StartVertex);

        Assert.Same(
            vertex3,
            loop.EdgeUses[1].EndVertex);
    }

    [Fact]
    public void Constructor_WithNullCollection_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Loop(null!));
    }

    [Fact]
    public void Constructor_WithFewerThanTwoEdgeUses_ThrowsArgumentException()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);

        EdgeUse edgeUse =
            new(CreateEdge(vertex1, vertex2));

        Assert.Throws<ArgumentException>(
            () => new Loop(
                new[]
                {
                    edgeUse
                }));
    }

    [Fact]
    public void Constructor_WithNullEdgeUse_ThrowsArgumentException()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(0.0, 1.0, 0.0);

        EdgeUse edgeUse1 =
            new(CreateEdge(vertex1, vertex2));

        EdgeUse edgeUse2 =
            new(CreateEdge(vertex2, vertex3));

        Assert.Throws<ArgumentException>(
            () => new Loop(
                new EdgeUse[]
                {
                    edgeUse1,
                    edgeUse2,
                    null!
                }));
    }

    [Fact]
    public void Constructor_WithBrokenContinuity_ThrowsArgumentException()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(0.0, 1.0, 0.0);

        Vertex disconnectedVertex =
            CreateVertex(5.0, 5.0, 0.0);

        Edge edge1 = CreateEdge(vertex1, vertex2);
        Edge edge2 = CreateEdge(disconnectedVertex, vertex3);
        Edge edge3 = CreateEdge(vertex3, vertex1);

        Assert.Throws<ArgumentException>(
            () => new Loop(
                new[]
                {
                    new EdgeUse(edge1),
                    new EdgeUse(edge2),
                    new EdgeUse(edge3)
                }));
    }

    [Fact]
    public void Constructor_WithOpenChain_ThrowsArgumentException()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(1.0, 1.0, 0.0);
        Vertex vertex4 = CreateVertex(0.0, 1.0, 0.0);

        Edge edge1 = CreateEdge(vertex1, vertex2);
        Edge edge2 = CreateEdge(vertex2, vertex3);
        Edge edge3 = CreateEdge(vertex3, vertex4);

        Assert.Throws<ArgumentException>(
            () => new Loop(
                new[]
                {
                    new EdgeUse(edge1),
                    new EdgeUse(edge2),
                    new EdgeUse(edge3)
                }));
    }

    [Fact]
    public void Constructor_WithSameCoordinatesButDifferentVertexIdentity_ThrowsArgumentException()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);

        Vertex differentVertexAtSamePosition =
            CreateVertex(1.0, 0.0, 0.0);

        Vertex vertex3 = CreateVertex(0.0, 1.0, 0.0);

        Edge edge1 = CreateEdge(vertex1, vertex2);

        Edge edge2 = CreateEdge(
            differentVertexAtSamePosition,
            vertex3);

        Edge edge3 = CreateEdge(vertex3, vertex1);

        Assert.Throws<ArgumentException>(
            () => new Loop(
                new[]
                {
                    new EdgeUse(edge1),
                    new EdgeUse(edge2),
                    new EdgeUse(edge3)
                }));
    }

    [Fact]
    public void Constructor_CopiesInputCollection()
    {
        Vertex vertex1 = CreateVertex(0.0, 0.0, 0.0);
        Vertex vertex2 = CreateVertex(1.0, 0.0, 0.0);
        Vertex vertex3 = CreateVertex(0.0, 1.0, 0.0);

        List<EdgeUse> edgeUses =
        [
            new EdgeUse(CreateEdge(vertex1, vertex2)),
            new EdgeUse(CreateEdge(vertex2, vertex3)),
            new EdgeUse(CreateEdge(vertex3, vertex1))
        ];

        Loop loop = new(edgeUses);

        edgeUses.Clear();

        Assert.Equal(3, loop.EdgeUses.Count);
    }

    private static Vertex CreateVertex(
        double x,
        double y,
        double z)
    {
        return new Vertex(
            new Point3D(x, y, z));
    }

    private static Edge CreateEdge(
        Vertex start,
        Vertex end)
    {
        Vector3D direction = CreateUnitDirection(
            start.Position,
            end.Position);

        Line3D curve = new(
            start.Position,
            direction);

        return new Edge(
            start,
            end,
            curve);
    }

    private static Vector3D CreateUnitDirection(
        Point3D start,
        Point3D end)
    {
        double x = end.X - start.X;
        double y = end.Y - start.Y;
        double z = end.Z - start.Z;

        double length = Math.Sqrt(
            x * x +
            y * y +
            z * z);

        if (length == 0.0)
        {
            throw new ArgumentException(
                "Cannot create a line direction from identical points.");
        }

        return new Vector3D(
            x / length,
            y / length,
            z / length);
    }
}