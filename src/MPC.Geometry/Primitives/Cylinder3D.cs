namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed record Cylinder3D : ISurface3D
{
    private const double UnitVectorTolerance = 1e-12;

    public Point3D AxisOrigin { get; }

    public Vector3D AxisDirection { get; }

    public double Radius { get; }

    public Cylinder3D(
        Point3D axisOrigin,
        Vector3D axisDirection,
        double radius)
    {
        ValidateFinitePoint(axisOrigin);

        ValidateFiniteVector(axisDirection);

        ValidateUnitVector(axisDirection);

        ValidateRadius(radius);

        AxisOrigin = axisOrigin;
        AxisDirection = axisDirection;
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
                "Cylinder axis origin coordinates must be finite.");
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
                "Cylinder axis direction coordinates must be finite.");
        }
    }

    private static void ValidateUnitVector(Vector3D direction)
    {
        var lengthSquared =
            direction.X * direction.X +
            direction.Y * direction.Y +
            direction.Z * direction.Z;

        if (Math.Abs(lengthSquared - 1.0) > UnitVectorTolerance)
        {
            throw new ArgumentException(
                "Cylinder axis direction must be a unit vector within tolerance.",
                nameof(direction));
        }
    }

    private static void ValidateRadius(double radius)
    {
        if (!double.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Cylinder radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Cylinder radius must be greater than zero.");
        }
    }
}