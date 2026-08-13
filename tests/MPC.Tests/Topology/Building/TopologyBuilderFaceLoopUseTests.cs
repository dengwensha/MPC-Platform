using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderFaceLoopUseTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    private sealed class TestSurface3D : ISurface3D
    {
    }

    [Fact]
    public void RegisterFace_WithDirectedLoopUses_RegistersCanonicalLoopsWithoutCopying()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Loop innerLoop =
            CreateTriangleLoop(
                0.2,
                0.2);

        Face face =
            new FaceBuilder(
                new TestSurface3D())
                .SetOuterLoop(
                    outerLoop,
                    LoopUseDirection.Reversed)
                .AddInnerLoop(
                    innerLoop,
                    LoopUseDirection.Reversed)
                .Build();

        TopologyBuilder builder =
            new();

        builder.RegisterFace(
            face);

        Assert.Equal(
            2,
            builder.Loops.Count);

        Assert.Contains(
            builder.Loops,
            registeredLoop =>
                ReferenceEquals(
                    registeredLoop,
                    outerLoop));

        Assert.Contains(
            builder.Loops,
            registeredLoop =>
                ReferenceEquals(
                    registeredLoop,
                    innerLoop));

        Assert.Same(
            outerLoop,
            face.OuterLoop);

        Assert.Same(
            innerLoop,
            face.InnerLoops[0]);

        Assert.Equal(
            LoopUseDirection.Reversed,
            face.OuterLoopUse.Direction);

        Assert.Equal(
            LoopUseDirection.Reversed,
            face.InnerLoopUses[0].Direction);
    }

    private static Loop CreateTriangleLoop(
        double xOffset,
        double yOffset)
    {
        var curve =
            new TestCurve3D();

        var firstVertex =
            new Vertex(
                new Point3D(
                    xOffset,
                    yOffset,
                    0.0));

        var secondVertex =
            new Vertex(
                new Point3D(
                    xOffset + 1.0,
                    yOffset,
                    0.0));

        var thirdVertex =
            new Vertex(
                new Point3D(
                    xOffset,
                    yOffset + 1.0,
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