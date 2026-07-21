namespace MPC.Connectors.Step.Semantics;

public sealed class StepAxis2Placement3D
    : StepSemanticEntity
{
    public string Name { get; }

    public string LocationReferenceId { get; }

    public string AxisReferenceId { get; }

    public string ReferenceDirectionId { get; }

    public StepAxis2Placement3D(
        string? sourceEntityId,
        string name,
        string locationReferenceId,
        string axisReferenceId,
        string referenceDirectionId)
        : base(
            sourceEntityId,
            "AXIS2_PLACEMENT_3D")
    {
        if (name is null)
        {
            throw new ArgumentNullException(
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(
                locationReferenceId))
        {
            throw new ArgumentException(
                "STEP AXIS2_PLACEMENT_3D location reference cannot be empty.",
                nameof(locationReferenceId));
        }

        if (string.IsNullOrWhiteSpace(
                axisReferenceId))
        {
            throw new ArgumentException(
                "STEP AXIS2_PLACEMENT_3D axis reference cannot be empty.",
                nameof(axisReferenceId));
        }

        if (string.IsNullOrWhiteSpace(
                referenceDirectionId))
        {
            throw new ArgumentException(
                "STEP AXIS2_PLACEMENT_3D reference direction cannot be empty.",
                nameof(referenceDirectionId));
        }

        Name =
            name;

        LocationReferenceId =
            locationReferenceId;

        AxisReferenceId =
            axisReferenceId;

        ReferenceDirectionId =
            referenceDirectionId;
    }
}