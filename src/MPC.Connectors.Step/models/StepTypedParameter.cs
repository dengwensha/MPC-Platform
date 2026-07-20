namespace MPC.Connectors.Step.Models;

public sealed class StepTypedParameter
    : StepParameter
{
    private readonly IReadOnlyList<StepParameter>
        _parameters;

    public string TypeName { get; }

    public IReadOnlyList<StepParameter> Parameters =>
        _parameters;

    public StepTypedParameter(
        string rawText,
        string typeName,
        IEnumerable<StepParameter> parameters)
        : base(
            rawText)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(
                "STEP typed parameter name cannot be null, empty, or whitespace.",
                nameof(typeName));
        }

        if (parameters is null)
        {
            throw new ArgumentNullException(
                nameof(parameters));
        }

        var parameterList =
            parameters.ToList();

        if (parameterList.Any(
                parameter =>
                    parameter is null))
        {
            throw new ArgumentException(
                "STEP typed parameter children cannot contain null entries.",
                nameof(parameters));
        }

        var normalizedTypeName =
            typeName
                .Trim()
                .ToUpperInvariant();

        if (!IsValidTypeName(
                normalizedTypeName))
        {
            throw new ArgumentException(
                "STEP typed parameter name must be a valid STEP identifier.",
                nameof(typeName));
        }

        TypeName =
            normalizedTypeName;

        _parameters =
            parameterList.AsReadOnly();
    }

    private static bool IsValidTypeName(
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