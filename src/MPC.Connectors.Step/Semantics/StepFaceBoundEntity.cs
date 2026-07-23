namespace MPC.Connectors.Step.Semantics;

public class StepFaceBoundEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string LoopReferenceId { get; }

    public bool Orientation { get; }

    public StepFaceBoundEntity(
        string? sourceEntityId,
        string name,
        string loopReferenceId,
        bool orientation)
        : this(
            sourceEntityId,
            "FACE_BOUND",
            name,
            loopReferenceId,
            orientation)
    {
    }

    protected StepFaceBoundEntity(
        string? sourceEntityId,
        string sourceEntityType,
        string name,
        string loopReferenceId,
        bool orientation)
        : base(
            sourceEntityId,
            sourceEntityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            sourceEntityType);

        ArgumentNullException.ThrowIfNull(
            name);

        if (string.IsNullOrWhiteSpace(
                loopReferenceId))
        {
            throw new ArgumentException(
                "Loop reference ID cannot be null, empty, or whitespace.",
                nameof(loopReferenceId));
        }

        Name =
            name;

        LoopReferenceId =
            loopReferenceId;

        Orientation =
            orientation;
    }
}