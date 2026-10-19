using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class BSplineNormalParameterPolicyTests
{
    [Fact]
    public void ClampedEndpoints_AgreeForRationalAndNonRational()
    {
        var basis = new BSplineSurface3D(1,1,
            new[] {
                new[] { new Point3D(0,0,0), new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0), new Point3D(1,1,0) }
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var rational = new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0},new[] {1.0,1.0}});
        foreach (double u in new[] {0.0,1.0})
            foreach (double v in new[] {0.0,1.0})
            {
                Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(basis,u,v,out var a));
                Assert.True(RationalBSplineNormalQuery.TryEvaluate(rational,u,v,out var b));
                Assert.Equal(a.Z,b.Z,10);
            }
    }

    [Fact]
    public void NonClampedDomainBoundary_IsRejectedInBothQueries()
    {
        var basis = new BSplineSurface3D(2,1,
            Enumerable.Range(0,4).Select(i => new[] {
                new Point3D(i,0,0), new Point3D(i,1,0)
            }),new[] {2,1,2,2},new[] {2,2},
            new[] {0.0,0.25,0.75,1.0},new[] {0.0,1.0});
        var rational = new RationalBSplineSurface3D(basis,
            Enumerable.Range(0,4).Select(_ => new[] {1.0,1.0}));
        Assert.False(BSplineSurfaceNormalQuery.TryEvaluate(basis,0.25,0.5,out _));
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(rational,0.25,0.5,out _));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonFiniteInput_Rejected(double value)
    {
        Assert.False(BSplineNormalParameterPolicy.TryEvaluate(value,
            new[] {0.0,0.0,1.0,1.0},1,2,new[] {0.0,1.0},
            new[] {2,2},out _));
    }
}
