namespace MPC.Application.Step;

public sealed class StepImportDiagnostic
{
    public string Code { get; }

    public StepImportDiagnosticSeverity Severity { get; }

    public string Message { get; }

    public string? SourceEntityId { get; }

    public StepImportDiagnostic(
        string code,
        StepImportDiagnosticSeverity severity,
        string message,
        string? sourceEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "STEP import diagnostic code cannot be null, empty, or whitespace.",
                nameof(code));
        }

        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "STEP import diagnostic severity is not defined.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "STEP import diagnostic message cannot be null, empty, or whitespace.",
                nameof(message));
        }

        Code = code.Trim();
        Severity = severity;
        Message = message.Trim();
        SourceEntityId = NormalizeOptional(sourceEntityId);
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
