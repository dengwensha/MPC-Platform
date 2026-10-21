using MPC.Geometry.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Checks derivative independence without an absolute cross-product threshold.</summary>
internal static class RegularSurfaceNormal
{
    internal static bool TryCreate(
        double ux, double uy, double uz, double vx, double vy, double vz,
        out Vector3D normal)
    {
        normal = default;
        double us = Math.Max(Math.Abs(ux), Math.Max(Math.Abs(uy), Math.Abs(uz)));
        double vs = Math.Max(Math.Abs(vx), Math.Max(Math.Abs(vy), Math.Abs(vz)));
        if (!(us > 0) || !(vs > 0) || !double.IsFinite(us) || !double.IsFinite(vs))
            return false;
        ux /= us; uy /= us; uz /= us;
        vx /= vs; vy /= vs; vz /= vs;
        double ul = Math.Sqrt(ux*ux + uy*uy + uz*uz);
        double vl = Math.Sqrt(vx*vx + vy*vy + vz*vz);
        double x = uy*vz - uz*vy, y = uz*vx - ux*vz, z = ux*vy - uy*vx;
        double cross = Math.Sqrt(x*x + y*y + z*z);
        // Relative sine threshold is invariant to coordinate and weight scaling.
        if (!double.IsFinite(cross) || cross <= 1e-10 * ul * vl) return false;
        normal = new Vector3D(x/cross, y/cross, z/cross);
        return double.IsFinite(normal.X) && double.IsFinite(normal.Y) &&
               double.IsFinite(normal.Z);
    }
}
