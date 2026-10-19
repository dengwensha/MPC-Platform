using MPC.Application.Manufacturing;
using MPC.Mapping.Step;

namespace MPC.Application.Tests.Manufacturing;

public sealed class ManufacturingRegionContractTests
{
    [Fact]
    public void UnmappedFace_IsNeverAccepted()
    {
        // No mapping has been published; unknown source identity must fail.
        var operation = StepCanonicalMappingOperation.Create(
            new MPC.Connectors.Step.Semantics.StepSemanticEntityResolver(
                Array.Empty<MPC.Connectors.Step.Semantics.StepSemanticEntity>()));
        bool ok = ManufacturingRegion.TryCreate(
            operation, new string('a', 64), "#100", out var region);
        Assert.False(ok);
        Assert.Null(region);
    }

    [Theory]
    [InlineData("")]
    [InlineData("wrong-hash")]
    public void InvalidFingerprint_IsRejected(string fingerprint)
    {
        var operation = StepCanonicalMappingOperation.Create(
            new MPC.Connectors.Step.Semantics.StepSemanticEntityResolver(
                Array.Empty<MPC.Connectors.Step.Semantics.StepSemanticEntity>()));
        Assert.False(ManufacturingRegion.TryCreate(
            operation, fingerprint, "#100", out _));
    }
}
