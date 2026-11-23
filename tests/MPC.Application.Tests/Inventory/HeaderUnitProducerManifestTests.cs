using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitProducerManifestTests
{
    [Theory]
    [InlineData("HeaderUnitObservation.cs", "7706678B27D55A59BA0BF684BEA65F19282D595A170C1DA558415F869FC701BA")]
    [InlineData("InventoryEvidence.cs", "0DD4C78C461BE5ABBBC3A7740848262FD1AB21591690AD725476F7F3F24EFAB2")]
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
