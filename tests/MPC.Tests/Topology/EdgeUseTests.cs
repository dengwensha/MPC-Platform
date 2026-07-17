using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology;

public sealed class EdgeUseTests
{
    [Fact]
    public void Constructor_WithForwardDirection_UsesEdgeDirection()
    {
        Vertex start = CreateVertex(0.0, 0.0, 0.0);
        Vertex end = CreateVertex(1.0, 0.0, 0.0);
        Edge edge = CreateEdge(start, end);

        EdgeUse edgeUse = new(
            edge,
            EdgeUseDirection.Forward);

        Assert.Same(edge, edgeUse.Edge);
        Assert.Equal(
            EdgeUseDirection.Forward,
            edgeUse.Direction);

        Assert.Same(start, edgeUse.StartVertex);
        Assert.Same(end, edgeUse.EndVertex);
    }

    [Fact]
    public void Constructor_WithReversedDirection_ReversesTraversal()
    {
        Vertex start = CreateVertex(0.0, 0.0, 0.0);
        Vertex end = CreateVertex(1.0, 0.0, 0.0);
        Edge edge = CreateEdge(start, end);

        EdgeUse edgeUse = new(
            edge,
            EdgeUseDirection.Reversed);

        Assert.Same(edge, edgeUse.Edge);
        Assert.Equal(
            EdgeUseDirection.Reversed,
            edgeUse.Direction);

        Assert.Same(end, edgeUse.StartVertex);
        Assert.Same(start, edgeUse.EndVertex);
    }

    [Fact]
    public void SameEdge_CanBeUsedInBothDirections()
    {
        Vertex start = CreateVertex(0.0, 0.0, 0.0);
        Vertex end = CreateVertex(1.0, 0.0, 0.0);
        Edge edge = CreateEdge(start, end);

        EdgeUse forward = new(
            edge,
            EdgeUseDirection.Forward);

        EdgeUse reversed = new(
            edge,
            EdgeUseDirection.Reversed);

        Assert.Same(forward.Edge, reversed.Edge);

        Assert.Same(start, forward.StartVertex);
        Assert.Same(end, forward.EndVertex);

        Assert.Same(end, reversed.StartVertex);
        Assert.Same(start, reversed.EndVertex);
    }

    [Fact]
    public void Constructor_WithNullEdge_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new EdgeUse(null!));
    }

    [Fact]
    public void Constructor_WithUnsupportedDirection_ThrowsArgumentOutOfRangeException()
    {
        Vertex start = CreateVertex(0.0, 0.0, 0.0);
        Vertex end = CreateVertex(1.0, 0.0, 0.0);
        Edge edge = CreateEdge(start, end);

        EdgeUseDirection unsupportedDirection =
            (EdgeUseDirection)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EdgeUse(
                edge,
                unsupportedDirection));
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
        Vector3D direction = new(
            end.Position.X - start.Position.X,
            end.Position.Y - start.Position.Y,
            end.Position.Z - start.Position.Z);

        Line3D curve = new(
            start.Position,
            direction);

        return new Edge(
            start,
            end,
            curve);
    }
}