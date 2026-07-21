namespace MPC.Connectors.Step.Semantics;

public sealed class StepCylindricalSurface
    : StepSemanticEntity
{
    public string Name { get; }

    public string PositionReferenceId { get; }

    public double Radius { get; }

    public StepCylindricalSurface(
        string? sourceEntityId,
        string name,
        string positionReferenceId,
        double radius)
        : base(
            sourceEntityId,
            "CYLINDRICAL_SURFACE")
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
                "STEP cylindrical surface position reference cannot be empty.",
                nameof(positionReferenceId));
        }

        if (double.IsNaN(radius) ||
            double.IsInfinity(radius) ||
            radius <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "STEP cylindrical surface radius must be finite and greater than zero.");
        }

        Name =
            name;

        PositionReferenceId =
            positionReferenceId;

        Radius =
            radius;
    }
}