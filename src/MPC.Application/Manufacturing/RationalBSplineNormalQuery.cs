using MPC.Geometry.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Parametric NURBS normal at a regular (u,v) using homogeneous derivatives.
/// Does not infer UV from XYZ, face trimming, or boundary-wide continuity.</summary>
public static class RationalBSplineNormalQuery
{
    public static bool TryEvaluate(RationalBSplineSurface3D surface, double u, double v,
        out Vector3D normal)
    {
        ArgumentNullException.ThrowIfNull(surface);
        normal = default;
        if (!double.IsFinite(u) || !double.IsFinite(v)) return false;
        BSplineSurface3D s = surface.Basis;
        if (RegularSurfaceNormal.IsCollapsedBoundary(s, u, v)) return false;
        var uk = Expand(s.UKnots, s.UMultiplicities);
        var vk = Expand(s.VKnots, s.VMultiplicities);
        int nu = s.ControlPointGrid.Count, nv = s.ControlPointGrid[0].Count;
        if (!BSplineNormalParameterPolicy.TryEvaluate(u, uk, s.UDegree, nu, s.UKnots, s.UMultiplicities, out double su) ||
            !BSplineNormalParameterPolicy.TryEvaluate(v, vk, s.VDegree, nv, s.VKnots, s.VMultiplicities, out double sv)) return false;

        // Normalise homogeneous weights before accumulation: scaling all weights
        // by a common positive factor must not change the result or its acceptance.
        double weightScale = surface.Weights.SelectMany(row => row).Max();
        if (!double.IsFinite(weightScale) || weightScale <= 0) return false;
        double ax = 0, ay = 0, az = 0, w = 0;
        double aux = 0, auy = 0, auz = 0, wu = 0;
        double avx = 0, avy = 0, avz = 0, wv = 0;
        for (int i = 0; i < nu; i++)
        {
            double bu = Basis(i,s.UDegree,su,uk), du = Derivative(i,s.UDegree,su,uk);
            for (int j = 0; j < nv; j++)
            {
                double bv = Basis(j,s.VDegree,sv,vk), dv = Derivative(j,s.VDegree,sv,vk);
                double weight = surface.Weights[i][j] / weightScale;
                double b = bu*bv*weight, dU = du*bv*weight, dV = bu*dv*weight;
                Point3D p = s.ControlPointGrid[i][j];
                ax += b*p.X; ay += b*p.Y; az += b*p.Z; w += b;
                aux += dU*p.X; auy += dU*p.Y; auz += dU*p.Z; wu += dU;
                avx += dV*p.X; avy += dV*p.Y; avz += dV*p.Z; wv += dV;
            }
        }
        if (!double.IsFinite(w) || w <= 0) return false;
        // Quotient-rule derivatives; using W rather than W² prevents an
        // artificial weight-scale threshold in the normal cross product.
        double px = ax / w, py = ay / w, pz = az / w;
        double ux = (aux - px * wu) / w;
        double uy = (auy - py * wu) / w;
        double uz = (auz - pz * wu) / w;
        double vx = (avx - px * wv) / w;
        double vy = (avy - py * wv) / w;
        double vz = (avz - pz * wv) / w;
        return RegularSurfaceNormal.TryCreate(ux, uy, uz, vx, vy, vz, out normal);
    }

    private static double[] Expand(IReadOnlyList<double> knots,IReadOnlyList<int> mult)
    {
        var list=new List<double>();
        for(int i=0;i<knots.Count;i++)
            for(int j=0;j<mult[i];j++) list.Add(knots[i]);
        return list.ToArray();
    }

    private static bool Coordinate(double value,double[] knots,int degree,int count,
        IReadOnlyList<double> unique,out double sample)
    {
        sample=value;
        double min=knots[degree],max=knots[count];
        if(value<min||value>max||min>=max) return false;
        if(unique.Skip(1).SkipLast(1).Contains(value)) return false;
        if(value==max) sample=Math.BitDecrement(max);
        return sample>=min&&sample<max;
    }

    private static double Basis(int i,int degree,double t,double[] knots)
    {
        if(degree==0) return knots[i]<=t&&t<knots[i+1]?1:0;
        double a=knots[i+degree]-knots[i];
        double b=knots[i+degree+1]-knots[i+1];
        return (a>0?(t-knots[i])/a*Basis(i,degree-1,t,knots):0)
            +(b>0?(knots[i+degree+1]-t)/b*Basis(i+1,degree-1,t,knots):0);
    }

    private static double Derivative(int i,int degree,double t,double[] knots)
    {
        double a=knots[i+degree]-knots[i];
        double b=knots[i+degree+1]-knots[i+1];
        return (a>0?degree/a*Basis(i,degree-1,t,knots):0)
            -(b>0?degree/b*Basis(i+1,degree-1,t,knots):0);
    }
}
