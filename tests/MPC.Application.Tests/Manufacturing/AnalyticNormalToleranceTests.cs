using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class AnalyticNormalToleranceTests
{
    [Fact]
    public void Cylinder_ExplicitToleranceControlsNearSurfaceMembership()
    {
        var cylinder = new Cylinder3D(new Point3D(0,0,0),
            new Vector3D(0,0,1),2);
        var witness = new Point3D(2.0000001,0,1);
        Assert.False(CylinderLocalNormalQuery.TryEvaluate(cylinder,witness,out _));
        Assert.False(CylinderLocalNormalQuery.TryEvaluate(cylinder,witness,1e-8,out _));
        Assert.True(CylinderLocalNormalQuery.TryEvaluate(cylinder,witness,1e-6,out var normal));
        Assert.Equal(1,normal.X,8);
    }

    [Fact]
    public void Cone_ExplicitToleranceControlsNearSurfaceMembership()
    {
        var cone = new Cone3D(new Point3D(0,0,0),
            new Vector3D(0,0,1),Math.PI/4);
        var witness = new Point3D(2.0000001,0,2);
        Assert.False(ConeLocalNormalQuery.TryEvaluate(cone,witness,out _));
        Assert.False(ConeLocalNormalQuery.TryEvaluate(cone,witness,1e-8,out _));
        Assert.True(ConeLocalNormalQuery.TryEvaluate(cone,witness,1e-6,out var normal));
        Assert.InRange(normal.X,0.70,0.72);
    }

    [Fact]
    public void NegativeAndNonFiniteTolerancesAreRejected()
    {
        var cylinder = new Cylinder3D(new Point3D(0,0,0),
            new Vector3D(0,0,1),2);
        var cone = new Cone3D(new Point3D(0,0,0),
            new Vector3D(0,0,1),Math.PI/4);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CylinderLocalNormalQuery.TryEvaluate(cylinder,new Point3D(2,0,0),-1,out _));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConeLocalNormalQuery.TryEvaluate(cone,new Point3D(2,0,2),double.NaN,out _));
    }
}
