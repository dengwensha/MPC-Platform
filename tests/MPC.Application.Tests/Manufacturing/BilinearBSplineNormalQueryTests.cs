using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class BilinearBSplineNormalQueryTests
{
    private static BSplineSurface3D Patch(Point3D p11) => new(
        1, 1,
        new[]
        {
            new[] { new Point3D(0, 0, 0), new Point3D(0, 1, 0) },
            new[] { new Point3D(1, 0, 0), p11 }
        },
        new[] { 2, 2 }, new[] { 2, 2 },
        new[] { 0.0, 1.0 }, new[] { 0.0, 1.0 });

    [Theory]
    [InlineData(0.25, 0.25)]
    [InlineData(0.5, 0.5)]
    [InlineData(1.0, 1.0)]
    public void PlanarPatch_ProducesUnitNormal(double u, double v)
    {
        Assert.True(BilinearBSplineNormalQuery.TryEvaluate(
            Patch(new Point3D(1, 1, 0)), u, v, out var normal));
        Assert.Equal(0.0, normal.X, 12);
        Assert.Equal(0.0, normal.Y, 12);
        Assert.Equal(1.0, normal.Z, 12);
    }

    [Fact]
    public void TwistedPatch_HasLocationDependentNormal()
    {
        var patch = Patch(new Point3D(1, 1, 1));
        Assert.True(BilinearBSplineNormalQuery.TryEvaluate(patch, 0, 0, out var start));
        Assert.True(BilinearBSplineNormalQuery.TryEvaluate(patch, 1, 1, out var end));
        Assert.Equal(1.0, start.Z, 12);
        Assert.True(end.Z < 1.0);
    }

    [Fact]
    public void OutsideDomain_Rejects()
    {
        Assert.False(BilinearBSplineNormalQuery.TryEvaluate(
            Patch(new Point3D(1, 1, 0)), -0.01, 0.5, out _));
    }

    [Fact]
    public void SingularPatch_Rejects()
    {
        var patch = Patch(new Point3D(0, 1, 0));
        Assert.False(BilinearBSplineNormalQuery.TryEvaluate(patch, 0.5, 0.5, out _));
    }

    [Fact]
    public void UnsupportedHigherDegree_Rejects()
    {
        var patch = new BSplineSurface3D(2, 1,
            new[]
            {
                new[] { new Point3D(0,0,0), new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0), new Point3D(1,1,0) },
                new[] { new Point3D(2,0,0), new Point3D(2,1,0) }
            }, new[] { 3, 3 }, new[] { 2, 2 },
            new[] { 0.0, 1.0 }, new[] { 0.0, 1.0 });
        Assert.False(BilinearBSplineNormalQuery.TryEvaluate(patch, 0.5, 0.5, out _));
    }
}
