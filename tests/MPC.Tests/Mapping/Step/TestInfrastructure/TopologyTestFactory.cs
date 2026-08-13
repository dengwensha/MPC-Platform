using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step.TestInfrastructure;

internal static class TopologyTestFactory
{
    public static Face CreateFace(
        double offset = 0.0,
        ISurface3D? surface = null,
        FaceOrientation orientation =
            FaceOrientation.Forward)
    {
        return new Face(
            surface ?? new TestSurface3D(),
            CreateTriangleLoop(
                offset),
            orientation:
                orientation);
    }

    public static Shell CreateClosedTetrahedronShell()
    {
        var curve =
            new TestCurve3D();

        Vertex a =
            CreateVertex(
                0.0,
                0.0,
                0.0);

        Vertex b =
            CreateVertex(
                1.0,
                0.0,
                0.0);

        Vertex c =
            CreateVertex(
                0.0,
                1.0,
                0.0);

        Vertex d =
            CreateVertex(
                0.0,
                0.0,
                1.0);

        Edge ab =
            CreateEdge(
                a,
                b,
                curve);

        Edge ac =
            CreateEdge(
                a,
                c,
                curve);

        Edge ad =
            CreateEdge(
                a,
                d,
                curve);

        Edge bc =
            CreateEdge(
                b,
                c,
                curve);

        Edge bd =
            CreateEdge(
                b,
                d,
                curve);

        Edge cd =
            CreateEdge(
                c,
                d,
                curve);

        Loop abc =
            CreateLoop(
                new EdgeUse(
                    ab,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    bc,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    ac,
                    EdgeUseDirection.Reversed));

        Loop adb =
            CreateLoop(
                new EdgeUse(
                    ad,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    bd,
                    EdgeUseDirection.Reversed),
                new EdgeUse(
                    ab,
                    EdgeUseDirection.Reversed));

        Loop bcd =
            CreateLoop(
                new EdgeUse(
                    bc,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    cd,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    bd,
                    EdgeUseDirection.Reversed));

        Loop acd =
            CreateLoop(
                new EdgeUse(
                    ac,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    cd,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    ad,
                    EdgeUseDirection.Reversed));

        return new Shell(
            [
                CreateFace(
                    abc),

                CreateFace(
                    adb),

                CreateFace(
                    bcd),

                CreateFace(
                    acd)
            ]);
    }

    public static Loop CreateTriangleLoop(
        double offset = 0.0)
    {
        Vertex firstVertex =
            CreateVertex(
                offset,
                0.0);

        Vertex secondVertex =
            CreateVertex(
                offset + 1.0,
                0.0);

        Vertex thirdVertex =
            CreateVertex(
                offset,
                1.0);

        Edge firstEdge =
            CreateEdge(
                firstVertex,
                secondVertex);

        Edge secondEdge =
            CreateEdge(
                secondVertex,
                thirdVertex);

        Edge thirdEdge =
            CreateEdge(
                thirdVertex,
                firstVertex);

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

    public static Vertex CreateVertex(
        double x,
        double y,
        double z = 0.0)
    {
        return new Vertex(
            new Point3D(
                x,
                y,
                z));
    }

    public static Edge CreateEdge(
        Vertex startVertex,
        Vertex endVertex,
        ICurve3D? curve = null)
    {
        ArgumentNullException.ThrowIfNull(
            startVertex);

        ArgumentNullException.ThrowIfNull(
            endVertex);

        return new Edge(
            startVertex,
            endVertex,
            curve ?? new TestCurve3D());
    }

    private static Loop CreateLoop(
        params EdgeUse[] edgeUses)
    {
        return new Loop(
            edgeUses);
    }

    private static Face CreateFace(
        Loop outerLoop)
    {
        return new Face(
            new TestSurface3D(),
            outerLoop);
    }
}