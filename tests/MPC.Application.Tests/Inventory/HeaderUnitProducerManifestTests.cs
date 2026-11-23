using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitProducerManifestTests
{
    [Theory]
    [InlineData("HeaderUnitObservation.cs", "FE3CFE04AB15D31BC64FB38D42AFDAC52AA3C444CD2309F81E145B35BDB1D1A3")]
    [InlineData("InventoryEvidence.cs", "18536E650A3E3A1C0FC5A64B2D0F3253413FD33C7EAF726450D0EFCDE59E80A3")]
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
