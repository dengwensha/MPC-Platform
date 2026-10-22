using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

public enum LocalTangentStatus
{
    Indeterminate,
    TangentPlaneCoincidentAtSample,
    NotTangentAtSample
}

/// <summary>Unoriented tangent-plane coincidence at one witness only.
/// Antiparallel normals also coincide as planes; this is not oriented G1,
/// outward-material verification, or edge-wide continuity.</summary>
public static class LocalTangentPlaneQuery
{
    public static LocalTangentStatus Evaluate(
        IReadOnlyList<Face> faces, Face first, Face second,
        double angularToleranceDegrees, out double differenceDegrees)
        => Evaluate(faces, first, second, 1e-8, angularToleranceDegrees,
            out differenceDegrees);

    /// <summary>Explicit linear tolerance applies to curved-face witness membership.</summary>
    public static LocalTangentStatus Evaluate(
        IReadOnlyList<Face> faces, Face first, Face second,
        double linearTolerance, double angularToleranceDegrees,
        out double differenceDegrees)
    {
        if (!double.IsFinite(linearTolerance) || linearTolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(linearTolerance));
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (!double.IsFinite(angularToleranceDegrees) ||
            angularToleranceDegrees < 0 || angularToleranceDegrees > 90)
            throw new ArgumentOutOfRangeException(nameof(angularToleranceDegrees));

        differenceDegrees = 0;
        if (ReferenceEquals(first, second)) return LocalTangentStatus.Indeterminate;

        // Existing plane-plane query validates topological adjacency.
        if (first.Surface is Plane3D && second.Surface is Plane3D)
        {
            if (!PlanarFaceAngleQuery.TryCalculate(
                faces, first, second, out double planeAngle))
                return LocalTangentStatus.Indeterminate;
            differenceDegrees = Math.Min(planeAngle, 180 - planeAngle);
        }
        else
        {
            // Existing curved-face query validates a unique shared edge,
            // a supported sample and on-surface membership for curved face.
            if (!PlanarCurvedFaceAngleQuery.TryCalculate(
                faces, first, second, linearTolerance, out double curvedAngle))
                return LocalTangentStatus.Indeterminate;
            differenceDegrees = Math.Min(curvedAngle, 180 - curvedAngle);
        }

        return differenceDegrees <= angularToleranceDegrees
            ? LocalTangentStatus.TangentPlaneCoincidentAtSample
            : LocalTangentStatus.NotTangentAtSample;
    }
}
