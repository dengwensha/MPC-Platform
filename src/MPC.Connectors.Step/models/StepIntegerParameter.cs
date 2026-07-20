namespace MPC.Connectors.Step.Models;

public sealed class StepIntegerParameter
    : StepParameter
{
    public long Value { get; }

    public StepIntegerParameter(
        string rawText,
        long value)
        : base(
            rawText)
    {
        Value =
            value;
    }
}