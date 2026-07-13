namespace MPC.Geometry.Primitives;

using MPC.Geometry.Contracts;

public sealed record Cone3D : ISurface3D
{
    private const double UnitVectorTolerance = 1e-12;

    public Point3D Apex { get; }

    public Vector3D AxisDirection { get; }

    public double SemiAngle { get; }

    public Cone3D(
        Point3D apex,
        Vector3D axisDirection,
        double semiAngle)
    {
        ValidateFinitePoint(apex);
        ValidateFiniteVector(axisDirection);
        ValidateUnitVector(axisDirection);
        ValidateSemiAngle(semiAngle);

        Apex = apex;
        AxisDirection = axisDirection;
        SemiAngle = semiAngle;
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
                "Cone apex coordinates must be finite.");
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
                "Cone axis direction coordinates must be finite.");
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
                "Cone axis direction must be a unit vector within tolerance.",
                nameof(direction));
        }
    }

    private static void ValidateSemiAngle(double semiAngle)
    {
        if (!double.IsFinite(semiAngle))
        {
            throw new ArgumentOutOfRangeException(
                nameof(semiAngle),
                semiAngle,
                "Cone semi-angle must be finite.");
        }

        if (semiAngle <= 0.0 || semiAngle >= Math.PI / 2.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(semiAngle),
                semiAngle,
                "Cone semi-angle must be greater than zero and less than PI / 2.");
        }
    }
}