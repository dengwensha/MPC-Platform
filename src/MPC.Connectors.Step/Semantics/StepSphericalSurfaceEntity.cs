namespace MPC.Connectors.Step.Semantics;

public sealed class StepSphericalSurfaceEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PlacementReferenceId { get; }

    public double Radius { get; }

    public StepSphericalSurfaceEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId,
        double radius)
        : base(
            sourceEntityId,
            "SPHERICAL_SURFACE")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        if (string.IsNullOrWhiteSpace(
                placementReferenceId))
        {
            throw new ArgumentException(
                "Placement reference ID cannot be null, empty, or whitespace.",
                nameof(placementReferenceId));
        }

        if (!double.IsFinite(
                radius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Radius must be finite.");
        }

        if (radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Radius must be greater than zero.");
        }

        Name =
            name;

        PlacementReferenceId =
            placementReferenceId;

        Radius =
            radius;
    }
}