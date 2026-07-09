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
        return new Edge(
            start,
            end,
            new Line3D(start.Position, end.Position));
    }
}