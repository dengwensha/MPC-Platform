namespace MPC.Connectors.Step.Semantics;

public sealed class StepLineEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string PointReferenceId { get; }

    public string VectorReferenceId { get; }

    public StepLineEntity(
        string? sourceEntityId,
        string name,
        string pointReferenceId,
        string vectorReferenceId)
        : base(
            sourceEntityId,
            "LINE")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (string.IsNullOrWhiteSpace(pointReferenceId))
        {
            throw new ArgumentException(
                "STEP LINE point reference ID cannot be null, empty, or whitespace.",
                nameof(pointReferenceId));
        }

        if (string.IsNullOrWhiteSpace(vectorReferenceId))
        {
            throw new ArgumentException(
                "STEP LINE vector reference ID cannot be null, empty, or whitespace.",
                nameof(vectorReferenceId));
        }

        PointReferenceId =
            pointReferenceId.Trim();

        VectorReferenceId =
            vectorReferenceId.Trim();
    }
}