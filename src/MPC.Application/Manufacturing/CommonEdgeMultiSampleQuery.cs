using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>
/// Three validated interior witness points of a non-degenerate straight edge.
/// Circular edges are deliberately not sampled without certified trim/sweep data.
/// Points are evidence at samples, not a full-edge continuity certificate.
/// </summary>
public static class CommonEdgeMultiSampleQuery
{
    public static bool TrySample(Edge edge, out IReadOnlyList<Point3D> samples)
    {
        ArgumentNullException.ThrowIfNull(edge);
        samples = Array.Empty<Point3D>();
        if (edge.Curve is not Line3D ||
            !CommonEdgeSampleQuery.TrySample(edge, out _))
            return false;

        Point3D a = edge.StartVertex.Position;
        Point3D b = edge.EndVertex.Position;
        double dx = b.X - a.X, dy = b.Y - a.Y, dz = b.Z - a.Z;
        double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (!double.IsFinite(length) || length <= 1e-12)
            return false;

        var result = new Point3D[3];
        for (int i = 0; i < result.Length; i++)
        {
            double t = (i + 1) / 4.0;
            result[i] = new Point3D(a.X + t * dx, a.Y + t * dy, a.Z + t * dz);
            if (!double.IsFinite(result[i].X) ||
                !double.IsFinite(result[i].Y) ||
                !double.IsFinite(result[i].Z))
                return false;
        }
        samples = Array.AsReadOnly(result);
        return true;
    }
}
