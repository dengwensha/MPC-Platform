using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Conservative witness point for an existing canonical edge.
/// Line: chord midpoint after validation. Circle: verified start vertex only;
/// arc interior is not inferred without trimming parameters.</summary>
public static class CommonEdgeSampleQuery
{
    public static bool TrySample(Edge edge, out Point3D point)
    {
        ArgumentNullException.ThrowIfNull(edge);
        point = default;
        Point3D a = edge.StartVertex.Position;
        Point3D b = edge.EndVertex.Position;
        if (!Finite(a) || !Finite(b)) return false;

        if (edge.Curve is Line3D line)
        {
            if (!OnLine(line, a) || !OnLine(line, b)) return false;
            point = new Point3D(
                a.X + (b.X - a.X) / 2,
                a.Y + (b.Y - a.Y) / 2,
                a.Z + (b.Z - a.Z) / 2);
            return Finite(point);
        }

        if (edge.Curve is Circle3D circle)
        {
            if (!OnCircle(circle, a) || !OnCircle(circle, b)) return false;
            point = a; // Endpoint belongs to the trimmed arc; midpoint may not.
            return true;
        }

        return false;
    }

    private static bool Finite(Point3D p) =>
        double.IsFinite(p.X) && double.IsFinite(p.Y) && double.IsFinite(p.Z);

    private static bool OnLine(Line3D line, Point3D p)
    {
        double x = p.X - line.Origin.X, y = p.Y - line.Origin.Y,
            z = p.Z - line.Origin.Z;
        double dx = line.Direction.X, dy = line.Direction.Y, dz = line.Direction.Z;
        double t = x * dx + y * dy + z * dz;
        double rx = x - t * dx, ry = y - t * dy, rz = z - t * dz;
        double distance = Math.Sqrt(rx * rx + ry * ry + rz * rz);
        return double.IsFinite(distance) &&
            distance <= 1e-8 * Math.Max(1, Math.Sqrt(x*x+y*y+z*z));
    }

    private static bool OnCircle(Circle3D circle, Point3D p)
    {
        double x = p.X - circle.Center.X, y = p.Y - circle.Center.Y,
            z = p.Z - circle.Center.Z;
        double plane = x * circle.Normal.X + y * circle.Normal.Y +
            z * circle.Normal.Z;
        double radius = Math.Sqrt(x*x + y*y + z*z);
        double tolerance = 1e-8 * Math.Max(1, circle.Radius);
        return double.IsFinite(radius) && Math.Abs(plane) <= tolerance &&
            Math.Abs(radius - circle.Radius) <= tolerance;
    }
}
