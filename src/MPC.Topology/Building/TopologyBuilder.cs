using System.Collections.ObjectModel;
using MPC.Topology.Primitives;

namespace MPC.Topology.Building;

public sealed class TopologyBuilder
{
    private readonly List<Vertex> _vertices;
    private readonly List<Edge> _edges;
    private readonly List<Loop> _loops;

    private readonly ReadOnlyCollection<Vertex> _verticesView;
    private readonly ReadOnlyCollection<Edge> _edgesView;
    private readonly ReadOnlyCollection<Loop> _loopsView;

    public TopologyBuilderState State { get; private set; }

    public IReadOnlyCollection<Vertex> Vertices =>
        _verticesView;

    public IReadOnlyCollection<Edge> Edges =>
        _edgesView;

    public IReadOnlyCollection<Loop> Loops =>
        _loopsView;

    public TopologyBuilder()
    {
        _vertices = [];
        _edges = [];
        _loops = [];

        _verticesView = _vertices.AsReadOnly();
        _edgesView = _edges.AsReadOnly();
        _loopsView = _loops.AsReadOnly();

        State = TopologyBuilderState.Open;
    }

    public void RegisterVertex(Vertex vertex)
    {
        EnsureOpen();

        ArgumentNullException.ThrowIfNull(vertex);

        if (ContainsVertex(vertex))
        {
            return;
        }

        _vertices.Add(vertex);
    }

    public void RegisterEdge(Edge edge)
    {
        EnsureOpen();

        ArgumentNullException.ThrowIfNull(edge);

        if (ContainsEdge(edge))
        {
            return;
        }

        RegisterVertex(edge.StartVertex);
        RegisterVertex(edge.EndVertex);

        _edges.Add(edge);
    }

    public void RegisterLoop(Loop loop)
    {
        EnsureOpen();

        ArgumentNullException.ThrowIfNull(loop);

        if (ContainsLoop(loop))
        {
            return;
        }

        foreach (EdgeUse edgeUse in loop.EdgeUses)
        {
            RegisterEdge(edgeUse.Edge);
        }

        VerifyLoop(loop);

        _loops.Add(loop);
    }

    public void Verify()
    {
        EnsureOpen();

        VerifyGraph();
    }

    public PublishedTopology Publish()
    {
        EnsureOpen();

        VerifyGraph();

        List<(Vertex Vertex, Edge Edge)> pendingAttachments =
            CreatePendingAttachments();

        PublishedTopology publishedTopology =
            new(
                _vertices,
                _edges,
                _loops);

        foreach ((Vertex vertex, Edge edge) in pendingAttachments)
        {
            vertex.AttachEdge(edge);
        }

        State = TopologyBuilderState.Published;

        return publishedTopology;
    }

    public void Rollback()
    {
        EnsureOpen();

        _loops.Clear();
        _edges.Clear();
        _vertices.Clear();

        State = TopologyBuilderState.RolledBack;
    }

    private void VerifyGraph()
    {
        VerifyRegisteredEdges();
        VerifyExistingVertexIncidentEdges();
        VerifyRegisteredLoops();
    }

    private void VerifyRegisteredEdges()
    {
        foreach (Edge edge in _edges)
        {
            if (!ContainsVertex(edge.StartVertex))
            {
                throw new InvalidOperationException(
                    "An edge references an unregistered start vertex.");
            }

            if (!ContainsVertex(edge.EndVertex))
            {
                throw new InvalidOperationException(
                    "An edge references an unregistered end vertex.");
            }
        }
    }

    private void VerifyExistingVertexIncidentEdges()
    {
        foreach (Vertex vertex in _vertices)
        {
            HashSet<Edge> encounteredEdges =
                new(ReferenceEqualityComparer.Instance);

            foreach (Edge incidentEdge in vertex.IncidentEdges)
            {
                if (!ContainsEdge(incidentEdge))
                {
                    throw new InvalidOperationException(
                        "A registered vertex references an edge that is not registered in the builder.");
                }

                bool edgeReferencesVertex =
                    ReferenceEquals(
                        incidentEdge.StartVertex,
                        vertex) ||
                    ReferenceEquals(
                        incidentEdge.EndVertex,
                        vertex);

                if (!edgeReferencesVertex)
                {
                    throw new InvalidOperationException(
                        "A vertex contains an incident edge that does not reference the vertex.");
                }

                if (!encounteredEdges.Add(incidentEdge))
                {
                    throw new InvalidOperationException(
                        "A vertex contains the same incident edge more than once.");
                }
            }
        }
    }

    private void VerifyRegisteredLoops()
    {
        foreach (Loop loop in _loops)
        {
            VerifyLoop(loop);
        }
    }

    private void VerifyLoop(Loop loop)
    {
        if (loop.EdgeUses.Count < 3)
        {
            throw new InvalidOperationException(
                "A registered loop must contain at least three edge uses.");
        }

        for (int index = 0;
             index < loop.EdgeUses.Count;
             index++)
        {
            EdgeUse currentEdgeUse =
                loop.EdgeUses[index];

            if (!ContainsEdge(currentEdgeUse.Edge))
            {
                throw new InvalidOperationException(
                    "A loop references an edge that is not registered in the builder.");
            }

            int nextIndex =
                (index + 1) %
                loop.EdgeUses.Count;

            EdgeUse nextEdgeUse =
                loop.EdgeUses[nextIndex];

            if (!ReferenceEquals(
                    currentEdgeUse.EndVertex,
                    nextEdgeUse.StartVertex))
            {
                throw new InvalidOperationException(
                    "A loop contains discontinuous edge uses.");
            }
        }
    }

    private List<(Vertex Vertex, Edge Edge)>
        CreatePendingAttachments()
    {
        List<(Vertex Vertex, Edge Edge)> attachments = [];

        foreach (Edge edge in _edges)
        {
            attachments.Add(
                (
                    edge.StartVertex,
                    edge
                ));

            if (!ReferenceEquals(
                    edge.StartVertex,
                    edge.EndVertex))
            {
                attachments.Add(
                    (
                        edge.EndVertex,
                        edge
                    ));
            }
        }

        return attachments;
    }

    private bool ContainsVertex(Vertex vertex)
    {
        return _vertices.Any(
            registeredVertex =>
                ReferenceEquals(
                    registeredVertex,
                    vertex));
    }

    private bool ContainsEdge(Edge edge)
    {
        return _edges.Any(
            registeredEdge =>
                ReferenceEquals(
                    registeredEdge,
                    edge));
    }

    private bool ContainsLoop(Loop loop)
    {
        return _loops.Any(
            registeredLoop =>
                ReferenceEquals(
                    registeredLoop,
                    loop));
    }

    private void EnsureOpen()
    {
        if (State != TopologyBuilderState.Open)
        {
            throw new InvalidOperationException(
                $"TopologyBuilder is closed. Current state: {State}.");
        }
    }
}