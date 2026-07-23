namespace MPC.Connectors.Step.Semantics;

public sealed class StepOrientedEdgeEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string EdgeElementReferenceId { get; }

    public bool Orientation { get; }

    public StepOrientedEdgeEntity(
        string? sourceEntityId,
        string name,
        string edgeElementReferenceId,
        bool orientation)
        : base(
            sourceEntityId,
            "ORIENTED_EDGE")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        if (string.IsNullOrWhiteSpace(
                edgeElementReferenceId))
        {
            throw new ArgumentException(
                "Edge element reference ID cannot be null, empty, or whitespace.",
                nameof(edgeElementReferenceId));
        }

        Name =
            name;

        EdgeElementReferenceId =
            edgeElementReferenceId;

        Orientation =
            orientation;
    }
}