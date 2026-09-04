namespace MPC.Connectors.Step.Semantics;

public class StepToroidalSurfaceEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PlacementReferenceId { get; }

    public double MajorRadius { get; }

    public double MinorRadius { get; }

    public StepToroidalSurfaceEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId,
        double majorRadius,
        double minorRadius)
        : this(
            sourceEntityId,
            "TOROIDAL_SURFACE",
            name,
            placementReferenceId,
            majorRadius,
            minorRadius)
    {
    }

    protected StepToroidalSurfaceEntity(
        string? sourceEntityId,
        string sourceEntityType,
        string name,
        string placementReferenceId,
        double majorRadius,
        double minorRadius)
        : base(
            sourceEntityId,
            sourceEntityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            sourceEntityType);

        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(placementReferenceId))
        {
            throw new ArgumentException(
                "Placement reference ID cannot be null, empty, or whitespace.",
                nameof(placementReferenceId));
        }

        if (!double.IsFinite(majorRadius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(majorRadius),
                majorRadius,
                "Major radius must be finite.");
        }

        if (majorRadius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(majorRadius),
                majorRadius,
                "Major radius must be greater than zero.");
        }

        if (!double.IsFinite(minorRadius))
        {
            throw new ArgumentOutOfRangeException(
                nameof(minorRadius),
                minorRadius,
                "Minor radius must be finite.");
        }

        if (minorRadius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minorRadius),
                minorRadius,
                "Minor radius must be greater than zero.");
        }

        Name =
            name;

        PlacementReferenceId =
            placementReferenceId;

        MajorRadius =
            majorRadius;

        MinorRadius =
            minorRadius;
    }
}
