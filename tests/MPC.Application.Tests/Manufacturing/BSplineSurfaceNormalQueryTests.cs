using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class BSplineSurfaceNormalQueryTests
{
    [Theory]
    [InlineData(0.25, 0.25)]
    [InlineData(0.5, 0.75)]
    [InlineData(1.0, 1.0)]
    public void QuadraticPlanarPatch_UnitNormal(double u, double v)
    {
        var surface = new BSplineSurface3D(2, 2,
            new[]
            {
                new[] { new Point3D(0,0,0),new Point3D(0,1,0),new Point3D(0,2,0) },
                new[] { new Point3D(1,0,0),new Point3D(1,1,0),new Point3D(1,2,0) },
                new[] { new Point3D(2,0,0),new Point3D(2,1,0),new Point3D(2,2,0) }
            }, new[] { 3, 3 },new[] { 3, 3 },
            new[] { 0.0, 1.0 },new[] { 0.0, 1.0 });
        Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(surface,u,v,out var n));
        Assert.Equal(0,n.X,10);
        Assert.Equal(0,n.Y,10);
        Assert.Equal(1,n.Z,10);
        Assert.False(BSplineSurfaceNormalQuery.TryEvaluate(surface,-0.1,v,out _));
    }

    [Fact]
    public void BilinearPatch_AgreesWithE1()
    {
        var surface = new BSplineSurface3D(1,1,
            new[]
            {
                new[] { new Point3D(0,0,0),new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0),new Point3D(1,1,1) }
            },new[] { 2,2 },new[] { 2,2 },
            new[] { 0.0,1.0 },new[] { 0.0,1.0 });
        Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(surface,.3,.7,out var current));
        Assert.True(BilinearBSplineNormalQuery.TryEvaluate(surface,.3,.7,out var prior));
        Assert.Equal(prior.X,current.X,10);
        Assert.Equal(prior.Y,current.Y,10);
        Assert.Equal(prior.Z,current.Z,10);
    }

    [Fact]
    public void RepeatedInteriorKnot_IsConservativelyRejectedAtKnot()
    {
        var surface = new BSplineSurface3D(2,1,
            Enumerable.Range(0,5).Select(i=>new[]
            {
                new Point3D(i,0,0),new Point3D(i,1,0)
            }), new[] { 3,2,3 },new[] { 2,2 },
            new[] { 0.0,.5,1.0 },new[] { 0.0,1.0 });
        Assert.False(BSplineSurfaceNormalQuery.TryEvaluate(surface,.5,.5,out _));
        Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(surface,.25,.5,out _));
    }

    [Theory]
    [InlineData(1e-12)]
    [InlineData(1e-11)]
    public void RoundedPoleBoundary_IsRejected(double noise)
    {
        var basis = new BSplineSurface3D(1, 1,
            new[]
            {
                new[] { new Point3D(0,0,0), new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0), new Point3D(noise,1,0) }
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        Assert.False(BSplineSurfaceNormalQuery.TryEvaluate(basis, 0.37, 1.0, out _));
    }
}
