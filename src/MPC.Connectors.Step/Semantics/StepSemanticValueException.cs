namespace MPC.Connectors.Step.Semantics;

public sealed class StepSemanticValueException
    : Exception
{
    public string? SourceEntityId { get; }

    public string SourceEntityType { get; }

    public string ParameterName { get; }

    public string RejectedValue { get; }

    public StepSemanticValueException(
        string? sourceEntityId,
        string sourceEntityType,
        string parameterName,
        string rejectedValue,
        Exception innerException)
        : base(
            "A STEP semantic entity value was rejected.",
            innerException)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityType))
        {
            throw new ArgumentException(
                "STEP semantic entity type cannot be null, empty, or whitespace.",
                nameof(sourceEntityType));
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(
                "STEP semantic parameter name cannot be null, empty, or whitespace.",
                nameof(parameterName));
        }

        if (string.IsNullOrWhiteSpace(rejectedValue))
        {
            throw new ArgumentException(
                "Rejected STEP semantic value cannot be null, empty, or whitespace.",
                nameof(rejectedValue));
        }

        ArgumentNullException.ThrowIfNull(innerException);

        SourceEntityId =
            NormalizeOptional(sourceEntityId);

        SourceEntityType =
            sourceEntityType.Trim();

        ParameterName =
            parameterName.Trim();

        RejectedValue =
            rejectedValue.Trim();
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}