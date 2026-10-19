using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class LocalG0PointMembershipQueryTests
{
    [Theory]
    [InlineData(0, LocalG0Status.CoincidentAtSample)]
    [InlineData(0.1, LocalG0Status.MismatchAtSample)]
    public void SharedLine_MembershipIsNotAssumedFromTopology(
        double planeOffset, LocalG0Status expected)
    {
        var a = new Vertex(1, 0, 0);
        var b = new Vertex(1, 0, 2);
        var c = new Vertex(2, 0, 2);
        var curve = new Line3D(new Point3D(1, 0, 0), new Vector3D(0, 0, 1));
        var common = new Edge(a, b, curve);
        var first = new Face(new Plane3D(new Point3D(0, planeOffset, 0),
            new Vector3D(0, 1, 0)), new Loop([
                new EdgeUse(common), new EdgeUse(new Edge(b, c, curve)),
                new EdgeUse(new Edge(c, a, curve))]));
        var second = new Face(new Cylinder3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), 1), new Loop([
                new EdgeUse(common), new EdgeUse(new Edge(b, c, curve)),
                new EdgeUse(new Edge(c, a, curve))]));
        // Other boundaries are independent canonical edges; only one common edge.
        var result = LocalG0PointMembershipQuery.Evaluate(
            [first, second], first, second, 1e-6, out _);
        Assert.Equal(expected, result);
    }
}
