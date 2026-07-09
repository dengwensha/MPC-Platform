using MPC.Topology.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MPC.Topology;

public sealed class Shell : IEnumerable<Face>
{
    private readonly IReadOnlyList<Face> _faces;

    public IReadOnlyList<Face> Faces => _faces;

    public int Count => _faces.Count;

    public Shell(IEnumerable<Face> faces)
    {
        ArgumentNullException.ThrowIfNull(faces);

        var faceList = faces.ToList();

        if (faceList.Count == 0)
            throw new ArgumentException("A shell must contain at least one face.", nameof(faces));

        if (faceList.Any(face => face is null))
            throw new ArgumentException("A shell cannot contain null faces.", nameof(faces));

        _faces = faceList.AsReadOnly();
    }

    public IEnumerator<Face> GetEnumerator()
    {
        return _faces.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}