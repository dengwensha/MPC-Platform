namespace MPC.Connectors.Step.Semantics;

public sealed class StepConicalSurfaceEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PlacementReferenceId { get; }

    public double Radius { get; }

    public double SemiAngle { get; }

    public StepConicalSurfaceEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId,
        double radius,
        double semiAngle)
        : base(
            sourceEntityId,
            "CONICAL_SURFACE")
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

        if (radius < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Radius cannot be negative.");
        }

        if (!double.IsFinite(
                semiAngle))
        {
            throw new ArgumentOutOfRangeException(
                nameof(semiAngle),
                semiAngle,
                "Semi-angle must be finite.");
        }

        Name =
            name;

        PlacementReferenceId =
            placementReferenceId;

        Radius =
            radius;

        SemiAngle =
            semiAngle;
    }
}