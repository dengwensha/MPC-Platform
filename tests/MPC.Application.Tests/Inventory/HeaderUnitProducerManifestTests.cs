using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitProducerManifestTests
{
    [Theory]
    [InlineData("HeaderUnitObservation.cs", "D062130ED86A69D3B0C26F3246CFF6EC3B4F72B891C9D4235F9FBD41C7041633")]
    [InlineData("InventoryEvidence.cs", "7D960E2238128FDE43B8E642282D36920A6FD58E0FAC39EE28F8E49E9BCBA94F")]
    [InlineData("EnvelopeObservation.cs", "1CAD8B6B3C980D63B89612B21B445DBFEEFB113256F11A9D358F5DF78A35E4D6")]
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
        Assert.Equal(2,HeaderUnitObservation.HeaderPolicyRevision);
        Assert.Equal(1,HeaderUnitObservation.UnitPolicyRevision);
        Assert.Equal("Error",HeaderUnitProducer.ReasonSeverity);
        Assert.Equal(new[] {"HeaderMalformed", "UnsupportedSchema", "UnsupportedSchemaForm", "UnsupportedImplementationLevel",
            "InventoryUnavailable", "NoGeometricContext", "ContextIdentityInvalid", "ContextMalformed",
            "UnitAssignmentsMissing", "UnitReferenceInvalid", "UnitDefinitionMalformed", "BlockedConversionUnit",
            "UnsupportedUnit", "MissingLengthUnit", "MissingAngleUnit", "AmbiguousLengthUnit", "AmbiguousAngleUnit"},HeaderUnitProducer.ReasonCodes);
    }
}
