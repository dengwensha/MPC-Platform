namespace MPC.Contracts;

public enum RegionSourceValidationRequirement
{
    Unknown = 0,
    SourceLinkOnly = 1,
    MaterialSideDependent = 2
}

// Separate wire DTO: existing WorkerOperationRequest constructor is unchanged.
// SourceFaceId identifies the requested STEP face within the expected byte snapshot.
public sealed class RegionSourceValidationRequest
{
    public string? ContractVersion { get; }
    public string? RequestId { get; }
    public WorkerOperationKind Operation { get; }
    public string? InputArtifactPath { get; }
    public string? SourceFaceId { get; }
    public string? ExpectedDocumentSha256 { get; }
    public RegionSourceValidationRequirement Requirement { get; }

    public RegionSourceValidationRequest(string? contractVersion, string? requestId,
        WorkerOperationKind operation, string? inputArtifactPath, string? sourceFaceId,
        string? expectedDocumentSha256, RegionSourceValidationRequirement requirement)
    {
        ContractVersion = contractVersion;
        RequestId = requestId;
        Operation = operation;
        InputArtifactPath = inputArtifactPath;
        SourceFaceId = sourceFaceId;
        ExpectedDocumentSha256 = expectedDocumentSha256;
        Requirement = requirement;
    }
}
