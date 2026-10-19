using MPC.Geometry.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Evaluates the outward radial normal on an analytic cylinder.
/// Does not apply FaceOrientation or classify material-side transitions.</summary>
public static class CylinderLocalNormalQuery
{
    public static bool TryEvaluate(
        Cylinder3D cylinder,
        Point3D point,
        out Vector3D normal)
    {
        ArgumentNullException.ThrowIfNull(cylinder);
        normal = default;
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) ||
            !double.IsFinite(point.Z))
            return false;

        Vector3D axis = cylinder.AxisDirection;
        double dx = point.X - cylinder.AxisOrigin.X;
        double dy = point.Y - cylinder.AxisOrigin.Y;
        double dz = point.Z - cylinder.AxisOrigin.Z;
        double axial = dx * axis.X + dy * axis.Y + dz * axis.Z;
        double rx = dx - axial * axis.X;
        double ry = dy - axial * axis.Y;
        double rz = dz - axial * axis.Z;
        double length = Math.Sqrt(rx * rx + ry * ry + rz * rz);
        if (!double.IsFinite(length) || length <= 0)
            return false;
        double radiusError = Math.Abs(length - cylinder.Radius);
        if (radiusError > 1e-8 * Math.Max(1.0, cylinder.Radius))
            return false;
        normal = new Vector3D(rx / length, ry / length, rz / length);
        return true;
    }
}
