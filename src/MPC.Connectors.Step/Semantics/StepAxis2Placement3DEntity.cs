namespace MPC.Connectors.Step.Semantics;

public sealed class StepAxis2Placement3DEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string LocationReferenceId { get; }

    public string? AxisReferenceId { get; }

    public string? RefDirectionReferenceId { get; }

    public StepAxis2Placement3DEntity(
        string? sourceEntityId,
        string name,
        string locationReferenceId,
        string? axisReferenceId,
        string? refDirectionReferenceId)
        : base(
            sourceEntityId,
            "AXIS2_PLACEMENT_3D")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (string.IsNullOrWhiteSpace(
                locationReferenceId))
        {
            throw new ArgumentException(
                "STEP AXIS2_PLACEMENT_3D location reference ID cannot be null, empty, or whitespace.",
                nameof(locationReferenceId));
        }

        LocationReferenceId =
            locationReferenceId.Trim();

        AxisReferenceId =
            NormalizeOptionalReferenceId(
                axisReferenceId);

        RefDirectionReferenceId =
            NormalizeOptionalReferenceId(
                refDirectionReferenceId);
    }

    private static string?
        NormalizeOptionalReferenceId(
            string? referenceId)
    {
        return string.IsNullOrWhiteSpace(
                referenceId)
            ? null
            : referenceId.Trim();
    }
}