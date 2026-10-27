namespace MPC.Application.Manufacturing;

/// <summary>Composes the existing snapshot import and bounded source-link gate.
/// File/permission and unexpected import exceptions propagate to the caller.
/// A returned region certifies only source linkage, never machining suitability.</summary>
public static class ValidatedRegionImportService
{
    public static RegionSourceGateCode Execute(
        string inputFilePath,
        string sourceFaceId,
        string? expectedDocumentSha256,
        string? expectedSourceFaceId,
        RegionEvaluationRequirement requirement,
        out ManufacturingRegion? region)
    {
        region = null;
        if (requirement is not (RegionEvaluationRequirement.SourceLinkOnly or
            RegionEvaluationRequirement.MaterialSideDependent))
            return RegionSourceGateCode.UnknownRequirement;

        if (!ManufacturingRegionImportService.TryCreate(
                inputFilePath, sourceFaceId, out var imported))
            return RegionSourceGateCode.MissingRegion;

        var code = RegionSourceGate.Evaluate(imported,
            expectedDocumentSha256, expectedSourceFaceId, requirement);
        if (code == RegionSourceGateCode.SourceLinkMatched)
            region = imported;
        return code;
    }
}
