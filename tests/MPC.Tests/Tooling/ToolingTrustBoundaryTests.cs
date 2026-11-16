using MPC.Tooling;
using MPC.Tooling.Compatibility;

namespace MPC.Tests.Tooling;

// Synthetic interfaces only; code 2 has no verified supplier meaning.
// Production-path tests, separate from test-local catalog specifications.
public sealed class ToolingTrustBoundaryTests
{
    private static ToolInterface Interface(int? mount = 2) =>
        new("C", "19", 19.05, 6.35, 80, 0, mount);

    private static ToolHolder Holder(ToolHand hand, ToolInterface? face = null) =>
        new(new ToolProductIdentity("Synthetic", "holder", "H"), face ?? Interface(), "family", hand);

    private static CuttingInsert Insert(ToolHand hand, ToolInterface? face = null) =>
        new(new ToolProductIdentity("Synthetic", "insert", "I"), face ?? Interface(),
            new CuttingGrade("test", "test"), null, hand);

    [Theory]
    [InlineData(default(ToolHand), ToolHand.Right)]
    [InlineData(ToolHand.Right, default(ToolHand))]
    [InlineData(ToolHand.Unknown, ToolHand.Unknown)]
    [InlineData(ToolHand.Unknown, ToolHand.Neutral)]
    [InlineData(ToolHand.Neutral, ToolHand.Unknown)]
    public void DefaultHand_NeverApprovesInterface(ToolHand holder, ToolHand insert)
    {
        var result = new InsertHolderCompatibilityEvaluator().Evaluate(
            Holder(holder), Insert(insert));
        Assert.False(result.IsCompatible);
        Assert.Equal(CompatibilityCode.UnknownHand, result.Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UnmappedMounting_NeverApprovesInterface(bool unknownHolder)
    {
        var result = new InsertHolderCompatibilityEvaluator().Evaluate(
            Holder(ToolHand.Right, Interface(unknownHolder ? null : 2)),
            Insert(ToolHand.Right, Interface(unknownHolder ? 2 : null)));
        Assert.False(result.IsCompatible);
        Assert.Equal("UnmappedMountingStyle", result.Code.ToString());
    }

    [Fact]
    public void TwoUnmappedMountings_DoNotMatchEachOther()
    {
        var result = new InsertHolderCompatibilityEvaluator().Evaluate(
            Holder(ToolHand.Right, Interface(null)), Insert(ToolHand.Right, Interface(null)));
        Assert.False(result.IsCompatible);
    }

    [Fact]
    public void Result_ExplicitlyLimitsScopeToInterface()
    {
        var result = new InsertHolderCompatibilityEvaluator().Evaluate(
            Holder(ToolHand.Right), Insert(ToolHand.Right));
        Assert.Equal(CompatibilityScope.InterfaceOnly, result.Scope);
    }

    [Theory]
    [InlineData("11412575")]
    [InlineData("7323225046702")]
    [InlineData("123")]
    [InlineData("ABCDEFGH")]
    [InlineData("１２３４５６７０")]
    [InlineData("   ")]
    public void InvalidGtin_PreservesRawValueWithoutVerifiedValue(string raw)
    {
        var identity = new ToolProductIdentity("Synthetic", "id", "code", raw);
        Assert.Equal(raw, identity.Ean);
        Assert.Equal(GtinValidationStatus.NeedsReview, identity.GtinStatus);
        Assert.Null(identity.FormatValidatedGtin);
    }

    [Theory]
    [InlineData("96385074")]
    [InlineData("036000291452")]
    [InlineData("7323225046701")]
    [InlineData("10012345000017")]
    [InlineData(" 7323225046701 ")]
    public void ValidFormat_IsNotProductIdentityCertification(string raw)
    {
        var identity = new ToolProductIdentity("Synthetic", "id", "code", raw);
        Assert.Equal(raw, identity.Ean);
        Assert.Equal(GtinValidationStatus.ValidFormat, identity.GtinStatus);
        Assert.Equal(raw.Trim(), identity.FormatValidatedGtin);
    }

    [Fact]
    public void AbsentGtin_IsNotValidated()
    {
        var identity = new ToolProductIdentity("Synthetic", "id", "code");
        Assert.Equal(GtinValidationStatus.NotProvided, identity.GtinStatus);
        Assert.Null(identity.FormatValidatedGtin);
    }

    [Theory]
    [InlineData(ToolHand.Neutral, ToolHand.Left, true)]
    [InlineData(ToolHand.Neutral, ToolHand.Right, true)]
    [InlineData(ToolHand.Left, ToolHand.Neutral, true)]
    [InlineData(ToolHand.Right, ToolHand.Neutral, true)]
    [InlineData(ToolHand.Neutral, ToolHand.Neutral, true)]
    [InlineData(ToolHand.Left, ToolHand.Right, false)]
    [InlineData(ToolHand.Right, ToolHand.Left, false)]
    public void KnownNeutralBehavior_IsCharacterizedNotCertified(
        ToolHand holder, ToolHand insert, bool expected)
    {
        Assert.Equal(expected, new InsertHolderCompatibilityEvaluator()
            .Evaluate(Holder(holder), Insert(insert)).IsCompatible);
    }
}
