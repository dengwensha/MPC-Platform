using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;
namespace MPC.Application.Tests.Inventory;
public sealed class TargetProducerManifestTests
{
    [Theory]
    [InlineData("TargetObservation.cs","81C618243BD4D362230B2B950175D85D1AC484E9CCDD03A397D4414F91C31A5B")]
    [InlineData("TargetProducer.cs","685C016FFDAFC0559BCCC26F22B386E6FF4389A4CBDAC8E0F094D07D06E73030")]
    [InlineData("TargetBinding.cs","2A44B81A7B73308DD2BADB8822600B323C96790242EE1771E62C0854ADD0C590")]
    public void CompleteTargetSourcesRequireReview(string name,string expected)
    {
        string text=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"TargetAudit",name)).TrimStart('\uFEFF').Replace("\r\n","\n");
        Assert.Equal(expected,Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))));
    }
    [Fact]
    public void ContractReasonCodesAndSeverityAreFixed()
    {
        Assert.Equal("F6C55BD1B7E84CE00683BBB2BC018DC3871EDD6FF2AD0BAFD1DCABFE7227BBB1",TargetProducer.ContractSha256);
        Assert.Equal("Error",TargetProducer.ReasonSeverity);
        Assert.Equal(new[]{"TargetEvidenceUnavailable","TargetOwnerMismatch","TargetRevisionUnsupported","InventoryNotClear","RecordIdentityInvalid","RecordPartial","UnclassifiedSignature","UnsupportedScope","FieldConstraintFailed","ReferenceUnresolved","ReferenceRoleMismatch","UnownedGeometry","RootBindingInvalid","ProductBindingInvalid","PresentationInvalid","ContextBindingUnavailable","ContextUnitsUnsupported","BridgeUnitMismatch","CurrentImportIncompatible","DependencyUnavailable"},TargetProducer.ReasonCodes);
        Assert.Equal(new[]{"TargetBinding.cs","TargetObservation.cs","TargetProducer.cs"},Directory.GetFiles(Path.Combine(AppContext.BaseDirectory,"TargetAudit"),"Target*.cs").Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }
}
