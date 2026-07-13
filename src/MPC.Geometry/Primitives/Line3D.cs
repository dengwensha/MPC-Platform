namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed record Line3D : ICurve3D
{
    private const double UnitVectorTolerance = 1e-12;

    public Point3D Origin { get; }

    public Vector3D Direction { get; }

    public Line3D(
        Point3D origin,
        Vector3D direction)
    {
        ValidateFinitePoint(origin);
        ValidateFiniteVector(direction);
        ValidateUnitVector(direction);

        Origin = origin;
        Direction = direction;
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
                "Line origin coordinates must be finite.");
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
                "Line direction coordinates must be finite.");
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
                "Line direction must be a unit vector within tolerance.",
                nameof(direction));
        }
    }
}