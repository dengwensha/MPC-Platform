namespace MPC.Connectors.Step.Semantics;

public sealed class StepVectorEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string OrientationReferenceId { get; }

    public double Magnitude { get; }

    public StepVectorEntity(
        string? sourceEntityId,
        string name,
        string orientationReferenceId,
        double magnitude)
        : base(
            sourceEntityId,
            "VECTOR")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (string.IsNullOrWhiteSpace(
                orientationReferenceId))
        {
            throw new ArgumentException(
                "STEP VECTOR orientation reference ID cannot be null, empty, or whitespace.",
                nameof(orientationReferenceId));
        }

        if (!double.IsFinite(
                magnitude))
        {
            throw new ArgumentOutOfRangeException(
                nameof(magnitude),
                "STEP VECTOR magnitude must be finite.");
        }

        OrientationReferenceId =
            orientationReferenceId.Trim();

        Magnitude =
            magnitude;
    }
}