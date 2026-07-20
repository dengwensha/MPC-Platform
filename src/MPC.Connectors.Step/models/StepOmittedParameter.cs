namespace MPC.Connectors.Step.Models;

public sealed class StepOmittedParameter
    : StepParameter
{
    public StepOmittedParameter(
        string rawText)
        : base(
            rawText)
    {
        if (RawText != "*")
        {
            throw new ArgumentException(
                "STEP omitted parameter raw text must be '*'.",
                nameof(rawText));
        }
    }
}