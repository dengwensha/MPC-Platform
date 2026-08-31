using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology;

public sealed class TwoEdgeLoopSupportTests
{
    [Fact]
    public void Constructor_WithContinuousClosedTwoEdgeCycle_ReturnsLoop()
    {
        EdgeUse[] edgeUses =
            CreateTwoEdgeCycle();

        var loop =
            new Loop(
                edgeUses);

        Assert.Equal(
            2,
            loop.EdgeUses.Count);

        Assert.Same(
            edgeUses[0],
            loop.EdgeUses[0]);

        Assert.Same(
            edgeUses[1],
            loop.EdgeUses[1]);

        Assert.Same(
            loop.EdgeUses[0].EndVertex,
            loop.EdgeUses[1].StartVertex);

        Assert.Same(
            loop.EdgeUses[1].EndVertex,
            loop.EdgeUses[0].StartVertex);
    }

    [Fact]
    public void BuildLoop_WithContinuousClosedTwoEdgeCycle_RegistersAndVerifies()
    {
        EdgeUse[] edgeUses =
            CreateTwoEdgeCycle();

        var builder =
            new TopologyBuilder();

        Loop loop =
            builder.BuildLoop(
                edgeUses);

        Assert.Same(
            loop,
            Assert.Single(
                builder.Loops));

        Assert.Equal(
            2,
            builder.Edges.Count);

        Assert.Equal(
            2,
            builder.Vertices.Count);

        Exception? verificationException =
            Record.Exception(
                builder.Verify);

        Assert.Null(
            verificationException);
    }

    [Fact]
    public void Constructor_WithDiscontinuousTwoEdgeCycle_ThrowsArgumentException()
    {
        var vertex1 =
            new Vertex(
                new Point3D(
                    0.0,
                    0.0,
                    0.0));

        var vertex2 =
            new Vertex(
                new Point3D(
                    1.0,
                    0.0,
                    0.0));

        var vertex3 =
            new Vertex(
                new Point3D(
                    2.0,
                    0.0,
                    0.0));

        var edge1 =
            new Edge(
                vertex1,
                vertex2,
                new TestCurve3D());

        var edge2 =
            new Edge(
                vertex3,
                vertex1,
                new TestCurve3D());

        Assert.Throws<ArgumentException>(
            () => new Loop(
                [
                    new EdgeUse(
                        edge1),

                    new EdgeUse(
                        edge2)
                ]));
    }

    private static EdgeUse[] CreateTwoEdgeCycle()
    {
        var vertex1 =
            new Vertex(
                new Point3D(
                    0.0,
                    0.0,
                    0.0));

        var vertex2 =
            new Vertex(
                new Point3D(
                    1.0,
                    0.0,
                    0.0));

        var edge1 =
            new Edge(
                vertex1,
                vertex2,
                new TestCurve3D());

        var edge2 =
            new Edge(
                vertex2,
                vertex1,
                new TestCurve3D());

        return
        [
            new EdgeUse(
                edge1),

            new EdgeUse(
                edge2)
        ];
    }

    private sealed class TestCurve3D :
        ICurve3D
    {
    }
}