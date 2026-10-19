using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class LocalTangentPlaneQueryTests
{
    [Theory]
    [InlineData(0, 0, 1, LocalTangentStatus.TangentAtSample)]
    [InlineData(0, 0, -1, LocalTangentStatus.TangentAtSample)]
    [InlineData(1, 0, 0, LocalTangentStatus.NotTangentAtSample)]
    public void PlanarNeighbors_AreComparedWithoutNormalSign(
        double x, double y, double z, LocalTangentStatus expected)
    {
        var a = new Vertex(0, 0, 0);
        var b = new Vertex(1, 0, 0);
        var c = new Vertex(0, 1, 0);
        var d = new Vertex(0, -1, 0);
        var curve = new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var shared = new Edge(a, b, curve);
        var f1 = new Face(new Plane3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1)),
            new Loop([new EdgeUse(shared),
                new EdgeUse(new Edge(b, c, curve)),
                new EdgeUse(new Edge(c, a, curve))]));
        var f2 = new Face(new Plane3D(new Point3D(0, 0, 0),
            new Vector3D(x, y, z)),
            new Loop([new EdgeUse(shared, EdgeUseDirection.Reversed),
                new EdgeUse(new Edge(a, d, curve)),
                new EdgeUse(new Edge(d, b, curve))]));
        Assert.Equal(expected, LocalTangentPlaneQuery.Evaluate(
            [f1, f2], f1, f2, 0.01, out double angle));
        Assert.InRange(angle, expected == LocalTangentStatus.TangentAtSample
            ? 0 : 90, expected == LocalTangentStatus.TangentAtSample ? 0.001 : 90.001);
    }

    [Fact]
    public void InvalidTolerance_IsRejected()
    {
        var start = new Vertex(0, 0, 0);
        var end = new Vertex(1, 0, 0);
        var edge = new Edge(start, end,
            new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)));
        var face = new Face(new Plane3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1)),
            new Loop([new EdgeUse(edge),
                new EdgeUse(edge, EdgeUseDirection.Reversed)]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LocalTangentPlaneQuery.Evaluate([face], face, face, -0.1, out _));
    }
}
