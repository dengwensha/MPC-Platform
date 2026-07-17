namespace MPC.Topology.Primitives;

public sealed class EdgeUse
{
    public Edge Edge { get; }

    public EdgeUseDirection Direction { get; }

    public Vertex StartVertex =>
        Direction == EdgeUseDirection.Forward
            ? Edge.StartVertex
            : Edge.EndVertex;

    public Vertex EndVertex =>
        Direction == EdgeUseDirection.Forward
            ? Edge.EndVertex
            : Edge.StartVertex;

    public EdgeUse(
        Edge edge,
        EdgeUseDirection direction = EdgeUseDirection.Forward)
    {
        Edge = edge ?? throw new ArgumentNullException(nameof(edge));

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Unsupported edge-use direction.");
        }

        Direction = direction;
    }
}