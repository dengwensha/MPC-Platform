using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class RationalBSplineNormalQueryTests
{
    private static BSplineSurface3D Patch() => new(
        1,1,
        new[]
        {
            new[] { new Point3D(0,0,0),new Point3D(0,1,0) },
            new[] { new Point3D(1,0,0),new Point3D(1,1,1) }
        },
        new[] {2,2},new[] {2,2},
        new[] {0.0,1.0},new[] {0.0,1.0});

    [Fact]
    public void EqualWeights_MatchNonRationalE2()
    {
        var basis=Patch();
        var rational=new RationalBSplineSurface3D(basis,
            new[] {new[] {3.0,3.0},new[] {3.0,3.0}});
        Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(basis,0.3,0.7,out var expected));
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(rational,0.3,0.7,out var actual));
        Assert.Equal(expected.X,actual.X,10);
        Assert.Equal(expected.Y,actual.Y,10);
        Assert.Equal(expected.Z,actual.Z,10);
    }

    [Fact]
    public void UnequalWeights_ChangeTwistedPatchNormal()
    {
        var basis=Patch();
        var rational=new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0},new[] {1.0,4.0}});
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(rational,.5,.5,out var actual));
        Assert.True(BSplineSurfaceNormalQuery.TryEvaluate(basis,.5,.5,out var unweighted));
        Assert.True(Math.Abs(actual.X-unweighted.X)>1e-4 ||
                    Math.Abs(actual.Y-unweighted.Y)>1e-4 ||
                    Math.Abs(actual.Z-unweighted.Z)>1e-4);
    }

    [Fact]
    public void OutsideParameterDomain_IsRejected()
    {
        var rational=new RationalBSplineSurface3D(Patch(),
            new[] {new[] {1.0,1.0},new[] {1.0,1.0}});
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(rational,-.1,.5,out _));
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(rational,double.NaN,.5,out _));
    }

    [Fact]
    public void SingularPatch_IsRejected()
    {
        var basis=new BSplineSurface3D(1,1,
            new[]
            {
                new[] {new Point3D(0,0,0),new Point3D(0,0,0)},
                new[] {new Point3D(1,0,0),new Point3D(1,0,0)}
            },new[] {2,2},new[] {2,2},
            new[] {0.0,1.0},new[] {0.0,1.0});
        var surface=new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0},new[] {1.0,1.0}});
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface,.5,.5,out _));
    }

    [Fact]
    public void RoundedPoleBoundary_WithPositiveWeights_IsRejected()
    {
        var basis = new BSplineSurface3D(1, 1,
            new[]
            {
                new[] { new Point3D(0,0,0), new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0), new Point3D(1e-12,1,0) }
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] { new[] {1.0,2.0}, new[] {3.0,4.0} });
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface, 0.37, 1.0, out _));
    }

    [Fact]
    public void NearPoleOneUlpInsideBoundary_RejectsTinyDerivative()
    {
        var basis = new BSplineSurface3D(1, 1,
            new[]
            {
                new[] { new Point3D(0,0,0), new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0), new Point3D(1e-12,1,0) }
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] { new[] {1.0,1.0}, new[] {1.0,1.0} });
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(
            surface, 0.37, Math.BitDecrement(1.0), out _));
    }

    [Theory]
    [InlineData(1e-16)]
    [InlineData(1e-14)]
    [InlineData(1e-12)]
    [InlineData(1e-10)]
    public void PositiveEqualWeights_NoisyPoleBoundary_IsRejected(double noise)
    {
        var basis = new BSplineSurface3D(1, 1,
            new[]
            {
                new[] {new Point3D(0,0,0), new Point3D(0,1,0)},
                new[] {new Point3D(1,0,0), new Point3D(noise,1,0)}
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0}, new[] {1.0,1.0}});
        foreach (double u in new[] {0.37,0.61})
            Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface,u,1.0,out _));
    }

    [Fact]
    public void PositiveEqualWeights_OneToHundredUlps_RejectsUnreliableNormal()
    {
        var basis = new BSplineSurface3D(1, 1,
            new[]
            {
                new[] {new Point3D(0,0,0), new Point3D(0,1,0)},
                new[] {new Point3D(1,0,0), new Point3D(1e-12,1,0)}
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0}, new[] {1.0,1.0}});
        foreach (int ulps in new[] {1,2,10,100})
        {
            double v = 1.0;
            for (int i=0;i<ulps;i++) v=Math.BitDecrement(v);
            Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface,0.37,v,out _));
        }
    }
}
