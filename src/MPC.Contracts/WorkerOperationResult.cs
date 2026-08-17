using System;
using System.Collections.Generic;
using System.Linq;

namespace MPC.Contracts;

public sealed class WorkerOperationResult
{
    private readonly IReadOnlyList<ContractDiagnostic> _diagnostics;

    public string? ContractVersion { get; }

    public string? RequestId { get; }

    public WorkerOperationStatus Status { get; }

    public string? OutputArtifactPath { get; }

    public IReadOnlyList<ContractDiagnostic> Diagnostics => _diagnostics;

    public WorkerOperationResult(
        string? contractVersion,
        string? requestId,
        WorkerOperationStatus status,
        string? outputArtifactPath,
        IReadOnlyList<ContractDiagnostic>? diagnostics)
    {
        if (!Enum.IsDefined(typeof(WorkerOperationStatus), status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Worker operation status is not defined.");
        }

        var diagnosticArray = diagnostics?.ToArray()
            ?? Array.Empty<ContractDiagnostic>();

        if (diagnosticArray.Any(diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "Contract diagnostics cannot contain null entries.",
                nameof(diagnostics));
        }

        ContractVersion = contractVersion;
        RequestId = requestId;
        Status = status;
        OutputArtifactPath = NormalizeOptional(outputArtifactPath);
        _diagnostics = Array.AsReadOnly(diagnosticArray);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
