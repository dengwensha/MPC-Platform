using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class PlanarFaceAngleQueryTests
{
    [Theory]
    [InlineData(0, 0, 1, 0)]
    [InlineData(1, 0, 0, 90)]
    [InlineData(0, 0, -1, 180)]
    [InlineData(0.7071067811865476, 0, 0.7071067811865476, 45)]
    public void SharedEdge_ReturnsSurfaceNormalAngle(
        double x, double y, double z, double expected)
    {
        var (a, b) = Pair(new Vector3D(x, y, z), true);
        Assert.True(PlanarFaceAngleQuery.TryCalculate([a, b], a, b, out var angle));
        Assert.InRange(angle, expected - 1e-8, expected + 1e-8);
    }

    [Fact]
    public void IndependentEdges_AreNotAdjacent()
    {
        var (a, b) = Pair(new Vector3D(1, 0, 0), false);
        Assert.False(PlanarFaceAngleQuery.TryCalculate([a, b], a, b, out _));
    }

    [Fact]
    public void SameFace_IsNotAccepted()
    {
        var (a, b) = Pair(new Vector3D(1, 0, 0), true);
        Assert.False(PlanarFaceAngleQuery.TryCalculate([a, b], a, a, out _));
    }

    private static (Face, Face) Pair(Vector3D otherNormal, bool shared)
    {
        var a = new Vertex(0, 0, 0);
        var b = new Vertex(1, 0, 0);
        var c = new Vertex(0, 1, 0);
        var d = new Vertex(0, -1, 0);
        var curve = new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var ab = new Edge(a, b, curve);
        var bc = new Edge(b, c, curve);
        var ca = new Edge(c, a, curve);
        var ad = new Edge(a, d, curve);
        var db = new Edge(d, b, curve);
        var secondary = shared ? ab : new Edge(a, b, curve);
        var first = new Face(
            new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1)),
            new Loop([new EdgeUse(ab), new EdgeUse(bc), new EdgeUse(ca)]));
        var second = new Face(
            new Plane3D(new Point3D(0, 0, 0), otherNormal),
            new Loop([
                new EdgeUse(secondary, EdgeUseDirection.Reversed),
                new EdgeUse(ad), new EdgeUse(db)]));
        return (first, second);
    }
}
