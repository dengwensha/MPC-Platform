using System.Security.Cryptography;
using MPC.Application.Manufacturing;

namespace MPC.Application.Tests.Manufacturing;

public sealed class ValidatedRegionImportServiceTests
{
    private static string Golden()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            string file = Path.Combine(current.FullName, "tests", "MPC.Tests",
                "TestData", "Step", "Golden", "T001_PRISM.step");
            if (File.Exists(file)) return file;
            current = current.Parent;
        }
        throw new FileNotFoundException("T001_PRISM.step");
    }

    private static string Hash(string file) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));

    [Fact]
    public void MatchingRealImport_PublishesOnlySourceLinkedRegion()
    {
        string file = Golden();
        var code = ValidatedRegionImportService.Execute(file, "#8", Hash(file), "#8",
            RegionEvaluationRequirement.SourceLinkOnly, out var region);
        Assert.Equal(RegionSourceGateCode.SourceLinkMatched, code);
        Assert.NotNull(region);
        Assert.Equal(Hash(file), region.DocumentFingerprint);
        Assert.Equal("#8", region.SourceFaceId);
    }

    [Fact]
    public void ChangedSourceBytes_RejectOldExpectedHashWithoutPublishingRegion()
    {
        string file = Golden();
        string copy = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        try
        {
            File.Copy(file, copy);
            string oldHash = Hash(copy);
            File.AppendAllText(copy, Environment.NewLine);
            var code = ValidatedRegionImportService.Execute(copy, "#8", oldHash, "#8",
                RegionEvaluationRequirement.SourceLinkOnly, out var region);
            Assert.Equal(RegionSourceGateCode.DocumentMismatch, code);
            Assert.Null(region);
        }
        finally { File.Delete(copy); }
    }

    [Fact]
    public void WrongExpectedFace_DoesNotPublishImportedRegion()
    {
        string file = Golden();
        var code = ValidatedRegionImportService.Execute(file, "#8", Hash(file), "#30",
            RegionEvaluationRequirement.SourceLinkOnly, out var region);
        Assert.Equal(RegionSourceGateCode.FaceMismatch, code);
        Assert.Null(region);
    }

    [Fact]
    public void MaterialSideRequest_RemainsBlockedAfterSuccessfulImport()
    {
        string file = Golden();
        var code = ValidatedRegionImportService.Execute(file, "#8", Hash(file), "#8",
            RegionEvaluationRequirement.MaterialSideDependent, out var region);
        Assert.Equal(RegionSourceGateCode.MaterialSideUnverified, code);
        Assert.Null(region);
    }

    [Fact]
    public void UnknownSourceFace_ReturnsMissingRegion()
    {
        string file = Golden();
        var code = ValidatedRegionImportService.Execute(file, "#999999", Hash(file), "#999999",
            RegionEvaluationRequirement.SourceLinkOnly, out var region);
        Assert.Equal(RegionSourceGateCode.MissingRegion, code);
        Assert.Null(region);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void UnknownRequirement_IsRejectedBeforeFileAccess(int requirement)
    {
        var code = ValidatedRegionImportService.Execute("unread-file", "#8", null, null,
            (RegionEvaluationRequirement)requirement, out var region);
        Assert.Equal(RegionSourceGateCode.UnknownRequirement, code);
        Assert.Null(region);
    }

    [Fact]
    public void InvalidExpectedIdentity_DoesNotPublishRegion()
    {
        var code = ValidatedRegionImportService.Execute(Golden(), "#8", "bad-hash", "#8",
            RegionEvaluationRequirement.SourceLinkOnly, out var region);
        Assert.Equal(RegionSourceGateCode.InvalidSourceIdentity, code);
        Assert.Null(region);
    }

    [Fact]
    public void MissingFile_ExceptionDoesNotProduceSuccessfulResult()
    {
        string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        Assert.Throws<FileNotFoundException>(() => ValidatedRegionImportService.Execute(
            missing, "#8", new string('A', 64), "#8",
            RegionEvaluationRequirement.SourceLinkOnly, out _));
    }
}
