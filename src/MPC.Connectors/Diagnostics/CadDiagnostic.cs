namespace MPC.Connectors.Diagnostics;

public sealed class CadDiagnostic
{
    public string Code { get; }

    public CadDiagnosticSeverity Severity { get; }

    public string Message { get; }

    public string? SourceEntityId { get; }

    public string? SourceEntityType { get; }

    public string? AffectedField { get; }

    public string? ExceptionType { get; }

    public CadDiagnostic(
        string code,
        CadDiagnosticSeverity severity,
        string message,
        string? sourceEntityId = null,
        string? sourceEntityType = null,
        string? affectedField = null,
        string? exceptionType = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Diagnostic code cannot be null, empty, or whitespace.",
                nameof(code));
        }

        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "Diagnostic severity is not defined.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Diagnostic message cannot be null, empty, or whitespace.",
                nameof(message));
        }

        Code = code.Trim();
        Severity = severity;
        Message = message.Trim();
        SourceEntityId = NormalizeOptional(sourceEntityId);
        SourceEntityType = NormalizeOptional(sourceEntityType);
        AffectedField = NormalizeOptional(affectedField);
        ExceptionType = NormalizeOptional(exceptionType);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
