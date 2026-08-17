using System;

namespace MPC.Contracts;

public sealed class ContractDiagnostic
{
    public string Code { get; }

    public ContractDiagnosticSeverity Severity { get; }

    public string Message { get; }

    public ContractDiagnostic(
        string code,
        ContractDiagnosticSeverity severity,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Contract diagnostic code cannot be null, empty, or whitespace.",
                nameof(code));
        }

        if (!Enum.IsDefined(typeof(ContractDiagnosticSeverity), severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "Contract diagnostic severity is not defined.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Contract diagnostic message cannot be null, empty, or whitespace.",
                nameof(message));
        }

        Code = code.Trim();
        Severity = severity;
        Message = message.Trim();
    }
}
