namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed record Sphere3D : ISurface3D
{
    public Point3D Center { get; }

    public double Radius { get; }

    public Sphere3D(
        Point3D center,
        double radius)
    {
        ValidateFinitePoint(center);
        ValidateRadius(radius);

        Center = center;
        Radius = radius;
    }

    private static void ValidateFinitePoint(Point3D point)
    {
        if (!double.IsFinite(point.X) ||
            !double.IsFinite(point.Y) ||
            !double.IsFinite(point.Z))
        {
            throw new ArgumentOutOfRangeException(
                nameof(point),
                point,
                "Sphere center coordinates must be finite.");
        }
    }

    private static void ValidateRadius(double radius)
    {
        if (!double.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Sphere radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Sphere radius must be greater than zero.");
        }
    }
}