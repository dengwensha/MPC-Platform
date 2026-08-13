using MPC.Topology.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MPC.Topology;

public sealed class Shell : IEnumerable<Face>
{
    private readonly IReadOnlyList<Face> _faces;

    public IReadOnlyList<Face> Faces =>
        _faces;

    public int Count =>
        _faces.Count;

    public bool IsClosed
    {
        get;
    }

    public Shell(
        IEnumerable<Face> faces)
    {
        ArgumentNullException.ThrowIfNull(
            faces);

        List<Face> faceList =
            faces.ToList();

        if (faceList.Count == 0)
        {
            throw new ArgumentException(
                "A shell must contain at least one face.",
                nameof(faces));
        }

        if (faceList.Any(
                face => face is null))
        {
            throw new ArgumentException(
                "A shell cannot contain null faces.",
                nameof(faces));
        }

        _faces =
            faceList.AsReadOnly();

        IsClosed =
            DetermineIsClosed(
                faceList);
    }

    public IEnumerator<Face> GetEnumerator()
    {
        return _faces.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static bool DetermineIsClosed(
        IReadOnlyList<Face> faces)
    {
        var edgeUseCounts =
            new Dictionary<Edge, int>(
                ReferenceEqualityComparer.Instance);

        foreach (Face face in faces)
        {
            CountLoopEdges(
                face.OuterLoop,
                edgeUseCounts);

            foreach (Loop innerLoop
                     in face.InnerLoops)
            {
                CountLoopEdges(
                    innerLoop,
                    edgeUseCounts);
            }
        }

        return
            edgeUseCounts.Count > 0 &&
            edgeUseCounts.Values.All(
                count => count == 2);
    }

    private static void CountLoopEdges(
        Loop loop,
        IDictionary<Edge, int> edgeUseCounts)
    {
        foreach (EdgeUse edgeUse
                 in loop.EdgeUses)
        {
            if (edgeUseCounts.TryGetValue(
                    edgeUse.Edge,
                    out int currentCount))
            {
                edgeUseCounts[edgeUse.Edge] =
                    currentCount + 1;
            }
            else
            {
                edgeUseCounts.Add(
                    edgeUse.Edge,
                    1);
            }
        }
    }
}