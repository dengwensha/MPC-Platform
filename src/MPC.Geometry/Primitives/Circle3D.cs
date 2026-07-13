namespace MPC.Geometry.Primitives;

public sealed record Circle3D
{
    private const double UnitVectorTolerance = 1e-12;

    public Point3D Center { get; }

    public Vector3D Normal { get; }

    public double Radius { get; }

    public Circle3D(
        Point3D center,
        Vector3D normal,
        double radius)
    {
        ValidateFinitePoint(center);
        ValidateFiniteVector(normal);
        ValidateUnitVector(normal);
        ValidateRadius(radius);

        Center = center;
        Normal = normal;
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
                "Circle center coordinates must be finite.");
        }
    }

    private static void ValidateFiniteVector(Vector3D vector)
    {
        if (!double.IsFinite(vector.X) ||
            !double.IsFinite(vector.Y) ||
            !double.IsFinite(vector.Z))
        {
            throw new ArgumentOutOfRangeException(
                nameof(vector),
                vector,
                "Circle normal coordinates must be finite.");
        }
    }

    private static void ValidateUnitVector(Vector3D normal)
    {
        var lengthSquared =
            normal.X * normal.X +
            normal.Y * normal.Y +
            normal.Z * normal.Z;

        if (Math.Abs(lengthSquared - 1.0) > UnitVectorTolerance)
        {
            throw new ArgumentException(
                "Circle normal must be a unit vector within tolerance.",
                nameof(normal));
        }
    }

    private static void ValidateRadius(double radius)
    {
        if (!double.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Circle radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Circle radius must be greater than zero.");
        }
    }
}