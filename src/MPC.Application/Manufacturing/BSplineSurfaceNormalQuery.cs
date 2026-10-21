using MPC.Geometry.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Non-rational B-spline parametric normal via analytical basis derivatives.
/// Defined only at regular, unambiguous parameter samples; no trim or XYZ-to-UV inference.</summary>
public static class BSplineSurfaceNormalQuery
{
    public static bool TryEvaluate(BSplineSurface3D surface, double u, double v,
        out Vector3D normal)
    {
        ArgumentNullException.ThrowIfNull(surface);
        normal = default;
        if (!double.IsFinite(u) || !double.IsFinite(v)) return false;
        var uk = Expand(surface.UKnots, surface.UMultiplicities);
        var vk = Expand(surface.VKnots, surface.VMultiplicities);
        int pu = surface.UDegree, pv = surface.VDegree;
        int nu = surface.ControlPointGrid.Count, nv = surface.ControlPointGrid[0].Count;
        if (!BSplineNormalParameterPolicy.TryEvaluate(u, uk, pu, nu, surface.UKnots, surface.UMultiplicities, out double su) ||
            !BSplineNormalParameterPolicy.TryEvaluate(v, vk, pv, nv, surface.VKnots, surface.VMultiplicities, out double sv))
            return false;

        var bu = new double[nu]; var du = new double[nu];
        var bv = new double[nv]; var dv = new double[nv];
        for (int i = 0; i < nu; i++)
        {
            bu[i] = Basis(i, pu, su, uk);
            du[i] = Derivative(i, pu, su, uk);
        }
        for (int j = 0; j < nv; j++)
        {
            bv[j] = Basis(j, pv, sv, vk);
            dv[j] = Derivative(j, pv, sv, vk);
        }

        double ux = 0, uy = 0, uz = 0, vx = 0, vy = 0, vz = 0;
        for (int i = 0; i < nu; i++)
            for (int j = 0; j < nv; j++)
            {
                Point3D p = surface.ControlPointGrid[i][j];
                double wu = du[i] * bv[j], wv = bu[i] * dv[j];
                ux += wu * p.X; uy += wu * p.Y; uz += wu * p.Z;
                vx += wv * p.X; vy += wv * p.Y; vz += wv * p.Z;
            }
        return RegularSurfaceNormal.TryCreate(ux, uy, uz, vx, vy, vz, out normal);
    }

    private static double[] Expand(IReadOnlyList<double> knots, IReadOnlyList<int> mult)
    {
        var result = new List<double>();
        for (int i = 0; i < knots.Count; i++)
            for (int j = 0; j < mult[i]; j++) result.Add(knots[i]);
        return result.ToArray();
    }

    private static bool Coordinate(double value, double[] knots, int degree, int count,
        IReadOnlyList<double> uniqueKnots, out double evaluated)
    {
        evaluated = value;
        double min = knots[degree], max = knots[count];
        if (value < min || value > max || min >= max) return false;
        // At an interior knot a one-sided normal can conceal a tangent discontinuity.
        if (uniqueKnots.Skip(1).SkipLast(1).Contains(value)) return false;
        if (value == max) evaluated = Math.BitDecrement(max);
        return evaluated >= min && evaluated < max;
    }

    private static double Basis(int i, int degree, double t, double[] knots)
    {
        if (degree == 0) return knots[i] <= t && t < knots[i + 1] ? 1 : 0;
        double a = knots[i + degree] - knots[i];
        double b = knots[i + degree + 1] - knots[i + 1];
        return (a > 0 ? (t - knots[i]) / a * Basis(i, degree - 1, t, knots) : 0)
            + (b > 0 ? (knots[i + degree + 1] - t) / b *
                Basis(i + 1, degree - 1, t, knots) : 0);
    }

    private static double Derivative(int i, int degree, double t, double[] knots)
    {
        double a = knots[i + degree] - knots[i];
        double b = knots[i + degree + 1] - knots[i + 1];
        return (a > 0 ? degree / a * Basis(i, degree - 1, t, knots) : 0)
            - (b > 0 ? degree / b * Basis(i + 1, degree - 1, t, knots) : 0);
    }
}
