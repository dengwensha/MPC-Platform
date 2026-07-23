using System.Collections.ObjectModel;
using MPC.Topology.Primitives;

namespace MPC.Topology.Building;

public sealed class TopologyBuilder
{
    private readonly List<Vertex> _vertices;
    private readonly List<Edge> _edges;
    private readonly List<Loop> _loops;
    private readonly List<Face> _faces;

    private readonly ReadOnlyCollection<Vertex> _verticesView;
    private readonly ReadOnlyCollection<Edge> _edgesView;
    private readonly ReadOnlyCollection<Loop> _loopsView;
    private readonly ReadOnlyCollection<Face> _facesView;

    public TopologyBuilderState State { get; private set; }

    public IReadOnlyCollection<Vertex> Vertices =>
        _verticesView;

    public IReadOnlyCollection<Edge> Edges =>
        _edgesView;

    public IReadOnlyCollection<Loop> Loops =>
        _loopsView;

    public IReadOnlyCollection<Face> Faces =>
        _facesView;

    public TopologyBuilder()
    {
        _vertices = [];
        _edges = [];
        _loops = [];
        _faces = [];

        _verticesView =
            _vertices.AsReadOnly();

        _edgesView =
            _edges.AsReadOnly();

        _loopsView =
            _loops.AsReadOnly();

        _facesView =
            _faces.AsReadOnly();

        State =
            TopologyBuilderState.Open;
    }

    public Loop BuildLoop(
        IEnumerable<EdgeUse> edgeUses)
    {
        EnsureOpen();

        ArgumentNullException.ThrowIfNull(
            edgeUses);

        Loop loop =
            new(
                edgeUses);

        RegisterLoop(
            loop);

        return loop;
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

        RegistrationCheckpoint checkpoint =
            CreateRegistrationCheckpoint();

        try
        {
            RegisterVertex(
                edge.StartVertex);

            RegisterVertex(
                edge.EndVertex);

            _edges.Add(edge);
        }
        catch
        {
            RestoreRegistrationCheckpoint(
                checkpoint);

            throw;
        }
    }

    public void RegisterLoop(Loop loop)
    {
        EnsureOpen();

        ArgumentNullException.ThrowIfNull(loop);

        if (ContainsLoop(loop))
        {
            return;
        }

        RegistrationCheckpoint checkpoint =
            CreateRegistrationCheckpoint();

        try
        {
            foreach (EdgeUse edgeUse in loop.EdgeUses)
            {
                RegisterEdge(
                    edgeUse.Edge);
            }

            VerifyLoop(loop);

            _loops.Add(loop);
        }
        catch
        {
            RestoreRegistrationCheckpoint(
                checkpoint);

            throw;
        }
    }

    public void RegisterFace(Face face)
    {
        EnsureOpen();

        ArgumentNullException.ThrowIfNull(face);

        if (ContainsFace(face))
        {
            return;
        }

        RegistrationCheckpoint checkpoint =
            CreateRegistrationCheckpoint();

        try
        {
            RegisterLoop(
                face.OuterLoop);

            foreach (Loop innerLoop in face.InnerLoops)
            {
                RegisterLoop(
                    innerLoop);
            }

            VerifyFace(face);

            _faces.Add(face);
        }
        catch
        {
            RestoreRegistrationCheckpoint(
                checkpoint);

            throw;
        }
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
                _loops,
                _faces);

        foreach ((Vertex vertex, Edge edge) in pendingAttachments)
        {
            vertex.AttachEdge(edge);
        }

        State =
            TopologyBuilderState.Published;

