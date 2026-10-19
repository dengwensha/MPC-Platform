using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

public enum LocalDihedralShape
{
    Indeterminate,
    Concave,
    Convex,
    Smooth
}

/// <summary>
/// Classifies an F3 signed local dihedral witness only when an independent
/// caller has verified that both oriented normals point outward from material.
/// The canonical Shell.IsClosed property does NOT constitute that proof.
/// No global outward-normal verifier is implemented by this component.
/// </summary>
public static class LocalConcavityConvexityQuery
{
    public static LocalDihedralShape Evaluate(
        IReadOnlyList<Face> faces, Face first, Face second,
        bool outwardNormalsIndependentlyVerified,
        double angularToleranceDegrees)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (!double.IsFinite(angularToleranceDegrees) ||
            angularToleranceDegrees < 0 || angularToleranceDegrees >= 90)
            throw new ArgumentOutOfRangeException(nameof(angularToleranceDegrees));

        // Never infer the material side from a face flag or edge counts.
        if (!outwardNormalsIndependentlyVerified ||
            !SignedLocalDihedralQuery.TryCalculate(faces, first, second, out double signedAngle))
            return LocalDihedralShape.Indeterminate;

        return ClassifyVerifiedOutwardAngle(signedAngle, angularToleranceDegrees);
    }

    /// <summary>
    /// Mathematical classifier for an externally verified outward-normal
    /// signed angle. Positive rotation around first-face effective edge-use
    /// direction is convex; negative is concave. No geometric proof here.
    /// </summary>
    public static LocalDihedralShape ClassifyVerifiedOutwardAngle(
        double signedAngleDegrees, double angularToleranceDegrees)
    {
        if (!double.IsFinite(angularToleranceDegrees) ||
            angularToleranceDegrees < 0 || angularToleranceDegrees >= 90)
            throw new ArgumentOutOfRangeException(nameof(angularToleranceDegrees));
        if (!double.IsFinite(signedAngleDegrees) ||
            signedAngleDegrees < -180 || signedAngleDegrees > 180)
            return LocalDihedralShape.Indeterminate;
        if (Math.Abs(signedAngleDegrees) >= 180 - angularToleranceDegrees)
            return LocalDihedralShape.Indeterminate;
        if (Math.Abs(signedAngleDegrees) <= angularToleranceDegrees)
            return LocalDihedralShape.Smooth;
        return signedAngleDegrees > 0 ? LocalDihedralShape.Convex : LocalDihedralShape.Concave;
    }
}
