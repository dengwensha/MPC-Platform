using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

internal static class StepGeometryMath
{
    private const double ZeroLengthTolerance =
        1e-15;

    public static double Dot(
        Vector3D first,
        Vector3D second)
    {
        return
            first.X * second.X +
            first.Y * second.Y +
            first.Z * second.Z;
    }

    public static Vector3D Cross(
        Vector3D first,
        Vector3D second)
    {
        return new Vector3D(
            first.Y * second.Z -
            first.Z * second.Y,

            first.Z * second.X -
            first.X * second.Z,

            first.X * second.Y -
            first.Y * second.X);
    }

    public static Vector3D Normalize(
        Vector3D vector)
    {
        double lengthSquared =
            Dot(
                vector,
                vector);

        if (!double.IsFinite(lengthSquared) ||
            lengthSquared <=
            ZeroLengthTolerance *
            ZeroLengthTolerance)
        {
            throw new ArgumentException(
                "A zero-length or numerically invalid vector cannot be normalized.",
                nameof(vector));
        }

        double length =
            Math.Sqrt(
                lengthSquared);

        return new Vector3D(
            vector.X / length,
            vector.Y / length,
            vector.Z / length);
    }

    public static Vector3D ProjectOntoPlane(
        Vector3D vector,
        Vector3D planeNormal)
    {
        Vector3D unitNormal =
            Normalize(
                planeNormal);

        double projectionScale =
            Dot(
                vector,
                unitNormal);

        return new Vector3D(
            vector.X -
            projectionScale * unitNormal.X,

            vector.Y -
            projectionScale * unitNormal.Y,

            vector.Z -
            projectionScale * unitNormal.Z);
    }

    public static Vector3D ProjectOntoPlaneAndNormalize(
        Vector3D vector,
        Vector3D planeNormal)
    {
        Vector3D projected =
            ProjectOntoPlane(
                vector,
                planeNormal);

        return Normalize(
            projected);
    }
}