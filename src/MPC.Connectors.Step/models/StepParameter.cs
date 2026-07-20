namespace MPC.Connectors.Step.Models;

public abstract class StepParameter
{
    public string RawText { get; }

    protected StepParameter(
        string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new ArgumentException(
                "STEP parameter raw text cannot be null, empty, or whitespace.",
                nameof(rawText));
        }

        RawText =
            rawText.Trim();
    }
}