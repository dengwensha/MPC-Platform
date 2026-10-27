namespace MPC.Application.Manufacturing;

public enum RegionEvaluationRequirement
{
    Unknown = 0,
    SourceLinkOnly = 1,
    MaterialSideDependent = 2
}

public enum RegionSourceGateCode
{
    UnknownRequirement = 0,
    MissingRegion = 1,
    InvalidSourceIdentity = 2,
    DocumentMismatch = 3,
    FaceMismatch = 4,
    MaterialSideUnverified = 5,
    SourceLinkMatched = 6
}

/// <summary>Checks a region's existing import provenance only. A matched link is
/// not tool eligibility, dimensional/spatial qualification or material-side approval.</summary>
public static class RegionSourceGate
{
    public static RegionSourceGateCode Evaluate(
        ManufacturingRegion? region,
        string? expectedDocumentSha256,
        string? expectedSourceFaceId,
        RegionEvaluationRequirement requirement)
    {
        if (requirement is not (RegionEvaluationRequirement.SourceLinkOnly or
            RegionEvaluationRequirement.MaterialSideDependent))
            return RegionSourceGateCode.UnknownRequirement;
        if (region is null)
            return RegionSourceGateCode.MissingRegion;
        if (expectedDocumentSha256 is null || expectedDocumentSha256.Length != 64 ||
            expectedDocumentSha256.Any(c => !((c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))) ||
            string.IsNullOrWhiteSpace(expectedSourceFaceId))
            return RegionSourceGateCode.InvalidSourceIdentity;
        if (!StringComparer.OrdinalIgnoreCase.Equals(
                region.DocumentFingerprint, expectedDocumentSha256))
            return RegionSourceGateCode.DocumentMismatch;
        if (!StringComparer.Ordinal.Equals(region.SourceFaceId, expectedSourceFaceId))
            return RegionSourceGateCode.FaceMismatch;
        // No trusted material-side evidence producer exists at this boundary.
        if (requirement == RegionEvaluationRequirement.MaterialSideDependent)
            return RegionSourceGateCode.MaterialSideUnverified;
        return RegionSourceGateCode.SourceLinkMatched;
    }
}
