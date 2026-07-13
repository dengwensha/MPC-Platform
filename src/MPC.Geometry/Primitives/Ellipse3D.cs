namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed record Ellipse3D : ICurve3D
{
    private const double UnitVectorTolerance = 1e-12;
    private const double OrthogonalityTolerance = 1e-12;

    public Point3D Center { get; }

    public Vector3D Normal { get; }

    public Vector3D MajorAxisDirection { get; }

    public double MajorRadius { get; }

    public double MinorRadius { get; }

    public Ellipse3D(
        Point3D center,
        Vector3D normal,
        Vector3D majorAxisDirection,
        double majorRadius,
        double minorRadius)
    {
        ValidateFinitePoint(center);

        ValidateFiniteVector(
            normal,
            nameof(normal),
            "Ellipse normal coordinates must be finite.");

        ValidateFiniteVector(
            majorAxisDirection,
            nameof(majorAxisDirection),
            "Ellipse major-axis direction coordinates must be finite.");

        ValidateUnitVector(normal, nameof(normal));
        ValidateUnitVector(
            majorAxisDirection,
            nameof(majorAxisDirection));

        ValidateOrthogonal(normal, majorAxisDirection);

        ValidateRadius(majorRadius, nameof(majorRadius));
        ValidateRadius(minorRadius, nameof(minorRadius));

        if (majorRadius <= minorRadius)
        {
            throw new ArgumentException(
                "Ellipse major radius must be greater than minor radius.",
                nameof(majorRadius));
        }

        Center = center;
        Normal = normal;
        MajorAxisDirection = majorAxisDirection;
        MajorRadius = majorRadius;
        MinorRadius = minorRadius;
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
                "Ellipse center coordinates must be finite.");
        }
    }

    private static void ValidateFiniteVector(
        Vector3D vector,
        string parameterName,
        string message)
    {
        if (!double.IsFinite(vector.X) ||
            !double.IsFinite(vector.Y) ||
            !double.IsFinite(vector.Z))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                vector,
                message);
        }
    }

    private static void ValidateUnitVector(
        Vector3D vector,
        string parameterName)
    {
        var lengthSquared =
            vector.X * vector.X +
            vector.Y * vector.Y +
            vector.Z * vector.Z;

        if (Math.Abs(lengthSquared - 1.0) > UnitVectorTolerance)
        {
            throw new ArgumentException(
                "Ellipse direction vectors must be unit vectors within tolerance.",
                parameterName);
        }
    }

    private static void ValidateOrthogonal(
        Vector3D normal,
        Vector3D majorAxisDirection)
    {
        var dotProduct =
            normal.X * majorAxisDirection.X +
            normal.Y * majorAxisDirection.Y +
            normal.Z * majorAxisDirection.Z;

        if (Math.Abs(dotProduct) > OrthogonalityTolerance)
        {
            throw new ArgumentException(
                "Ellipse normal and major-axis direction must be orthogonal within tolerance.",
                nameof(majorAxisDirection));
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
                "Ellipse radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                radius,
                "Ellipse radius must be greater than zero.");
        }
    }
}