namespace MPC.Mapping;

public sealed class MappingDiagnostic
{
    public MappingDiagnostic(
        string code,
        string message,
        MappingDiagnosticSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Mapping diagnostic code cannot be null, empty, or whitespace.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Mapping diagnostic message cannot be null, empty, or whitespace.",
                nameof(message));
        }

        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "Mapping diagnostic severity is not defined.");
        }

        Code = code;
        Message = message;
        Severity = severity;
    }

    public string Code { get; }

    public string Message { get; }

    public MappingDiagnosticSeverity Severity { get; }
}