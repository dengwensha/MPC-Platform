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
    // In a very narrow parameter-boundary band, a vanishing derivative can
    // retain a seemingly regular direction after independent normalization.
    // This guard only rejects; it never manufactures a normal.
    internal static bool HasUnreliableBoundaryDerivative(
        BSplineSurface3D surface, double u, double v,
        double ux, double uy, double uz, double vx, double vy, double vz)
    {
        const double boundaryBand = 1e-10;
        const double derivativeRatioFloor = 1e-10;
        double uSpan = surface.UKnots[^1] - surface.UKnots[0];
        double vSpan = surface.VKnots[^1] - surface.VKnots[0];
        if (!(uSpan > 0) || !(vSpan > 0) ||
            !double.IsFinite(uSpan) || !double.IsFinite(vSpan)) return true;
        bool nearU = (u - surface.UKnots[0]) / uSpan <= boundaryBand ||
                     (surface.UKnots[^1] - u) / uSpan <= boundaryBand;
        bool nearV = (v - surface.VKnots[0]) / vSpan <= boundaryBand ||
                     (surface.VKnots[^1] - v) / vSpan <= boundaryBand;
        if (!nearU && !nearV) return false;
        double uMagnitude = Math.Max(Math.Abs(ux), Math.Max(Math.Abs(uy), Math.Abs(uz))) * uSpan;
        double vMagnitude = Math.Max(Math.Abs(vx), Math.Max(Math.Abs(vy), Math.Abs(vz))) * vSpan;
        if (!double.IsFinite(uMagnitude) || !double.IsFinite(vMagnitude)) return true;
        double largest = Math.Max(uMagnitude, vMagnitude);
        return !(largest > 0) || Math.Min(uMagnitude, vMagnitude) / largest <= derivativeRatioFloor;
    }

    // Explicitly refuse clamped control-net poles at a parameter endpoint.
    internal static bool IsCollapsedBoundary(BSplineSurface3D surface, double u, double v)
    {
        var grid = surface.ControlPointGrid;
        // Compare the spread of the endpoint row/column with the full
        // control-net span. Exact coordinate equality misses rounded poles.
        double minX = double.PositiveInfinity, minY = double.PositiveInfinity, minZ = double.PositiveInfinity;
        double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity, maxZ = double.NegativeInfinity;
        foreach (var row in grid)
            foreach (Point3D point in row)
            {
                minX = Math.Min(minX, point.X); maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y); maxY = Math.Max(maxY, point.Y);
                minZ = Math.Min(minZ, point.Z); maxZ = Math.Max(maxZ, point.Z);
            }
        double scale = Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ));
        if (!(scale > 0) || !double.IsFinite(scale)) return true;
        const double relativePoleTolerance = 1e-10;
        bool NearSame(Point3D a, Point3D b) =>
            Math.Max(Math.Abs(a.X - b.X),
                Math.Max(Math.Abs(a.Y - b.Y), Math.Abs(a.Z - b.Z))) <=
            relativePoleTolerance * scale;
        if (u == surface.UKnots[0] || u == surface.UKnots[^1])
        {
            int i = u == surface.UKnots[0] ? 0 : grid.Count - 1;
            Point3D first = grid[i][0];
            if (grid[i].All(point => NearSame(point, first))) return true;
        }
        if (v == surface.VKnots[0] || v == surface.VKnots[^1])
        {
            int j = v == surface.VKnots[0] ? 0 : grid[0].Count - 1;
            Point3D first = grid[0][j];
            if (grid.All(row => NearSame(row[j], first))) return true;
        }
        return false;
    }

}
