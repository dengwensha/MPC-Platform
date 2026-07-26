using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

internal static class StepGeometryConversions
{
    private const int RequiredDimension =
        3;

    private const double ZeroLengthTolerance =
        1e-15;

    public static Point3D ToPoint3D(
        StepCartesianPoint source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ValidateDimension(
            source.Coordinates.Count,
            source.SourceEntityId,
            "CARTESIAN_POINT",
            "coordinate");

        return new Point3D(
            source.Coordinates[0],
            source.Coordinates[1],
            source.Coordinates[2]);
    }

    public static Vector3D ToUnitVector3D(
        StepDirection source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ValidateDimension(
            source.DirectionRatios.Count,
            source.SourceEntityId,
            "DIRECTION",
            "direction ratio");

        double x =
            source.DirectionRatios[0];

        double y =
            source.DirectionRatios[1];

        double z =
            source.DirectionRatios[2];

        double lengthSquared =
            x * x +
            y * y +
            z * z;

        if (!double.IsFinite(lengthSquared) ||
            lengthSquared <=
            ZeroLengthTolerance *
            ZeroLengthTolerance)
        {
            throw new ArgumentException(
                CreateInvalidDirectionMessage(
                    source),
                nameof(source));
        }

        double length =
            Math.Sqrt(
                lengthSquared);

        return new Vector3D(
            x / length,
            y / length,
            z / length);
    }

    private static void ValidateDimension(
        int actualCount,
        string? sourceEntityId,
        string entityType,
        string componentName)
    {
        if (actualCount ==
            RequiredDimension)
        {
            return;
        }

        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                sourceEntityId)
                ? entityType
                : sourceEntityId;

        throw new ArgumentException(
            $"STEP {entityType} '{sourceIdentity}' contains " +
            $"{actualCount} {componentName} values. " +
            $"Three-dimensional canonical geometry requires exactly " +
            $"{RequiredDimension} values.");
    }

    private static string CreateInvalidDirectionMessage(
        StepDirection source)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "DIRECTION"
                : source.SourceEntityId;

        return
            $"STEP DIRECTION '{sourceIdentity}' cannot be converted " +
            "to a canonical unit vector because its magnitude is zero " +
            "or numerically invalid.";
    }
}