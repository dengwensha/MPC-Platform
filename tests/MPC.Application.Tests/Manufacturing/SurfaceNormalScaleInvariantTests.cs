using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class SurfaceNormalScaleInvariantTests
{
    private static BSplineSurface3D Patch(double scale) => new(
        1, 1,
        new[]
        {
            new[] { new Point3D(0,0,0),new Point3D(0,scale,0) },
            new[] { new Point3D(scale,0,0),new Point3D(scale,scale,0) }
        },
        new[] {2,2},new[] {2,2},
        new[] {0.0,1.0},new[] {0.0,1.0});

    [Theory]
    [InlineData(0.0001)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void RegularPatch_GeometricScaleDoesNotChangeAcceptance(double scale)
    {
        var basis = Patch(scale);
        Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(basis,.3,.7,out var normal));
        Assert.Equal(1,normal.Z,8);
        Assert.True(BilinearBSplineNormalQuery.TryEvaluate(basis,.3,.7,out normal));
        Assert.Equal(1,normal.Z,8);
    }

    [Theory]
    [InlineData(0.0001)]
    [InlineData(0.001)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    [InlineData(1000000)]
    public void RationalWeights_GlobalScaleDoesNotChangeNormal(double weightScale)
    {
        var patch = Patch(1);
        var rational = new RationalBSplineSurface3D(patch,
            new[] { new[] { weightScale, weightScale * 2 },
                    new[] { weightScale * 3, weightScale * 4 } });
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(rational,.3,.7,out var normal));
        Assert.Equal(0,normal.X,8);
        Assert.Equal(0,normal.Y,8);
        Assert.Equal(1,normal.Z,8);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void CollapsedRow_IsRejectedAtSingularEndpoint(double scale)
    {
        var patch = new BSplineSurface3D(1,1,
            new[]
            {
                new[] { new Point3D(0,0,0), new Point3D(0,0,0) },
                new[] { new Point3D(scale,0,0), new Point3D(scale,0,0) }
            },
            new[] {2,2},new[] {2,2},
            new[] {0.0,1.0},new[] {0.0,1.0});
        Assert.False(BSplineSurfaceNormalQuery.TryEvaluate(patch,.3,1,out _));
        Assert.False(BilinearBSplineNormalQuery.TryEvaluate(patch,.3,1,out _));
        var rational = new RationalBSplineSurface3D(patch,
            new[] { new[] { 1.0,1.0 }, new[] { 1.0,1.0 } });
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(rational,.3,1,out _));
    }
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void CubicCollapsedBoundaryPole_IsRejectedAtAllScales(double scale)
    {
        var grid = Enumerable.Range(0, 4).Select(i =>
            Enumerable.Range(0, 4).Select(j =>
                j == 3
                    ? new Point3D(0, 0, scale)
                    : new Point3D(scale * i, scale * j, 0)).ToArray()).ToArray();
        var patch = new BSplineSurface3D(3, 3, grid,
            new[] {4,4}, new[] {4,4},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        foreach (double u in new[] {0.13, 0.37, 0.61, 0.89})
        {
            Assert.False(BSplineSurfaceNormalQuery.TryEvaluate(patch, u, 1.0, out _));
            foreach (double weightScale in new[] {0.0001, 1.0, 100.0, 1000000.0})
            {
                var weights = Enumerable.Range(0,4)
                    .Select(_ => Enumerable.Repeat(weightScale,4).ToArray()).ToArray();
                var rational = new RationalBSplineSurface3D(patch, weights);
                Assert.False(RationalBSplineNormalQuery.TryEvaluate(rational, u, 1.0, out _));
            }
        }
    }

}
