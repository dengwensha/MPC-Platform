namespace MPC.Connectors.Step.Models;

public sealed class StepNullParameter
    : StepParameter
{
    public StepNullParameter(
        string rawText)
        : base(
            rawText)
    {
        if (RawText != "$")
        {
            throw new ArgumentException(
                "STEP null parameter raw text must be '$'.",
                nameof(rawText));
        }
    }
}