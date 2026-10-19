using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class CylinderLocalNormalQueryTests
{
    [Theory]
    [InlineData(2, 0, 5, 1, 0, 0)]
    [InlineData(0, -2, -4, 0, -1, 0)]
    [InlineData(-2, 0, 0, -1, 0, 0)]
    public void AxisAlignedCylinder_ReturnsRadialNormal(
        double x, double y, double z,
        double nx, double ny, double nz)
    {
        var cylinder = new Cylinder3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), 2);
        Assert.True(CylinderLocalNormalQuery.TryEvaluate(cylinder,
            new Point3D(x, y, z), out var normal));
        Assert.Equal(nx, normal.X, 10);
        Assert.Equal(ny, normal.Y, 10);
        Assert.Equal(nz, normal.Z, 10);
    }

    [Fact]
    public void ArbitraryAxis_NormalIsPerpendicularToAxis()
    {
        var cylinder = new Cylinder3D(new Point3D(3, 4, 5),
            new Vector3D(1, 0, 0), 2);
        Assert.True(CylinderLocalNormalQuery.TryEvaluate(cylinder,
            new Point3D(9, 6, 5), out var normal));
        Assert.Equal(0, normal.X, 10);
        Assert.Equal(1, normal.Y, 10);
        Assert.Equal(0, normal.Z, 10);
    }

    [Fact]
    public void OffSurfacePoint_IsRejected()
    {
        var cylinder = new Cylinder3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), 2);
        Assert.False(CylinderLocalNormalQuery.TryEvaluate(cylinder,
            new Point3D(1, 0, 0), out _));
        Assert.False(CylinderLocalNormalQuery.TryEvaluate(cylinder,
            new Point3D(0, 0, 8), out _));
    }

    [Fact]
    public void NonFinitePoint_IsRejected()
    {
        var cylinder = new Cylinder3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), 2);
        Assert.False(CylinderLocalNormalQuery.TryEvaluate(cylinder,
            new Point3D(double.NaN, 2, 1), out _));
    }
}
