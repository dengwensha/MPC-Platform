using System;
using System.Collections.Generic;

namespace MPC.Contracts.Validation;

public static class RegionSourceValidationResultValidator
{
    public const string MissingResultCode = "CONTRACT-REGION-RESULT-001";
    public const string UnsupportedVersionCode = "CONTRACT-REGION-RESULT-002";
    public const string RequestMismatchCode = "CONTRACT-REGION-RESULT-003";
    public const string UnsupportedOperationCode = "CONTRACT-REGION-RESULT-004";
    public const string InvalidOutcomeCode = "CONTRACT-REGION-RESULT-005";

    public static ContractValidationResult Validate(
        RegionSourceValidationResult? result, string? expectedRequestId)
    {
        var diagnostics = new List<ContractDiagnostic>();
        void Error(string code, string message) =>
            diagnostics.Add(new ContractDiagnostic(code, ContractDiagnosticSeverity.Error, message));
        if (result is null)
        {
            Error(MissingResultCode, "Region source result is required.");
            return new ContractValidationResult(diagnostics);
        }
        if (!string.Equals(result.ContractVersion, ContractVersion.RegionSourceValidation,
                StringComparison.Ordinal))
            Error(UnsupportedVersionCode, "Region source result requires contract 1.1.");
        if (!Guid.TryParse(expectedRequestId, out _) ||
            !string.Equals(result.RequestId, expectedRequestId, StringComparison.Ordinal))
            Error(RequestMismatchCode, "Result identity did not match the request.");
        if (result.Operation != WorkerOperationKind.ValidateRegionSource)
            Error(UnsupportedOperationCode, "Result operation did not match region source validation.");
        if (!Enum.IsDefined(typeof(RegionSourceValidationOutcome), result.Outcome) ||
            result.Outcome == RegionSourceValidationOutcome.Unknown)
            Error(InvalidOutcomeCode, "Region source result outcome is unknown.");
        return new ContractValidationResult(diagnostics);
    }
}
