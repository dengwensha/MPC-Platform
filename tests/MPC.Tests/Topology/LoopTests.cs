using System;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;
using Xunit;

namespace MPC.Tests.Topology;

public sealed class LoopTests
{
    [Fact]
    public void Constructor_WithTriangleLoop_ShouldCreateLoop()
    {
        var v1 = new Vertex(0, 0, 0);
        var v2 = new Vertex(1, 0, 0);
        var v3 = new Vertex(0, 1, 0);

        var edges = new[]
        {
            CreateEdge(v1, v2),
            CreateEdge(v2, v3),
            CreateEdge(v3, v1)
        };

        var loop = new Loop(edges);

        Assert.Equal(3, loop.Count);
    }

    [Fact]
    public void Constructor_WithRectangleLoop_ShouldCreateLoop()
    {
        var v1 = new Vertex(0, 0, 0);
        var v2 = new Vertex(1, 0, 0);
        var v3 = new Vertex(1, 1, 0);
        var v4 = new Vertex(0, 1, 0);

        var edges = new[]
        {
            CreateEdge(v1, v2),
            CreateEdge(v2, v3),
            CreateEdge(v3, v4),
            CreateEdge(v4, v1)
        };

        var loop = new Loop(edges);

        Assert.Equal(4, loop.Count);
    }

    [Fact]
    public void Constructor_WithNullCollection_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new Loop(null!));
    }

    [Fact]
    public void Constructor_WithEmptyCollection_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Loop(Array.Empty<Edge>()));
    }

    [Fact]
    public void Constructor_WithTwoEdges_ShouldThrow()
    {
        var v1 = new Vertex(0, 0, 0);
        var v2 = new Vertex(1, 0, 0);

        var edges = new[]
        {
            CreateEdge(v1, v2),
            CreateEdge(v2, v1)
        };

        Assert.Throws<ArgumentException>(() => new Loop(edges));
    }

    [Fact]
    public void Constructor_WithBrokenChain_ShouldThrow()
    {
        var v1 = new Vertex(0, 0, 0);
        var v2 = new Vertex(1, 0, 0);
        var v3 = new Vertex(1, 1, 0);
        var v4 = new Vertex(0, 1, 0);

        var edges = new[]
        {
            CreateEdge(v1, v2),
            CreateEdge(v3, v4),
            CreateEdge(v4, v1)
        };

        Assert.Throws<ArgumentException>(() => new Loop(edges));
    }

    [Fact]
    public void Constructor_WithOpenLoop_ShouldThrow()
    {
        var v1 = new Vertex(0, 0, 0);
        var v2 = new Vertex(1, 0, 0);
        var v3 = new Vertex(1, 1, 0);
        var v4 = new Vertex(0, 1, 0);

        var edges = new[]
        {
            CreateEdge(v1, v2),
            CreateEdge(v2, v3),
            CreateEdge(v3, v4)
        };

        Assert.Throws<ArgumentException>(() => new Loop(edges));
    }

    private static Edge CreateEdge(Vertex start, Vertex end)
    {
        var direction = CreateUnitDirection(
            start.Position,
            end.Position);

        return new Edge(
            start,
            end,
            new Line3D(start.Position, direction));
    }

    private static Vector3D CreateUnitDirection(
        Point3D start,
        Point3D end)
    {
        var x = end.X - start.X;
        var y = end.Y - start.Y;
        var z = end.Z - start.Z;

        var length = Math.Sqrt(
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