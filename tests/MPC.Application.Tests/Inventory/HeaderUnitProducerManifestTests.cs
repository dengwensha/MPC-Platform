using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitProducerManifestTests
{
    [Theory]
    [InlineData("HeaderUnitObservation.cs", "CBC136F2215BF2C5C1D88957C7305C554F511B9A6F512E8BB44625B5577FE1CB")]
    [InlineData("InventoryEvidence.cs", "27897C8DD22F417D3D7A0DD25A4669C2437AA02CDE6D0454A0DFC121753ECD45")]
    [InlineData("StepParameterParser.cs", "69AA2488ECBAF60C869F83EE8DE3797B3DFA04ACBC26CB0E216CBAB27CD4AF97")]
    public void CompleteProducerAndCaptureSourcesArePinned(string name,string expected)
    {
        string text=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"HeaderUnitAudit",name+".txt"))
            .TrimStart('\uFEFF').Replace("\r\n","\n");
        Assert.Equal(expected,Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))));
    }
    [Fact]
    public void PolicyRevisionsReasonCodesAndSeverityRequireExplicitReview()
    {
        Assert.Equal(1,HeaderUnitObservation.HeaderPolicyRevision);
        Assert.Equal(1,HeaderUnitObservation.UnitPolicyRevision);
        Assert.Equal("Error",HeaderUnitProducer.ReasonSeverity);
        Assert.Equal(new[] {"HeaderMalformed", "UnsupportedSchema", "UnsupportedSchemaForm", "UnsupportedImplementationLevel",
            "InventoryUnavailable", "NoGeometricContext", "ContextIdentityInvalid", "ContextMalformed",
            "UnitAssignmentsMissing", "UnitReferenceInvalid", "UnitDefinitionMalformed", "BlockedConversionUnit",
            "UnsupportedUnit", "MissingLengthUnit", "MissingAngleUnit", "AmbiguousLengthUnit", "AmbiguousAngleUnit"},HeaderUnitProducer.ReasonCodes);
    }
}
