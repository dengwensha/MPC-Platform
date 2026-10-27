namespace MPC.Contracts;

// SourceLinkMatched means byte-snapshot/face linkage only, never machining eligibility.
public enum RegionSourceValidationOutcome
{
    Unknown = 0,
    SourceLinkMatched = 1,
    MissingRegion = 2,
    InvalidSourceIdentity = 3,
    DocumentMismatch = 4,
    FaceMismatch = 5,
    MaterialSideUnverified = 6,
    ImportFailed = 7
}

// Validate after deserialization before consuming any outcome.
// No geometry, material-side approval or manufacturing-success boolean is transported.
public sealed class RegionSourceValidationResult
{
    public string? ContractVersion { get; }
    public string? RequestId { get; }
    public WorkerOperationKind Operation { get; }
    public RegionSourceValidationOutcome Outcome { get; }

    public RegionSourceValidationResult(string? contractVersion, string? requestId,
        WorkerOperationKind operation, RegionSourceValidationOutcome outcome)
    {
        ContractVersion = contractVersion;
        RequestId = requestId;
        Operation = operation;
        Outcome = outcome;
    }
}
