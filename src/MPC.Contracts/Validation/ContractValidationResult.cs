using System;
using System.Collections.Generic;
using System.Linq;

namespace MPC.Contracts.Validation;

public sealed class ContractValidationResult
{
    private readonly IReadOnlyList<ContractDiagnostic> _diagnostics;

    public bool IsValid => _diagnostics.Count == 0;

    public IReadOnlyList<ContractDiagnostic> Diagnostics => _diagnostics;

    public ContractValidationResult(
        IEnumerable<ContractDiagnostic>? diagnostics)
    {
        var diagnosticArray = diagnostics?.ToArray()
            ?? Array.Empty<ContractDiagnostic>();

        if (diagnosticArray.Any(diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "Validation diagnostics cannot contain null entries.",
                nameof(diagnostics));
        }

        _diagnostics = Array.AsReadOnly(diagnosticArray);
    }
}
