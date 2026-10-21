using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Application.Tests.Manufacturing;

/// <summary>
/// Independent numerical position oracle for a rational degree-(1,1) patch.
/// The oracle evaluates rational bilinear positions directly, without invoking
/// production basis derivatives or normal formulas.
/// </summary>
public sealed class RationalSurfaceFiniteDifferenceOracleTests
{
    [Theory]
    [InlineData(0.13, 0.37, 1.0)]
    [InlineData(0.37, 0.61, 0.0001)]
    [InlineData(0.61, 0.89, 1000000.0)]
    [InlineData(0.89, 0.13, 100.0)]
    public void RationalNormal_AgreesWithIndependentPositionFiniteDifferences(
        double u, double v, double commonWeightScale)
    {
        var grid = new[]
        {
            new[] { new Point3D(0,0,0), new Point3D(0,2,0) },
            new[] { new Point3D(3,0,0), new Point3D(3,2,1) }
        };
        var basis = new BSplineSurface3D(1,1,grid,
            new[] {2,2},new[] {2,2},
            new[] {0.0,1.0},new[] {0.0,1.0});
        var weights = new[]
        {
            new[] {1.0*commonWeightScale,2.0*commonWeightScale},
            new[] {3.0*commonWeightScale,4.0*commonWeightScale}
        };
        var surface = new RationalBSplineSurface3D(basis,weights);
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(surface,u,v,out var actual));

        const double h = 0.000001;
        double[] uPlus=Point(u+h,v),uMinus=Point(u-h,v);
        double[] vPlus=Point(u,v+h),vMinus=Point(u,v-h);
        double[] du=Enumerable.Range(0,3).Select(i=>(uPlus[i]-uMinus[i])/(2*h)).ToArray();
        double[] dv=Enumerable.Range(0,3).Select(i=>(vPlus[i]-vMinus[i])/(2*h)).ToArray();
        double x=du[1]*dv[2]-du[2]*dv[1];
        double y=du[2]*dv[0]-du[0]*dv[2];
        double z=du[0]*dv[1]-du[1]*dv[0];
        double length=Math.Sqrt(x*x+y*y+z*z);
        Assert.True(length>0);
        Assert.InRange(Math.Abs(actual.X-x/length),0,1e-6);
        Assert.InRange(Math.Abs(actual.Y-y/length),0,1e-6);
        Assert.InRange(Math.Abs(actual.Z-z/length),0,1e-6);

        double[] Point(double a,double b)
        {
            double[] bu={1-a,a},bv={1-b,b};
            double denominator=0;
            var numerator=new double[3];
            for(int i=0;i<2;i++)
                for(int j=0;j<2;j++)
                {
                    double w=bu[i]*bv[j]*weights[i][j];
                    denominator+=w;
                    numerator[0]+=w*grid[i][j].X;
                    numerator[1]+=w*grid[i][j].Y;
                    numerator[2]+=w*grid[i][j].Z;
                }
            return numerator.Select(x=>x/denominator).ToArray();
        }
    }
}
