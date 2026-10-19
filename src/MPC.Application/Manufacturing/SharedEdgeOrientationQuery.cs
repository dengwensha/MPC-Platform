using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

public enum SharedEdgeOrientationStatus
{
    Indeterminate,
    Opposite,
    SameDirection
}

/// <summary>Compares effective oriented uses of one uniquely shared canonical edge.
/// Loop reversal reverses the edge-use traversal. This does not verify outward
/// normals, solid material side or concavity/convexity.</summary>
public static class SharedEdgeOrientationQuery
{
    public static SharedEdgeOrientationStatus Evaluate(
        IReadOnlyList<Face> faces, Face first, Face second)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (ReferenceEquals(first, second))
            return SharedEdgeOrientationStatus.Indeterminate;

        var boundaries = FaceAdjacencyQuery.Analyze(faces, first).Boundaries
            .Where(b => b.HasExactlyTwoFaceOwners &&
                b.Neighbors.Any(n => ReferenceEquals(n, second)))
            .ToArray();
        if (boundaries.Length != 1)
            return SharedEdgeOrientationStatus.Indeterminate;

        Edge shared = boundaries[0].Edge;
        if (!TryDirection(first, shared, out bool firstForward) ||
            !TryDirection(second, shared, out bool secondForward))
            return SharedEdgeOrientationStatus.Indeterminate;

        return firstForward == secondForward
            ? SharedEdgeOrientationStatus.SameDirection
            : SharedEdgeOrientationStatus.Opposite;
    }

    private static bool TryDirection(Face face, Edge edge, out bool forward)
    {
        forward = false;
        int matches = 0;
        foreach (var loopUse in new[] { face.OuterLoopUse }
            .Concat(face.InnerLoopUses))
        {
            foreach (var use in loopUse.Loop.EdgeUses)
            {
                if (!ReferenceEquals(use.Edge, edge)) continue;
                matches++;
                forward = (use.Direction == EdgeUseDirection.Forward) ==
                    (loopUse.Direction == LoopUseDirection.Forward);
            }
        }
        return matches == 1;
    }
}
