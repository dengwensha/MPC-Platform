namespace MPC.Connectors.Step.Semantics;

public sealed class StepDegenerateToroidalSurfaceEntity
    : StepToroidalSurfaceEntity
{
    public bool SelectOuter { get; }

    public StepDegenerateToroidalSurfaceEntity(
        string? sourceEntityId,
        string name,
        string placementReferenceId,
        double majorRadius,
        double minorRadius,
        bool selectOuter)
        : base(
            sourceEntityId,
            "DEGENERATE_TOROIDAL_SURFACE",
            name,
            placementReferenceId,
            majorRadius,
            minorRadius)
    {
        if (majorRadius >= minorRadius)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minorRadius),
                minorRadius,
                "Minor radius must be greater than major radius for a degenerate toroidal surface.");
        }

        SelectOuter =
            selectOuter;
    }
}
