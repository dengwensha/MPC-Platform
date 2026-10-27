using System;
using System.Collections.Generic;

namespace MPC.Contracts.Validation;

public static class RegionSourceValidationRequestValidator
{
    public const string MissingRequestCode = "CONTRACT-REGION-REQUEST-001";
    public const string UnsupportedVersionCode = "CONTRACT-REGION-REQUEST-002";
    public const string InvalidRequestIdCode = "CONTRACT-REGION-REQUEST-003";
    public const string UnsupportedOperationCode = "CONTRACT-REGION-REQUEST-004";
    public const string MissingInputPathCode = "CONTRACT-REGION-REQUEST-005";
    public const string InvalidFaceIdCode = "CONTRACT-REGION-REQUEST-006";
    public const string InvalidHashCode = "CONTRACT-REGION-REQUEST-007";
    public const string InvalidRequirementCode = "CONTRACT-REGION-REQUEST-008";

    public static ContractValidationResult Validate(RegionSourceValidationRequest? request)
    {
        var diagnostics = new List<ContractDiagnostic>();
        void Error(string code, string message) =>
            diagnostics.Add(new ContractDiagnostic(code, ContractDiagnosticSeverity.Error, message));
        if (request is null)
        {
            Error(MissingRequestCode, "Region source request is required.");
            return new ContractValidationResult(diagnostics);
        }
        if (!string.Equals(request.ContractVersion, ContractVersion.RegionSourceValidation,
                StringComparison.Ordinal))
            Error(UnsupportedVersionCode, "Region source validation requires contract 1.1.");
        if (!Guid.TryParse(request.RequestId, out _))
            Error(InvalidRequestIdCode, "Request identifier must be a valid GUID.");
        if (request.Operation != WorkerOperationKind.ValidateRegionSource)
            Error(UnsupportedOperationCode, "Region source operation is required.");
        if (string.IsNullOrWhiteSpace(request.InputArtifactPath))
            Error(MissingInputPathCode, "Input artifact path is required.");
        if (!IsFaceId(request.SourceFaceId))
            Error(InvalidFaceIdCode, "Source face must be a positive STEP entity identifier.");
        if (!IsSha256(request.ExpectedDocumentSha256))
            Error(InvalidHashCode, "Expected SHA-256 must contain exactly 64 ASCII hex digits.");
        if (request.Requirement != RegionSourceValidationRequirement.SourceLinkOnly &&
            request.Requirement != RegionSourceValidationRequirement.MaterialSideDependent)
            Error(InvalidRequirementCode, "Region source requirement is unknown.");
        return new ContractValidationResult(diagnostics);
    }

    private static bool IsFaceId(string? value)
    {
        if (value is null || value.Length < 2 || value[0] != '#' || value[1] < '1' || value[1] > '9')
            return false;
        for (int i = 2; i < value.Length; i++)
            if (value[i] < '0' || value[i] > '9') return false;
        return true;
    }

    private static bool IsSha256(string? value)
    {
        if (value is null || value.Length != 64) return false;
        foreach (char c in value)
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F'))) return false;
        return true;
    }
}
