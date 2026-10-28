using MPC.Application.Manufacturing;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Worker;

public sealed class RegionSourceValidationProcessor
{
    public RegionSourceValidationResult Process(RegionSourceValidationRequest? request)
    {
        // No file access before structural contract validation.
        if (!RegionSourceValidationRequestValidator.Validate(request).IsValid)
            return Result(request, RegionSourceValidationOutcome.RequestRejected);
        try
        {
            var requirement = request!.Requirement switch
            {
                RegionSourceValidationRequirement.SourceLinkOnly =>
                    RegionEvaluationRequirement.SourceLinkOnly,
                RegionSourceValidationRequirement.MaterialSideDependent =>
                    RegionEvaluationRequirement.MaterialSideDependent,
                _ => RegionEvaluationRequirement.Unknown
            };
            var code = ValidatedRegionImportService.Execute(
                request.InputArtifactPath!, request.SourceFaceId!,
                request.ExpectedDocumentSha256, request.SourceFaceId, requirement, out _);
            var outcome = code switch
            {
                RegionSourceGateCode.SourceLinkMatched => RegionSourceValidationOutcome.SourceLinkMatched,
                RegionSourceGateCode.MissingRegion => RegionSourceValidationOutcome.MissingRegion,
                RegionSourceGateCode.InvalidSourceIdentity => RegionSourceValidationOutcome.InvalidSourceIdentity,
                RegionSourceGateCode.DocumentMismatch => RegionSourceValidationOutcome.DocumentMismatch,
                RegionSourceGateCode.FaceMismatch => RegionSourceValidationOutcome.FaceMismatch,
                RegionSourceGateCode.MaterialSideUnverified => RegionSourceValidationOutcome.MaterialSideUnverified,
                _ => RegionSourceValidationOutcome.ImportFailed
            };
            return Result(request, outcome);
        }
        catch (Exception exception) when (exception is IOException or
            UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return Result(request, RegionSourceValidationOutcome.ImportFailed);
        }
    }

    private static RegionSourceValidationResult Result(
        RegionSourceValidationRequest? request, RegionSourceValidationOutcome outcome) =>
        new(ContractVersion.RegionSourceValidation, request?.RequestId,
            WorkerOperationKind.ValidateRegionSource, outcome);
}
