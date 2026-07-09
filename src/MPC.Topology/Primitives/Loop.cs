namespace MPC.Topology.Primitives;

public sealed class Loop
{
    private readonly IReadOnlyList<Edge> _edges;

    public IReadOnlyList<Edge> Edges => _edges;

    public int Count => _edges.Count;

    public Loop(IEnumerable<Edge> edges)
    {
        if (edges is null)
            throw new ArgumentNullException(nameof(edges));

        var list = edges.ToList();

        if (list.Count < 3)
            throw new ArgumentException("A loop must contain at least three edges.", nameof(edges));

        ValidateConnectedAndClosed(list);

        _edges = list.AsReadOnly();
    }

    private static void ValidateConnectedAndClosed(IReadOnlyList<Edge> edges)
    {
        for (var i = 0; i < edges.Count; i++)
        {
            var current = edges[i];
            var next = edges[(i + 1) % edges.Count];

            if (!Equals(current.EndVertex, next.StartVertex))
            {
                throw new ArgumentException(
                    "Edges must form a continuous closed loop.",
                    nameof(edges));
            }
        }
    }
}