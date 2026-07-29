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
        FaceOrientation orientation = FaceOrientation.Forward)
    {
        return new Face(
            surface ?? new TestSurface3D(),
            CreateTriangleLoop(offset),
            orientation: orientation);
    }

    public static Loop CreateTriangleLoop(
        double offset = 0.0)
    {
        Vertex firstVertex =
            CreateVertex(offset, 0.0);

        Vertex secondVertex =
            CreateVertex(offset + 1.0, 0.0);

        Vertex thirdVertex =
            CreateVertex(offset, 1.0);

        Edge firstEdge =
            CreateEdge(firstVertex, secondVertex);

        Edge secondEdge =
            CreateEdge(secondVertex, thirdVertex);

        Edge thirdEdge =
            CreateEdge(thirdVertex, firstVertex);

        return new Loop(
            new[]
            {
                new EdgeUse(
                    firstEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    secondEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    thirdEdge,
                    EdgeUseDirection.Forward)
            });
    }

    public static Vertex CreateVertex(
        double x,
        double y,
        double z = 0.0)
    {
        return new Vertex(
            new Point3D(x, y, z));
    }

    public static Edge CreateEdge(
        Vertex startVertex,
        Vertex endVertex,
        ICurve3D? curve = null)
    {
        ArgumentNullException.ThrowIfNull(startVertex);
        ArgumentNullException.ThrowIfNull(endVertex);

        return new Edge(
            startVertex,
            endVertex,
            curve ?? new TestCurve3D());
    }
}
