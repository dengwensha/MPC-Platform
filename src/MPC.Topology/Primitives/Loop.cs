namespace MPC.Topology.Primitives;

public sealed class Loop
{
    private readonly IReadOnlyList<EdgeUse> _edgeUses;

    public Guid Id { get; }

    public IReadOnlyList<EdgeUse> EdgeUses =>
        _edgeUses;

    public Loop(
        IEnumerable<EdgeUse> edgeUses)
    {
        ArgumentNullException.ThrowIfNull(
            edgeUses);

        EdgeUse[] edgeUseArray =
            edgeUses.ToArray();

        ValidateEdgeUses(
            edgeUseArray);

        Id =
            Guid.NewGuid();

        _edgeUses =
            Array.AsReadOnly(
                edgeUseArray);
    }

    private static void ValidateEdgeUses(
        IReadOnlyList<EdgeUse> edgeUses)
    {
        if (edgeUses.Count < 3)
        {
            throw new ArgumentException(
                "A loop must contain at least three directed edge uses.",
                nameof(edgeUses));
        }

        for (int index = 0;
             index < edgeUses.Count;
             index++)
        {
            if (edgeUses[index] is null)
            {
                throw new ArgumentException(
                    "A loop cannot contain a null edge use.",
                    nameof(edgeUses));
            }
        }

        ValidateContinuity(
            edgeUses);

        ValidateClosure(
            edgeUses);
    }

    private static void ValidateContinuity(
        IReadOnlyList<EdgeUse> edgeUses)
    {
        for (int index = 0;
             index < edgeUses.Count - 1;
             index++)
        {
            EdgeUse current =
                edgeUses[index];

            EdgeUse next =
                edgeUses[index + 1];

            if (!ReferenceEquals(
                    current.EndVertex,
                    next.StartVertex))
            {
                throw new ArgumentException(
                    $"Loop continuity is broken between edge uses " +
                    $"{index} and {index + 1}.",
                    nameof(edgeUses));
            }
        }
    }

    private static void ValidateClosure(
        IReadOnlyList<EdgeUse> edgeUses)
    {
        EdgeUse first =
            edgeUses[0];

        EdgeUse last =
            edgeUses[^1];

        if (!ReferenceEquals(
                last.EndVertex,
                first.StartVertex))
        {
            throw new ArgumentException(
                "A loop must form a closed topological cycle.",
                nameof(edgeUses));
        }
    }
}