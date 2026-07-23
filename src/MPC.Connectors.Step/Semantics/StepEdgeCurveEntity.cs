namespace MPC.Connectors.Step.Semantics;

public sealed class StepEdgeCurveEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string EdgeStartReferenceId { get; }

    public string EdgeEndReferenceId { get; }

    public string EdgeGeometryReferenceId { get; }

    public bool SameSense { get; }

    public StepEdgeCurveEntity(
        string? sourceEntityId,
        string name,
        string edgeStartReferenceId,
        string edgeEndReferenceId,
        string edgeGeometryReferenceId,
        bool sameSense)
        : base(
            sourceEntityId,
            "EDGE_CURVE")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        if (string.IsNullOrWhiteSpace(
                edgeStartReferenceId))
        {
            throw new ArgumentException(
                "Edge start reference ID cannot be null, empty, or whitespace.",
                nameof(edgeStartReferenceId));
        }

        if (string.IsNullOrWhiteSpace(
                edgeEndReferenceId))
        {
            throw new ArgumentException(
                "Edge end reference ID cannot be null, empty, or whitespace.",
                nameof(edgeEndReferenceId));
        }

        if (string.IsNullOrWhiteSpace(
                edgeGeometryReferenceId))
        {
            throw new ArgumentException(
                "Edge geometry reference ID cannot be null, empty, or whitespace.",
                nameof(edgeGeometryReferenceId));
        }

        Name =
            name;

        EdgeStartReferenceId =
            edgeStartReferenceId;

        EdgeEndReferenceId =
            edgeEndReferenceId;

        EdgeGeometryReferenceId =
            edgeGeometryReferenceId;

        SameSense =
            sameSense;
    }
}