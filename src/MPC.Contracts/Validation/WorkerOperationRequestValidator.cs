using System;
using System.Collections.Generic;

namespace MPC.Contracts.Validation;

public static class WorkerOperationRequestValidator
{
    public const string MissingRequestCode =
        "CONTRACT-REQUEST-001";

    public const string UnsupportedVersionCode =
        "CONTRACT-REQUEST-002";

    public const string InvalidRequestIdCode =
        "CONTRACT-REQUEST-003";

    public const string UnsupportedOperationCode =
        "CONTRACT-REQUEST-004";

    public const string MissingInputArtifactPathCode =
        "CONTRACT-REQUEST-005";

    public static ContractValidationResult Validate(
        WorkerOperationRequest? request)
    {
        var diagnostics = new List<ContractDiagnostic>();

        if (request is null)
        {
            diagnostics.Add(
                Error(
                    MissingRequestCode,
                    "Worker operation request is required."));

            return new ContractValidationResult(diagnostics);
        }

        if (!ContractVersion.IsSupported(request.ContractVersion))
        {
            diagnostics.Add(
                Error(
                    UnsupportedVersionCode,
                    "Contract version is missing or unsupported."));
        }

        if (!Guid.TryParse(request.RequestId, out _))
        {
            diagnostics.Add(
                Error(
                    InvalidRequestIdCode,
                    "Request identifier must be a valid GUID."));
        }

        if (!Enum.IsDefined(
                typeof(WorkerOperationKind),
                request.Operation))
        {
            diagnostics.Add(
                Error(
                    UnsupportedOperationCode,
                    "Worker operation is not supported."));
        }

        if (string.IsNullOrWhiteSpace(request.InputArtifactPath))
        {
            diagnostics.Add(
                Error(
                    MissingInputArtifactPathCode,
                    "Input artifact path is required."));
        }

        return new ContractValidationResult(diagnostics);
    }

    private static ContractDiagnostic Error(
        string code,
        string message)
    {
        return new ContractDiagnostic(
            code,
            ContractDiagnosticSeverity.Error,
            message);
    }
}
