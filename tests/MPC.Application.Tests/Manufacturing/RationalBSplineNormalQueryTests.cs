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

    [Theory]
    [InlineData(1e-16)]
    [InlineData(1e-12)]
    [InlineData(1e-9)]
    [InlineData(1e-8)]
    [InlineData(1e-7)]
    [InlineData(1e-6)]
    public void Ge03B01_ExpandedNoisyPoleSweep_IsRejected(double noise)
    {
        var basis = new BSplineSurface3D(1, 1,
            new[]
            {
                new[] { new Point3D(0,0,0), new Point3D(0,1,0) },
                new[] { new Point3D(1,0,0), new Point3D(noise,1,0) }
            }, new[] {2,2}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] { new[] {1.0,2.0}, new[] {3.0,4.0} });
        foreach (double u in new[] { 0.17, 0.37, 0.61, 0.83 })
            Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface, u, 1.0, out _));
    }

    [Fact]
    public void EqualWeightThinRegularBoundary_IsConservativelyRejected_InteriorRemainsValid()
    {
        var basis = new BSplineSurface3D(1,1,
            new[]
            {
                new[] {new Point3D(0,0,0),new Point3D(0,1,0)},
                new[] {new Point3D(1,0,0),new Point3D(5e-7,1,0)}
            },new[] {2,2},new[] {2,2},
            new[] {0.0,1.0},new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0},new[] {1.0,1.0}});
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(surface,0.37,0.5,out var interior));
        Assert.Equal(1.0,interior.Z,10);
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface,0.37,1.0,out _));
    }

    // GE03B-02: independently derived outward quarter-cone normal remains
    // constant along a fixed angular parameter as V approaches the apex.
    // Rejection is acceptable only when the existing fail-closed policy applies;
    // no inaccurate accepted normal is permitted.
    [Theory]
    [InlineData(1e-4)]
    [InlineData(1e-6)]
    [InlineData(1e-8)]
    [InlineData(1e-10)]
    [InlineData(1e-12)]
    public void QuarterConeNearApex_AcceptedNormalsMatchAnalyticDirection(double distance)
    {
        double w = Math.Sqrt(0.5);
        var basis = new BSplineSurface3D(2,1,
            new[]
            {
                new[] { new Point3D(1,0,0), new Point3D(0,0,1) },
                new[] { new Point3D(1,1,0), new Point3D(0,0,1) },
                new[] { new Point3D(0,1,0), new Point3D(0,0,1) }
            },
            new[] {3,3}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0}, new[] {w,w}, new[] {1.0,1.0}});
        int accepted = 0;
        foreach (double u in new[] { 0.17, 0.5, 0.83 })
        {
            if (!RationalBSplineNormalQuery.TryEvaluate(surface,u,1.0-distance,out var n))
                continue;
            accepted++;
            // Analytic cone: n=(cos(theta),sin(theta),1)/sqrt(2).
            // Independent rational quadratic circle position at u:
            double a = (1-u)*(1-u), b = 2*w*u*(1-u), c = u*u;
            double denom = a+b+c;
            double cx = (a+b)/denom, cy = (b+c)/denom;
            double radius = Math.Sqrt(cx*cx+cy*cy);
            double ex = cx/(Math.Sqrt(2)*radius);
            double ey = cy/(Math.Sqrt(2)*radius);
            double ez = Math.Sqrt(0.5);
            double dot = Math.Clamp(n.X*ex+n.Y*ey+n.Z*ez,-1.0,1.0);
            Assert.True(dot >= 1.0-1e-7,
                $"Near-apex accepted normal disagreed with analytic cone at u={u}, distance={distance}; dot={dot}.");
        }
        if (distance == 1e-4) Assert.Equal(3, accepted);
    }

    // GE03B-01: exact quarter-cone from a rational quadratic quarter-circle
    // and a linear interpolation to its apex. The midpoint reference normal
    // is analytic and does not use the implementation's derivative evaluator.
    [Theory]
    [InlineData(0.0)]
    [InlineData(1e-12)]
    [InlineData(1e-8)]
    [InlineData(1e-6)]
    public void ExactQuarterConeApex_WithExportNoise_IsRejected(double noise)
    {
        double w = Math.Sqrt(0.5);
        var basis = new BSplineSurface3D(2,1,
            new[]
            {
                new[] {new Point3D(1,0,0), new Point3D(0,0,1)},
                new[] {new Point3D(1,1,0), new Point3D(noise,0,1)},
                new[] {new Point3D(0,1,0), new Point3D(0,noise,1)}
            },
            new[] {3,3}, new[] {2,2},
            new[] {0.0,1.0}, new[] {0.0,1.0});
        var surface = new RationalBSplineSurface3D(basis,
            new[] {new[] {1.0,1.0}, new[] {w,w}, new[] {1.0,1.0}});
        foreach (double u in new[] {0.17,0.5,0.83})
            Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface,u,1.0,out _));
        // Only the exact cone (no export perturbation) has a known normal.
        if (noise != 0.0) return;
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(surface,0.5,0.5,out var normal));
        Assert.Equal(0.5,normal.X,7);
        Assert.Equal(0.5,normal.Y,7);
        Assert.Equal(Math.Sqrt(0.5),normal.Z,7);
    }
}
