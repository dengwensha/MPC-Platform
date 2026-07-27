using MPC.Geometry.Contracts;

namespace MPC.Geometry.Primitives;

public sealed record Plane3D : ISurface3D
{
    private const double UnitVectorTolerance = 1e-12;

    public Point3D Origin { get; }

    public Vector3D Normal { get; }

    public Plane3D(
        Point3D origin,
        Vector3D normal)
    {
        ValidateFinitePoint(
            origin);

        ValidateFiniteVector(
            normal);

        ValidateUnitVector(
            normal);

        Origin = origin;
        Normal = normal;
    }

    private static void ValidateFinitePoint(
        Point3D point)
    {
        if (!double.IsFinite(point.X) ||
            !double.IsFinite(point.Y) ||
            !double.IsFinite(point.Z))
        {
            throw new ArgumentOutOfRangeException(
                nameof(point),
                point,
                "Plane origin coordinates must be finite.");
        }
    }

    private static void ValidateFiniteVector(
        Vector3D vector)
    {
        if (!double.IsFinite(vector.X) ||
            !double.IsFinite(vector.Y) ||
            !double.IsFinite(vector.Z))
        {
            throw new ArgumentOutOfRangeException(
                nameof(vector),
                vector,
                "Plane normal coordinates must be finite.");
        }
    }

    private static void ValidateUnitVector(
        Vector3D normal)
    {
        double lengthSquared =
            normal.X * normal.X +
            normal.Y * normal.Y +
            normal.Z * normal.Z;

        if (Math.Abs(
                lengthSquared - 1.0) >
            UnitVectorTolerance)
        {
            throw new ArgumentException(
                "Plane normal must be a unit vector within tolerance.",
                nameof(normal));
        }
    }
}