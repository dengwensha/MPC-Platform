namespace MPC.Connectors.Step.Semantics;

public sealed class StepCylindricalSurfaceEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PlacementReferenceId { get; }

    public double Radius { get; }

    public StepCylindricalSurfaceEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId,
        double radius)
        : base(
            sourceEntityId,
            "CYLINDRICAL_SURFACE")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (string.IsNullOrWhiteSpace(
                placementReferenceId))
        {
            throw new ArgumentException(
                "STEP CYLINDRICAL_SURFACE placement reference ID cannot be null, empty, or whitespace.",
                nameof(placementReferenceId));
        }

        if (!double.IsFinite(
                radius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                "STEP CYLINDRICAL_SURFACE radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                "STEP CYLINDRICAL_SURFACE radius must be greater than zero.");
        }

        PlacementReferenceId =
            placementReferenceId;

        Radius =
            radius;
    }
}