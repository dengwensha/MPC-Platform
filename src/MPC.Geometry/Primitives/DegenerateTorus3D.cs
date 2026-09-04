namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed record DegenerateTorus3D : ISurface3D
{
    private const double UnitVectorTolerance = 1e-12;

    public Point3D Center { get; }

    public Vector3D AxisDirection { get; }

    public double MajorRadius { get; }

    public double MinorRadius { get; }

    public bool SelectOuter { get; }

    public DegenerateTorus3D(
        Point3D center,
        Vector3D axisDirection,
        double majorRadius,
        double minorRadius,
        bool selectOuter)
    {
        ValidateFinitePoint(center);
        ValidateFiniteVector(axisDirection);
        ValidateUnitVector(axisDirection);

        ValidateRadius(
            majorRadius,
            nameof(majorRadius));

        ValidateRadius(
            minorRadius,
            nameof(minorRadius));

        if (majorRadius >= minorRadius)
        {
            throw new ArgumentException(
                "Degenerate torus major radius must be less than minor radius.",
                nameof(majorRadius));
        }

        Center = center;
        AxisDirection = axisDirection;
        MajorRadius = majorRadius;
        MinorRadius = minorRadius;
        SelectOuter = selectOuter;
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
                "Degenerate torus center coordinates must be finite.");
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
                "Degenerate torus axis direction coordinates must be finite.");
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
                "Degenerate torus axis direction must be a unit vector within tolerance.",
                nameof(direction));
        }
    }

    private static void ValidateRadius(
        double radius,
        string parameterName)
    {
        if (!double.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                radius,
                "Degenerate torus radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                radius,
                "Degenerate torus radius must be greater than zero.");
        }
    }
}
