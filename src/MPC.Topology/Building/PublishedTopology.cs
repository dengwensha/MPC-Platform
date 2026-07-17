using System.Collections.ObjectModel;
using MPC.Topology.Primitives;

namespace MPC.Topology.Building;

public sealed class PublishedTopology
{
    private readonly ReadOnlyCollection<Vertex> _vertices;
    private readonly ReadOnlyCollection<Edge> _edges;
    private readonly ReadOnlyCollection<Loop> _loops;

    public IReadOnlyList<Vertex> Vertices =>
        _vertices;

    public IReadOnlyList<Edge> Edges =>
        _edges;

    public IReadOnlyList<Loop> Loops =>
        _loops;

    internal PublishedTopology(
        IEnumerable<Vertex> vertices,
        IEnumerable<Edge> edges,
        IEnumerable<Loop> loops)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(edges);
        ArgumentNullException.ThrowIfNull(loops);

        _vertices =
            new List<Vertex>(vertices)
                .AsReadOnly();

        _edges =
            new List<Edge>(edges)
                .AsReadOnly();

        _loops =
            new List<Loop>(loops)
                .AsReadOnly();
    }
}