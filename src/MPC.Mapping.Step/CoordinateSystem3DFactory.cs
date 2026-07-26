using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

internal static class CoordinateSystem3DFactory
{
    private static readonly Vector3D DefaultXAxis =
        new(
            1.0,
            0.0,
            0.0);

    private static readonly Vector3D DefaultYAxis =
        new(
            0.0,
            1.0,
            0.0);

    public static CoordinateSystem3D Create(
        Point3D origin,
        Vector3D zAxis,
        Vector3D? referenceDirection)
    {
        Vector3D normalizedZAxis =
            StepGeometryMath.Normalize(
                zAxis);

        Vector3D xAxis =
            referenceDirection.HasValue
                ? CreateXAxisFromReferenceDirection(
                    referenceDirection.Value,
                    normalizedZAxis)
                : CreateDeterministicXAxis(
                    normalizedZAxis);

        Vector3D yAxis =
            StepGeometryMath.Normalize(
                StepGeometryMath.Cross(
                    normalizedZAxis,
                    xAxis));

        return new CoordinateSystem3D(
            origin,
            xAxis,
            yAxis);
    }

    private static Vector3D
        CreateXAxisFromReferenceDirection(
            Vector3D referenceDirection,
            Vector3D zAxis)
    {
        return StepGeometryMath
            .ProjectOntoPlaneAndNormalize(
                referenceDirection,
                zAxis);
    }

    private static Vector3D
        CreateDeterministicXAxis(
            Vector3D zAxis)
    {
        double xAlignment =
            Math.Abs(
                StepGeometryMath.Dot(
                    zAxis,
                    DefaultXAxis));

        double yAlignment =
            Math.Abs(
                StepGeometryMath.Dot(
                    zAxis,
                    DefaultYAxis));

        Vector3D candidate =
            xAlignment < yAlignment
                ? DefaultXAxis
                : DefaultYAxis;

        return StepGeometryMath
            .ProjectOntoPlaneAndNormalize(
                candidate,
                zAxis);
    }
}