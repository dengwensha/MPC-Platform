using MPC.Geometry.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Analytic unit normal on the positive-axis cone nappe.
/// Not a face-oriented normal or a concavity classification.</summary>
public static class ConeLocalNormalQuery
{
    public static bool TryEvaluate(Cone3D cone, Point3D point, out Vector3D normal)
    {
        ArgumentNullException.ThrowIfNull(cone);
        normal = default;
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) ||
            !double.IsFinite(point.Z)) return false;

        Vector3D axis = cone.AxisDirection;
        double dx = point.X - cone.Apex.X;
        double dy = point.Y - cone.Apex.Y;
        double dz = point.Z - cone.Apex.Z;
        double h = dx * axis.X + dy * axis.Y + dz * axis.Z;
        if (!double.IsFinite(h) || h <= 0.0) return false;

        double rx = dx - h * axis.X;
        double ry = dy - h * axis.Y;
        double rz = dz - h * axis.Z;
        double radius = Math.Sqrt(rx * rx + ry * ry + rz * rz);
        if (!double.IsFinite(radius) || radius <= 0.0) return false;

        double expectedRadius = h * Math.Tan(cone.SemiAngle);
        if (!double.IsFinite(expectedRadius) ||
            Math.Abs(radius - expectedRadius) >
                1e-8 * Math.Max(1.0, expectedRadius)) return false;

        double cosine = Math.Cos(cone.SemiAngle);
        double sine = Math.Sin(cone.SemiAngle);
        normal = new Vector3D(
            cosine * rx / radius - sine * axis.X,
            cosine * ry / radius - sine * axis.Y,
            cosine * rz / radius - sine * axis.Z);
        return true;
    }
}
