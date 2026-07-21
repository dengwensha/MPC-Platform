namespace MPC.Connectors.Step.Semantics;

public sealed class StepPlane
    : StepSemanticEntity
{
    public string Name { get; }

    public string PositionReferenceId { get; }

    public StepPlane(
        string? sourceEntityId,
        string name,
        string positionReferenceId)
        : base(
            sourceEntityId,
            "PLANE")
    {
        if (name is null)
        {
            throw new ArgumentNullException(
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(
                positionReferenceId))
        {
            throw new ArgumentException(
                "STEP plane position reference cannot be empty.",
                nameof(positionReferenceId));
        }

        Name =
            name;

        PositionReferenceId =
            positionReferenceId;
    }
}