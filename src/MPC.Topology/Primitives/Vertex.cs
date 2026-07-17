using System.Collections.ObjectModel;
using MPC.Geometry.Primitives;

namespace MPC.Topology.Primitives;

public sealed class Vertex
{
    private readonly List<Edge> _incidentEdges;
    private readonly ReadOnlyCollection<Edge> _incidentEdgesView;

    public Guid Id { get; }

    public Point3D Position { get; }

    public IReadOnlyList<Edge> IncidentEdges =>
        _incidentEdgesView;

    public Vertex(
        double x,
        double y,
        double z)
        : this(
            new Point3D(
                x,
                y,
                z))
    {
    }

    public Vertex(Point3D position)
    {
        Id = Guid.NewGuid();
        Position = position;

        _incidentEdges = [];
        _incidentEdgesView = _incidentEdges.AsReadOnly();
    }

    internal void AttachEdge(Edge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        bool referencesVertex =
            ReferenceEquals(
                edge.StartVertex,
                this) ||
            ReferenceEquals(
                edge.EndVertex,
                this);

        if (!referencesVertex)
        {
            throw new ArgumentException(
                "The edge does not reference this vertex.",
                nameof(edge));
        }

        bool alreadyAttached =
            _incidentEdges.Any(
                existingEdge =>
                    ReferenceEquals(
                        existingEdge,
                        edge));

        if (alreadyAttached)
        {
            return;
        }

        _incidentEdges.Add(edge);
    }
}