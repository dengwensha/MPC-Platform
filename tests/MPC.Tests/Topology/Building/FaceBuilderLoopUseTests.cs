using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class FaceBuilderLoopUseTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    private sealed class TestSurface3D : ISurface3D
    {
    }

    [Fact]
    public void Build_PreservesOuterAndInnerLoopUseDirections()
    {
        Loop outerLoop =
            CreateLoop(0.0);

        Loop firstInnerLoop =
            CreateLoop(10.0);

        Loop secondInnerLoop =
            CreateLoop(20.0);

        Face face =
            new FaceBuilder(
                new TestSurface3D())
                .SetOuterLoop(
                    outerLoop,
                    LoopUseDirection.Reversed)
                .AddInnerLoop(
                    firstInnerLoop,
                    LoopUseDirection.Forward)
                .AddInnerLoop(
                    secondInnerLoop,
                    LoopUseDirection.Reversed)
                .Build();

        Assert.Same(
            outerLoop,
            face.OuterLoopUse.Loop);

        Assert.Equal(
            LoopUseDirection.Reversed,
            face.OuterLoopUse.Direction);

        Assert.Equal(
            2,
            face.InnerLoopUses.Count);

        Assert.Same(
            firstInnerLoop,
            face.InnerLoopUses[0].Loop);

        Assert.Equal(
            LoopUseDirection.Forward,
            face.InnerLoopUses[0].Direction);

        Assert.Same(
            secondInnerLoop,
            face.InnerLoopUses[1].Loop);

        Assert.Equal(
            LoopUseDirection.Reversed,
            face.InnerLoopUses[1].Direction);
    }

    [Fact]
    public void ExistingBuilderMethods_DefaultToForwardLoopUseDirection()
    {
        Loop outerLoop =
            CreateLoop(0.0);

        Loop innerLoop =
            CreateLoop(10.0);

        Face face =
            new FaceBuilder(
                new TestSurface3D())
                .SetOuterLoop(
                    outerLoop)
                .AddInnerLoop(
                    innerLoop)
                .Build();

        Assert.Equal(
            LoopUseDirection.Forward,
            face.OuterLoopUse.Direction);

        Assert.Single(
            face.InnerLoopUses);

        Assert.Equal(
            LoopUseDirection.Forward,
            face.InnerLoopUses[0].Direction);
    }

    [Fact]
    public void ExistingFaceConstructor_DefaultsAllLoopUsesToForward()
    {
        Loop outerLoop =
            CreateLoop(0.0);

        Loop firstInnerLoop =
            CreateLoop(10.0);

        Loop secondInnerLoop =
            CreateLoop(20.0);

        Face face =
            new(
                new TestSurface3D(),
                outerLoop,
                [
                    firstInnerLoop,
                    secondInnerLoop
                ]);

        Assert.Equal(
            LoopUseDirection.Forward,
            face.OuterLoopUse.Direction);

        Assert.All(
            face.InnerLoopUses,
            loopUse =>
                Assert.Equal(
                    LoopUseDirection.Forward,
                    loopUse.Direction));
    }

    [Fact]
    public void SameLoop_CanBeUsedByDifferentFacesInDifferentDirections()
    {
        Loop sharedLoop =
            CreateLoop(0.0);

        Face forwardFace =
            new FaceBuilder(
                new TestSurface3D())
                .SetOuterLoop(
                    sharedLoop,
                    LoopUseDirection.Forward)
                .Build();

        Face reversedFace =
            new FaceBuilder(
                new TestSurface3D())
                .SetOuterLoop(
                    sharedLoop,
                    LoopUseDirection.Reversed)
                .Build();

        Assert.Same(
            forwardFace.OuterLoop,
            reversedFace.OuterLoop);

        Assert.Equal(
            LoopUseDirection.Forward,
            forwardFace.OuterLoopUse.Direction);

        Assert.Equal(
            LoopUseDirection.Reversed,
            reversedFace.OuterLoopUse.Direction);
    }

    [Fact]
    public void SetOuterLoop_WithUnsupportedDirection_ThrowsArgumentOutOfRangeException()
    {
        FaceBuilder builder =
            new(
                new TestSurface3D());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.SetOuterLoop(
                CreateLoop(0.0),
                (LoopUseDirection)999));
    }

    [Fact]
    public void AddInnerLoop_WithUnsupportedDirection_ThrowsArgumentOutOfRangeException()
    {
        FaceBuilder builder =
            new FaceBuilder(
                new TestSurface3D())
                .SetOuterLoop(
                    CreateLoop(0.0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.AddInnerLoop(
                CreateLoop(10.0),
                (LoopUseDirection)999));
    }

    private static Loop CreateLoop(
        double xOffset)
    {
        var curve =
            new TestCurve3D();

        var firstVertex =
            new Vertex(
                new Point3D(
                    xOffset,
                    0.0,
                    0.0));

        var secondVertex =
            new Vertex(
                new Point3D(
                    xOffset + 1.0,
                    0.0,
                    0.0));

        var thirdVertex =
            new Vertex(
                new Point3D(
                    xOffset,
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