        return publishedTopology;
    }

    public void Rollback()
    {
        EnsureOpen();

        _faces.Clear();
        _loops.Clear();
        _edges.Clear();
        _vertices.Clear();

        State =
            TopologyBuilderState.RolledBack;
    }

    private void VerifyGraph()
    {
        VerifyRegisteredEdges();
        VerifyExistingVertexIncidentEdges();
        VerifyRegisteredLoops();
        VerifyRegisteredFaces();
    }

    private void VerifyRegisteredEdges()
    {
        foreach (Edge edge in _edges)
        {
            if (!ContainsVertex(
                    edge.StartVertex))
            {
                throw new InvalidOperationException(
                    "An edge references an unregistered start vertex.");
            }

            if (!ContainsVertex(
                    edge.EndVertex))
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
                new(
                    ReferenceEqualityComparer.Instance);

            foreach (Edge incidentEdge in vertex.IncidentEdges)
            {
                if (!ContainsEdge(
                        incidentEdge))
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

                if (!encounteredEdges.Add(
                        incidentEdge))
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

    private void VerifyRegisteredFaces()
    {
        HashSet<Face> encounteredFaces =
            new(
                ReferenceEqualityComparer.Instance);

        foreach (Face face in _faces)
        {
            if (!encounteredFaces.Add(face))
            {
                throw new InvalidOperationException(
                    "The same face is registered more than once.");
            }

            VerifyFace(face);
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

            if (!ContainsEdge(
                    currentEdgeUse.Edge))
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

    private void VerifyFace(Face face)
    {
        if (face.Surface is null)
        {
            throw new InvalidOperationException(
                "A face must reference a surface.");
        }

        if (!ContainsLoop(
                face.OuterLoop))
        {
            throw new InvalidOperationException(
                "A face references an outer loop that is not registered in the builder.");
        }

        HashSet<Loop> encounteredInnerLoops =
            new(
                ReferenceEqualityComparer.Instance);

        foreach (Loop innerLoop in face.InnerLoops)
        {
            if (ReferenceEquals(
                    face.OuterLoop,
                    innerLoop))
            {
                throw new InvalidOperationException(
                    "A face outer loop cannot also be registered as an inner loop.");
            }

            if (!encounteredInnerLoops.Add(
                    innerLoop))
            {
                throw new InvalidOperationException(
                    "A face contains the same inner loop more than once.");
            }

            if (!ContainsLoop(
                    innerLoop))
            {
                throw new InvalidOperationException(
                    "A face references an inner loop that is not registered in the builder.");
            }
        }
    }

    private List<(Vertex Vertex, Edge Edge)>
        CreatePendingAttachments()
    {
        List<(Vertex Vertex, Edge Edge)> attachments =
            [];

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

    private RegistrationCheckpoint
        CreateRegistrationCheckpoint()
    {
        return new RegistrationCheckpoint(
            _vertices.Count,
            _edges.Count,
            _loops.Count,
            _faces.Count);
    }

    private void RestoreRegistrationCheckpoint(
        RegistrationCheckpoint checkpoint)
    {
        RemoveAddedItems(
            _faces,
            checkpoint.FaceCount);

        RemoveAddedItems(
            _loops,
            checkpoint.LoopCount);

        RemoveAddedItems(
            _edges,
            checkpoint.EdgeCount);

        RemoveAddedItems(
            _vertices,
            checkpoint.VertexCount);
    }

    private static void RemoveAddedItems<T>(
        List<T> items,
        int originalCount)
    {
        int addedCount =
            items.Count -
            originalCount;

        if (addedCount <= 0)
        {
            return;
        }

        items.RemoveRange(
            originalCount,
            addedCount);
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

    private bool ContainsFace(Face face)
    {
        return _faces.Any(
            registeredFace =>
                ReferenceEquals(
                    registeredFace,
                    face));
    }

    private void EnsureOpen()
    {
        if (State != TopologyBuilderState.Open)
        {
            throw new InvalidOperationException(
                $"TopologyBuilder is closed. Current state: {State}.");
        }
    }

    private readonly record struct RegistrationCheckpoint(
        int VertexCount,
        int EdgeCount,
        int LoopCount,
        int FaceCount);
}