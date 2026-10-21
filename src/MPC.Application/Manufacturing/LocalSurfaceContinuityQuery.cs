using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Conservative classification at one verified common-edge witness.
/// Coincident tangent planes include opposite normal directions.
/// No result certifies oriented G1 or boundary-wide G0/G1.</summary>
public enum LocalSurfaceContinuityKind
{
    Indeterminate,
    GeometricMismatchAtSample,
    SharpAtSample,
    TangentPlaneCoincidentAtSample
}

public sealed record LocalSurfaceContinuityEvidence(
    LocalSurfaceContinuityKind Kind,
    LocalG0Status G0,
    LocalTangentStatus Tangency,
    int EligibleInteriorSampleCount);

public static class LocalSurfaceContinuityQuery
{
    public static LocalSurfaceContinuityEvidence Evaluate(
        IReadOnlyList<Face> faces, Face first, Face second,
        double linearTolerance, double angularToleranceDegrees)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (!double.IsFinite(linearTolerance) || linearTolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(linearTolerance));
        if (!double.IsFinite(angularToleranceDegrees) ||
            angularToleranceDegrees < 0 || angularToleranceDegrees > 90)
            throw new ArgumentOutOfRangeException(nameof(angularToleranceDegrees));

        LocalG0Status g0 = LocalG0PointMembershipQuery.Evaluate(
            faces, first, second, linearTolerance, out _);
        LocalTangentStatus tangent = LocalTangentStatus.Indeterminate;
        int availableSamples = 0;

        // D4 identifies eligible interior witnesses only. These points are NOT\n        // evaluated by the G0/tangency queries below, which use one separate\n        // common-edge witness. This count is not a coverage or G0/G1 proof.
        if (!ReferenceEquals(first, second))
        {
            var boundaries = FaceAdjacencyQuery.Analyze(faces, first).Boundaries
                .Where(b => b.HasExactlyTwoFaceOwners &&
                    b.Neighbors.Any(n => ReferenceEquals(n, second))).ToArray();
            if (boundaries.Length == 1 &&
                CommonEdgeMultiSampleQuery.TrySample(boundaries[0].Edge, out var points))
                availableSamples = points.Count;
        }

        if (g0 == LocalG0Status.Indeterminate)
            return new(LocalSurfaceContinuityKind.Indeterminate, g0, tangent, availableSamples);
        if (g0 == LocalG0Status.MismatchAtSample)
            return new(LocalSurfaceContinuityKind.GeometricMismatchAtSample, g0, tangent, availableSamples);

        tangent = LocalTangentPlaneQuery.Evaluate(
            faces, first, second, linearTolerance, angularToleranceDegrees, out _);
        LocalSurfaceContinuityKind kind = tangent switch
        {
            LocalTangentStatus.TangentPlaneCoincidentAtSample => LocalSurfaceContinuityKind.TangentPlaneCoincidentAtSample,
            LocalTangentStatus.NotTangentAtSample => LocalSurfaceContinuityKind.SharpAtSample,
            _ => LocalSurfaceContinuityKind.Indeterminate
        };
        return new(kind, g0, tangent, availableSamples);
    }
}
