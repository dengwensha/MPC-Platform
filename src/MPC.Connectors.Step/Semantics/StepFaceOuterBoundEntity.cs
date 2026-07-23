namespace MPC.Connectors.Step.Semantics;

public sealed class StepFaceOuterBoundEntity
    : StepFaceBoundEntity
{
    public StepFaceOuterBoundEntity(
        string? sourceEntityId,
        string name,
        string loopReferenceId,
        bool orientation)
        : base(
            sourceEntityId,
            "FACE_OUTER_BOUND",
            name,
            loopReferenceId,
            orientation)
    {
    }
}