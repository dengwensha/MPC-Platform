namespace MPC.Connectors.Step.Models;

public sealed class StepStringParameter
    : StepParameter
{
    public string Value { get; }

    public StepStringParameter(
        string rawText,
        string value)
        : base(
            rawText)
    {
        Value =
            value ??
            throw new ArgumentNullException(
                nameof(value));
    }
}