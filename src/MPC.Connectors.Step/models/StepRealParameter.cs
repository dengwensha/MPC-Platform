namespace MPC.Connectors.Step.Models;

public sealed class StepRealParameter
    : StepParameter
{
    public double Value { get; }

    public StepRealParameter(
        string rawText,
        double value)
        : base(
            rawText)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "STEP real parameter must be finite.");
        }

        Value =
            value;
    }
}