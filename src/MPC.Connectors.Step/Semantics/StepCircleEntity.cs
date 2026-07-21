namespace MPC.Connectors.Step.Semantics;

public sealed class StepCircleEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PlacementReferenceId { get; }

    public double Radius { get; }

    public StepCircleEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId,
        double radius)
        : base(
            sourceEntityId,
            "CIRCLE")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (string.IsNullOrWhiteSpace(
                placementReferenceId))
        {
            throw new ArgumentException(
                "STEP CIRCLE placement reference ID cannot be null, empty, or whitespace.",
                nameof(placementReferenceId));
        }

        if (!double.IsFinite(
                radius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                "STEP CIRCLE radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                "STEP CIRCLE radius must be greater than zero.");
        }

        PlacementReferenceId =
            placementReferenceId.Trim();

        Radius =
            radius;
    }
}