using MPC.Geometry.Primitives;

// Independent position-only oracle for a real rational STEP surface.
// This evaluator intentionally does not call MPC's analytic normal derivatives.
internal static class RationalPositionFiniteDifferenceOracle
{
    internal static bool TryCompare(RationalBSplineSurface3D surface,
        double u, double v, Vector3D normal, out double angularErrorDegrees)
    {
        angularErrorDegrees = double.NaN;
        var basis = surface.Basis;
        double? hu = Step(basis.UKnots, u);
        double? hv = Step(basis.VKnots, v);
        if (hu is null || hv is null) return false;
        if (!TryPosition(surface,u+hu.Value,v,out var up) ||
            !TryPosition(surface,u-hu.Value,v,out var um) ||
            !TryPosition(surface,u,v+hv.Value,out var vp) ||
            !TryPosition(surface,u,v-hv.Value,out var vm)) return false;
        double ux=(up.X-um.X)/(2*hu.Value),uy=(up.Y-um.Y)/(2*hu.Value),uz=(up.Z-um.Z)/(2*hu.Value);
        double vx=(vp.X-vm.X)/(2*hv.Value),vy=(vp.Y-vm.Y)/(2*hv.Value),vz=(vp.Z-vm.Z)/(2*hv.Value);
        double x=uy*vz-uz*vy,y=uz*vx-ux*vz,z=ux*vy-uy*vx;
        double length=Math.Sqrt(x*x+y*y+z*z);
        if (!(length>0) || !double.IsFinite(length)) return false;
        double dot=Math.Clamp((x*normal.X+y*normal.Y+z*normal.Z)/length,-1,1);
        angularErrorDegrees=Math.Acos(dot)*180/Math.PI;
        return double.IsFinite(angularErrorDegrees);
    }

    private static double? Step(IReadOnlyList<double> knots,double t)
    {
        for(int i=0;i+1<knots.Count;i++)
            if(knots[i]<t && t<knots[i+1])
            {
                double span=knots[i+1]-knots[i];
                double h=Math.Min(span*0.00001,Math.Min(t-knots[i],knots[i+1]-t)*0.1);
                return h>0 && double.IsFinite(h) ? h : null;
            }
        return null;
    }

    private static bool TryPosition(RationalBSplineSurface3D surface,double u,double v,out Point3D point)
    {
        point=default;
        var b=surface.Basis;
        double[] ku=Expand(b.UKnots,b.UMultiplicities),kv=Expand(b.VKnots,b.VMultiplicities);
        double x=0,y=0,z=0,w=0;
        for(int i=0;i<b.ControlPointGrid.Count;i++)
        {
            double bu=Basis(i,b.UDegree,u,ku);
            for(int j=0;j<b.ControlPointGrid[i].Count;j++)
            {
                double coeff=bu*Basis(j,b.VDegree,v,kv)*surface.Weights[i][j];
                var p=b.ControlPointGrid[i][j];
                x+=coeff*p.X;y+=coeff*p.Y;z+=coeff*p.Z;w+=coeff;
            }
        }
        if(!(w>0) || !double.IsFinite(w))return false;
        point=new Point3D(x/w,y/w,z/w);
        return double.IsFinite(point.X)&&double.IsFinite(point.Y)&&double.IsFinite(point.Z);
    }

    private static double[] Expand(IReadOnlyList<double> knots,IReadOnlyList<int> mult)
    {
        var result=new List<double>();
        for(int i=0;i<knots.Count;i++)
            for(int j=0;j<mult[i];j++)result.Add(knots[i]);
        return result.ToArray();
    }

    private static double Basis(int i,int degree,double t,double[] knots)
    {
        if(degree==0)return knots[i]<=t&&t<knots[i+1]?1:0;
        double left=knots[i+degree]-knots[i],right=knots[i+degree+1]-knots[i+1];
        return (left>0?(t-knots[i])/left*Basis(i,degree-1,t,knots):0)+
            (right>0?(knots[i+degree+1]-t)/right*Basis(i+1,degree-1,t,knots):0);
    }
}
