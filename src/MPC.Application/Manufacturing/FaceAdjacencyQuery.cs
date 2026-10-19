using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Read-only adjacency using canonical edge object identity.</summary>
public static class FaceAdjacencyQuery
{
    public static FaceAdjacencyResult Analyze(IReadOnlyList<Face> faces, Face selected)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(selected);
        if (faces.Any(face => face is null))
            throw new ArgumentException("Face collection cannot contain null.", nameof(faces));
        if (faces.Distinct(ReferenceEqualityComparer.Instance).Count() != faces.Count)
            throw new ArgumentException("Face collection contains duplicates.", nameof(faces));
        if (!faces.Any(face => ReferenceEquals(face, selected)))
            throw new ArgumentException("Selected face is absent.", nameof(selected));

        var owners = new Dictionary<Edge, HashSet<Face>>(ReferenceEqualityComparer.Instance);
        foreach (Face face in faces)
            foreach (Edge edge in BoundaryEdges(face))
            {
                if (!owners.TryGetValue(edge, out var set))
                    owners.Add(edge, set = new HashSet<Face>(ReferenceEqualityComparer.Instance));
                set.Add(face);
            }

        var boundaries = new List<FaceBoundaryAdjacency>();
        Append(selected.OuterLoop.EdgeUses, false);
        foreach (Loop loop in selected.InnerLoops)
            Append(loop.EdgeUses, true);

        return new FaceAdjacencyResult(boundaries.ToArray(),
            boundaries.SelectMany(boundary => boundary.Neighbors)
                .Distinct(ReferenceEqualityComparer.Instance).Cast<Face>().ToArray());

        void Append(IReadOnlyList<EdgeUse> uses, bool isInner)
        {
            foreach (EdgeUse use in uses)
            {
                Face[] neighbors = owners[use.Edge]
                    .Where(face => !ReferenceEquals(face, selected)).ToArray();
                boundaries.Add(new FaceBoundaryAdjacency(
                    use.Edge, isInner, neighbors, owners[use.Edge].Count == 2));
            }
        }
    }

    private static IEnumerable<Edge> BoundaryEdges(Face face)
    {
        foreach (EdgeUse use in face.OuterLoop.EdgeUses)
            yield return use.Edge;
        foreach (Loop loop in face.InnerLoops)
            foreach (EdgeUse use in loop.EdgeUses)
                yield return use.Edge;
    }
}

public sealed class FaceBoundaryAdjacency
{
    public Edge Edge { get; }
    public bool IsInnerBoundary { get; }
    public IReadOnlyList<Face> Neighbors { get; }
    public bool HasExactlyTwoFaceOwners { get; }

    internal FaceBoundaryAdjacency(
        Edge edge, bool isInner, Face[] neighbors, bool exactlyTwo)
    {
        Edge = edge;
        IsInnerBoundary = isInner;
        Neighbors = Array.AsReadOnly(neighbors);
        HasExactlyTwoFaceOwners = exactlyTwo;
    }
}

public sealed class FaceAdjacencyResult
{
    public IReadOnlyList<FaceBoundaryAdjacency> Boundaries { get; }
    public IReadOnlyList<Face> UniqueNeighbors { get; }

    internal FaceAdjacencyResult(FaceBoundaryAdjacency[] boundaries, Face[] neighbors)
    {
        Boundaries = Array.AsReadOnly(boundaries);
        UniqueNeighbors = Array.AsReadOnly(neighbors);
    }
}
