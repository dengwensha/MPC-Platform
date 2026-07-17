using System.Collections.ObjectModel;
using MPC.Topology.Primitives;

namespace MPC.Topology.Building;

public sealed class PublishedTopology
{
    private readonly ReadOnlyCollection<Vertex> _vertices;
    private readonly ReadOnlyCollection<Edge> _edges;
    private readonly ReadOnlyCollection<Loop> _loops;
    private readonly ReadOnlyCollection<Face> _faces;

    public IReadOnlyList<Vertex> Vertices =>
        _vertices;

    public IReadOnlyList<Edge> Edges =>
        _edges;

    public IReadOnlyList<Loop> Loops =>
        _loops;

    public IReadOnlyList<Face> Faces =>
        _faces;

    internal PublishedTopology(
        IEnumerable<Vertex> vertices,
        IEnumerable<Edge> edges,
        IEnumerable<Loop> loops,
        IEnumerable<Face> faces)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(edges);
        ArgumentNullException.ThrowIfNull(loops);
        ArgumentNullException.ThrowIfNull(faces);

        _vertices =
            new List<Vertex>(vertices)
                .AsReadOnly();

        _edges =
            new List<Edge>(edges)
                .AsReadOnly();

        _loops =
            new List<Loop>(loops)
                .AsReadOnly();

        _faces =
            new List<Face>(faces)
                .AsReadOnly();
    }
}