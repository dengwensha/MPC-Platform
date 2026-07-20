namespace MPC.Connectors.Step.Models;

public sealed class StepEnumerationParameter
    : StepParameter
{
    public string Name { get; }

    public StepEnumerationParameter(
        string rawText,
        string name)
        : base(
            rawText)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "STEP enumeration name cannot be null, empty, or whitespace.",
                nameof(name));
        }

        var normalizedName =
            name
                .Trim()
                .ToUpperInvariant();

        if (!IsValidName(
                normalizedName))
        {
            throw new ArgumentException(
                "STEP enumeration name must be a valid STEP identifier.",
                nameof(name));
        }

        Name =
            normalizedName;
    }

    private static bool IsValidName(
        string value)
    {
        if (value.Length == 0 ||
            !char.IsLetter(
                value[0]))
        {
            return false;
        }

        for (var index = 1;
             index < value.Length;
             index++)
        {
            var character =
                value[index];

            if (!char.IsLetterOrDigit(
                    character) &&
                character != '_')
            {
                return false;
            }
        }

        return true;
    }
}