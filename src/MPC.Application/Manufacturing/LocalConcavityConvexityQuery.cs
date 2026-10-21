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
/// Fail-closed manufacturing classification. A caller-supplied boolean is not
/// evidence of verified outward material normals or consistent loop winding.
/// Until a trusted proof provider exists this API cannot classify material side.
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

        // Intentional: the boolean is retained for call compatibility but
        // cannot grant authority. No verified material-side evidence type exists.
        _ = outwardNormalsIndependentlyVerified;
        return LocalDihedralShape.Indeterminate;
    }
}
