using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology;

public sealed class LoopUseTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    [Fact]
    public void Constructor_WithForwardDirection_PreservesLoop()
    {
        Loop loop =
            CreateTriangleLoop();

        LoopUse loopUse =
            new(
                loop,
                LoopUseDirection.Forward);

        Assert.Same(
            loop,
            loopUse.Loop);

        Assert.Equal(
            LoopUseDirection.Forward,
            loopUse.Direction);
    }

    [Fact]
    public void Constructor_WithReversedDirection_PreservesLoop()
    {
        Loop loop =
            CreateTriangleLoop();

        LoopUse loopUse =
            new(
                loop,
                LoopUseDirection.Reversed);

        Assert.Same(
            loop,
            loopUse.Loop);

        Assert.Equal(
            LoopUseDirection.Reversed,
            loopUse.Direction);
    }

    [Fact]
    public void SameLoop_CanBeUsedInBothDirections()
    {
        Loop loop =
            CreateTriangleLoop();

        LoopUse forward =
            new(
                loop,
                LoopUseDirection.Forward);

        LoopUse reversed =
            new(
                loop,
                LoopUseDirection.Reversed);

        Assert.Same(
            forward.Loop,
            reversed.Loop);

        Assert.Equal(
            LoopUseDirection.Forward,
            forward.Direction);

        Assert.Equal(
            LoopUseDirection.Reversed,
            reversed.Direction);
    }

    [Fact]
    public void Constructor_WithNullLoop_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new LoopUse(
                null!));
    }

    [Fact]
    public void Constructor_WithUnsupportedDirection_ThrowsArgumentOutOfRangeException()
    {
        Loop loop =
            CreateTriangleLoop();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new LoopUse(
                loop,
                (LoopUseDirection)999));
    }

    private static Loop CreateTriangleLoop()
    {
        var curve =
            new TestCurve3D();

        var firstVertex =
            new Vertex(
                new Point3D(
                    0.0,
                    0.0,
                    0.0));

        var secondVertex =
            new Vertex(
                new Point3D(
                    1.0,
                    0.0,
                    0.0));

        var thirdVertex =
            new Vertex(
                new Point3D(
                    0.0,
                    1.0,
                    0.0));

        var firstEdge =
            new Edge(
                firstVertex,
                secondVertex,
                curve);

        var secondEdge =
            new Edge(
                secondVertex,
                thirdVertex,
                curve);

        var thirdEdge =
            new Edge(
                thirdVertex,
                firstVertex,
                curve);

        return new Loop(
            [
                new EdgeUse(
                    firstEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    secondEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    thirdEdge,
                    EdgeUseDirection.Forward)
            ]);
    }
}