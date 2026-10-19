using MPC.Application.Manufacturing;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class PlanarCurvedFaceAngleQueryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedGeneratorLine_ProducesLocalRightAngle(bool conical)
    {
        // Plane y=0 contains both a cylinder generator and a cone generator.
        double endX = conical ? 3.0 : 1.0;
        var a = new Vertex(1, 0, 1);
        var b = new Vertex(endX, 0, 3);
        var c = new Vertex(0, 0, 2);
        var direction = conical
            ? new Vector3D(Math.Sqrt(0.5), 0, Math.Sqrt(0.5))
            : new Vector3D(0, 0, 1);
        var shared = new Edge(a, b, new Line3D(new Point3D(1, 0, 1), direction));
        var bc = new Edge(b, c, new Line3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1)));
        var ca = new Edge(c, a, new Line3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1)));
        var plane = new Face(new Plane3D(new Point3D(0, 0, 0),
            new Vector3D(0, 1, 0)),
            new Loop([new EdgeUse(shared), new EdgeUse(bc), new EdgeUse(ca)]));
        ISurface3D surface = conical
            ? new Cone3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), Math.PI / 4)
            : new Cylinder3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), 1);
        // Only the generator is topologically shared; the remaining
        // boundary edges are independent instances on the second face.
        var curved = new Face(surface,
            new Loop([new EdgeUse(shared),
                new EdgeUse(new Edge(b, c, bc.Curve)),
                new EdgeUse(new Edge(c, a, ca.Curve))]));
        Assert.True(PlanarCurvedFaceAngleQuery.TryCalculate(
            [plane, curved], plane, curved, out var angle));
        Assert.InRange(angle, 90 - 1e-8, 90 + 1e-8);
    }
}
