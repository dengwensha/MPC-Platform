using MPC.Application.Manufacturing;

namespace MPC.Application.Tests.Manufacturing;

public sealed class RegionSourceGateTests
{
    private static ManufacturingRegion Import()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            string file = Path.Combine(current.FullName, "tests", "MPC.Tests",
                "TestData", "Step", "Golden", "T001_PRISM.step");
            if (File.Exists(file))
            {
                Assert.True(ManufacturingRegionImportService.TryCreate(file, "#8", out var region));
                return Assert.IsType<ManufacturingRegion>(region);
            }
            current = current.Parent;
        }
        throw new FileNotFoundException("T001_PRISM.step");
    }

    [Fact]
    public void RealImport_MatchesExactSourcePairAcrossImportSessions()
    {
        var first = Import();
        var again = Import();
        Assert.NotEqual(first.CanonicalFaceId, again.CanonicalFaceId);
        Assert.Equal(RegionSourceGateCode.SourceLinkMatched, RegionSourceGate.Evaluate(
            again, first.DocumentFingerprint.ToLowerInvariant(), first.SourceFaceId,
            RegionEvaluationRequirement.SourceLinkOnly));
    }

    [Fact]
    public void CorrectSourcePair_CannotAuthorizeMaterialSide()
    {
        var region = Import();
        Assert.Equal(RegionSourceGateCode.MaterialSideUnverified, RegionSourceGate.Evaluate(
            region, region.DocumentFingerprint, region.SourceFaceId,
            RegionEvaluationRequirement.MaterialSideDependent));
    }

    [Fact]
    public void DifferentDocument_IsRejected()
    {
        var region = Import();
        string changed = (region.DocumentFingerprint[0] == 'A' ? "B" : "A") +
            region.DocumentFingerprint[1..];
        Assert.Equal(RegionSourceGateCode.DocumentMismatch, RegionSourceGate.Evaluate(
            region, changed, region.SourceFaceId, RegionEvaluationRequirement.SourceLinkOnly));
    }

    [Theory]
    [InlineData("#30")]
    [InlineData("#8 ")]
    public void DifferentOrNormalizedFaceId_IsRejected(string face)
    {
        var region = Import();
        Assert.Equal(RegionSourceGateCode.FaceMismatch, RegionSourceGate.Evaluate(
            region, region.DocumentFingerprint, face, RegionEvaluationRequirement.SourceLinkOnly));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("GA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD")]
    public void MalformedFingerprint_IsRejected(string? hash)
    {
        var region = Import();
        Assert.Equal(RegionSourceGateCode.InvalidSourceIdentity, RegionSourceGate.Evaluate(
            region, hash, "#8", RegionEvaluationRequirement.SourceLinkOnly));
    }

    [Fact]
    public void MissingFace_IsRejected()
    {
        var region = Import();
        Assert.Equal(RegionSourceGateCode.InvalidSourceIdentity, RegionSourceGate.Evaluate(
            region, region.DocumentFingerprint, null, RegionEvaluationRequirement.SourceLinkOnly));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void DefaultOrUndefinedRequirement_CannotApprove(int requirement) =>
        Assert.Equal(RegionSourceGateCode.UnknownRequirement, RegionSourceGate.Evaluate(
            null, null, null, (RegionEvaluationRequirement)requirement));

    [Fact]
    public void MissingImportedRegion_IsRejected() =>
        Assert.Equal(RegionSourceGateCode.MissingRegion, RegionSourceGate.Evaluate(
            null, new string('A', 64), "#8", RegionEvaluationRequirement.SourceLinkOnly));
}
