namespace MPC.Connectors.Step.Semantics;

public sealed class StepPlaneEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PlacementReferenceId { get; }

    public StepPlaneEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId)
        : base(
            sourceEntityId,
            "PLANE")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (string.IsNullOrWhiteSpace(
                placementReferenceId))
        {
            throw new ArgumentException(
                "STEP PLANE placement reference ID cannot be null, empty, or whitespace.",
                nameof(placementReferenceId));
        }

        PlacementReferenceId =
            placementReferenceId;
    }
}