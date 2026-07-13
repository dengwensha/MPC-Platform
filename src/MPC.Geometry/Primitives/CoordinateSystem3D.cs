namespace MPC.Geometry.Primitives;

public sealed record CoordinateSystem3D
{
    private const double UnitVectorTolerance = 1e-12;
    private const double OrthogonalityTolerance = 1e-12;

    public Point3D Origin { get; }

    public Vector3D XAxis { get; }

    public Vector3D YAxis { get; }

    public Vector3D ZAxis { get; }

    public CoordinateSystem3D(
        Point3D origin,
        Vector3D xAxis,
        Vector3D yAxis)
    {
        ValidateFinitePoint(origin);
        ValidateFiniteVector(xAxis, nameof(xAxis));
        ValidateFiniteVector(yAxis, nameof(yAxis));

        ValidateUnitVector(xAxis, nameof(xAxis));
        ValidateUnitVector(yAxis, nameof(yAxis));

        ValidateOrthogonal(xAxis, yAxis);

        Origin = origin;
        XAxis = xAxis;
        YAxis = yAxis;
        ZAxis = Cross(xAxis, yAxis);
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
                "Coordinate system origin coordinates must be finite.");
        }
    }

    private static void ValidateFiniteVector(
        Vector3D vector,
        string parameterName)
    {
        if (!double.IsFinite(vector.X) ||
            !double.IsFinite(vector.Y) ||
            !double.IsFinite(vector.Z))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                vector,
                "Coordinate system axis coordinates must be finite.");
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
                "Coordinate system axis must be a unit vector within tolerance.",
                parameterName);
        }
    }

    private static void ValidateOrthogonal(
        Vector3D xAxis,
        Vector3D yAxis)
    {
        var dotProduct =
            xAxis.X * yAxis.X +
            xAxis.Y * yAxis.Y +
            xAxis.Z * yAxis.Z;

        if (Math.Abs(dotProduct) > OrthogonalityTolerance)
        {
            throw new ArgumentException(
                "Coordinate system X and Y axes must be orthogonal within tolerance.",
                nameof(yAxis));
        }
    }

    private static Vector3D Cross(
        Vector3D first,
        Vector3D second)
    {
        return new Vector3D(
            first.Y * second.Z - first.Z * second.Y,
            first.Z * second.X - first.X * second.Z,
            first.X * second.Y - first.Y * second.X);
    }
}