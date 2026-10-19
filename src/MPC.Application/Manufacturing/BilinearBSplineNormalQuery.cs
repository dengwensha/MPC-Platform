using MPC.Geometry.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>
/// Parametric normal for the minimal non-rational 2x2 degree-(1,1) B-spline patch.
/// Rejects general B-splines, rational surfaces, singular points, and unvalidated
/// extrapolation. This is not a STEP XYZ-to-UV inversion or trimmed-face normal.
/// </summary>
public static class BilinearBSplineNormalQuery
{
    public static bool TryEvaluate(
        BSplineSurface3D surface, double u, double v, out Vector3D normal)
    {
        ArgumentNullException.ThrowIfNull(surface);
        normal = default;
        if (!double.IsFinite(u) || !double.IsFinite(v) ||
            surface.UDegree != 1 || surface.VDegree != 1 ||
            surface.ControlPointGrid.Count != 2 ||
            surface.ControlPointGrid[0].Count != 2 ||
            surface.ControlPointGrid[1].Count != 2 ||
            surface.UKnots.Count != 2 || surface.VKnots.Count != 2 ||
            surface.UMultiplicities.Count != 2 || surface.VMultiplicities.Count != 2 ||
            surface.UMultiplicities[0] != 2 || surface.UMultiplicities[1] != 2 ||
            surface.VMultiplicities[0] != 2 || surface.VMultiplicities[1] != 2)
            return false;

        double u0 = surface.UKnots[0], u1 = surface.UKnots[1];
        double v0 = surface.VKnots[0], v1 = surface.VKnots[1];
        if (u < u0 || u > u1 || v < v0 || v > v1) return false;
        double s = (u - u0) / (u1 - u0);
        double t = (v - v0) / (v1 - v0);
        var p00 = surface.ControlPointGrid[0][0];
        var p01 = surface.ControlPointGrid[0][1];
        var p10 = surface.ControlPointGrid[1][0];
        var p11 = surface.ControlPointGrid[1][1];

        double ux = (1 - t) * (p10.X - p00.X) + t * (p11.X - p01.X);
        double uy = (1 - t) * (p10.Y - p00.Y) + t * (p11.Y - p01.Y);
        double uz = (1 - t) * (p10.Z - p00.Z) + t * (p11.Z - p01.Z);
        double vx = (1 - s) * (p01.X - p00.X) + s * (p11.X - p10.X);
        double vy = (1 - s) * (p01.Y - p00.Y) + s * (p11.Y - p10.Y);
        double vz = (1 - s) * (p01.Z - p00.Z) + s * (p11.Z - p10.Z);
        double nx = uy * vz - uz * vy;
        double ny = uz * vx - ux * vz;
        double nz = ux * vy - uy * vx;
        double magnitude = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (!double.IsFinite(magnitude) || magnitude <= 1e-12) return false;
        normal = new Vector3D(nx / magnitude, ny / magnitude, nz / magnitude);
        return double.IsFinite(normal.X) && double.IsFinite(normal.Y) &&
            double.IsFinite(normal.Z);
    }
}
