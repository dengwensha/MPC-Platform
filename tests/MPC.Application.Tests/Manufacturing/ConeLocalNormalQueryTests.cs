using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class ConeLocalNormalQueryTests
{
    [Fact]
    public void PositiveAxisCone_ReturnsAnalyticNormal()
    {
        var cone = new Cone3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), Math.PI / 4);
        Assert.True(ConeLocalNormalQuery.TryEvaluate(cone,
            new Point3D(2, 0, 2), out Vector3D normal));
        Assert.Equal(Math.Sqrt(0.5), normal.X, 10);
        Assert.Equal(0, normal.Y, 10);
        Assert.Equal(-Math.Sqrt(0.5), normal.Z, 10);
    }

    [Fact]
    public void RotatedAxis_IsSupported()
    {
        var cone = new Cone3D(new Point3D(3, 4, 5),
            new Vector3D(1, 0, 0), Math.PI / 4);
        Assert.True(ConeLocalNormalQuery.TryEvaluate(cone,
            new Point3D(5, 6, 5), out Vector3D normal));
        Assert.Equal(-Math.Sqrt(0.5), normal.X, 10);
        Assert.Equal(Math.Sqrt(0.5), normal.Y, 10);
        Assert.Equal(0, normal.Z, 10);
    }

    [Fact]
    public void ApexOppositeNappeAndOffSurface_AreRejected()
    {
        var cone = new Cone3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), Math.PI / 4);
        Assert.False(ConeLocalNormalQuery.TryEvaluate(cone,
            new Point3D(0, 0, 0), out _));
        Assert.False(ConeLocalNormalQuery.TryEvaluate(cone,
            new Point3D(2, 0, -2), out _));
        Assert.False(ConeLocalNormalQuery.TryEvaluate(cone,
            new Point3D(1, 0, 2), out _));
    }

    [Fact]
    public void NonFinitePoint_IsRejected()
    {
        var cone = new Cone3D(new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1), Math.PI / 4);
        Assert.False(ConeLocalNormalQuery.TryEvaluate(cone,
            new Point3D(double.NaN, 0, 2), out _));
    }
}
