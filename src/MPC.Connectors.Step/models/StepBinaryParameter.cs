namespace MPC.Connectors.Step.Models;

public sealed class StepBinaryParameter
    : StepParameter
{
    public string EncodedValue { get; }

    public StepBinaryParameter(
        string rawText,
        string encodedValue)
        : base(
            rawText)
    {
        if (string.IsNullOrEmpty(
                encodedValue))
        {
            throw new ArgumentException(
                "STEP binary encoded value cannot be null or empty.",
                nameof(encodedValue));
        }

        EncodedValue =
            encodedValue;
    }
}