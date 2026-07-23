namespace MPC.Connectors.Step.Semantics;

public sealed class StepEdgeLoopEntity
    : StepSemanticEntity
{
    private readonly IReadOnlyList<string>
        _orientedEdgeReferenceIds;

    public string Name { get; }

    public IReadOnlyList<string> OrientedEdgeReferenceIds =>
        _orientedEdgeReferenceIds;

    public StepEdgeLoopEntity(
        string? sourceEntityId,
        string name,
        IEnumerable<string> orientedEdgeReferenceIds)
        : base(
            sourceEntityId,
            "EDGE_LOOP")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        ArgumentNullException.ThrowIfNull(
            orientedEdgeReferenceIds);

        var referenceIds =
            orientedEdgeReferenceIds.ToList();

        if (referenceIds.Count == 0)
        {
            throw new ArgumentException(
                "EDGE_LOOP must contain at least one oriented edge reference.",
                nameof(orientedEdgeReferenceIds));
        }

        if (referenceIds.Any(
                referenceId =>
                    string.IsNullOrWhiteSpace(
                        referenceId)))
        {
            throw new ArgumentException(
                "EDGE_LOOP oriented edge reference IDs cannot contain null, empty, or whitespace values.",
                nameof(orientedEdgeReferenceIds));
        }

        Name =
            name;

        _orientedEdgeReferenceIds =
            referenceIds.AsReadOnly();
    }
}