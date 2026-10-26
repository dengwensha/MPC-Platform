using MPC.Tooling;
using MPC.Tooling.Compatibility;

namespace MPC.Tests.Tooling;

// Synthetic interfaces only; code 2 has no verified supplier meaning.
// Production-path tests, separate from test-local catalog specifications.
public sealed class ToolingTrustBoundaryTests
{
    private static ToolInterface Interface(int? mount = 2) =>
        (ToolInterface)Activator.CreateInstance(typeof(ToolInterface),
            new object?[] { "C", "19", 19.05, 6.35, 80d, 0d, mount })!;

    private static ToolHolder Holder(ToolHand hand, ToolInterface? face = null) =>
        new(new ToolProductIdentity("Synthetic", "holder", "H"), face ?? Interface(), "family", hand);

    private static CuttingInsert Insert(ToolHand hand, ToolInterface? face = null) =>
        new(new ToolProductIdentity("Synthetic", "insert", "I"), face ?? Interface(),
            new CuttingGrade("test", "test"), null, hand);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DefaultHand_NeverApprovesInterface(bool unknownHolder)
    {
        var result = new InsertHolderCompatibilityEvaluator().Evaluate(
            Holder(unknownHolder ? default : ToolHand.Right),
            Insert(unknownHolder ? ToolHand.Right : default));
        Assert.False(result.IsCompatible);
        Assert.Equal("UnknownHand", result.Code.ToString());
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
        var scope = result.GetType().GetProperty("Scope");
        Assert.NotNull(scope);
        Assert.Equal("InterfaceOnly", scope.GetValue(result)?.ToString());
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
        var status = identity.GetType().GetProperty("GtinStatus");
        var value = identity.GetType().GetProperty("FormatValidatedGtin");
        Assert.NotNull(status);
        Assert.NotNull(value);
        Assert.Equal("NeedsReview", status.GetValue(identity)?.ToString());
        Assert.Null(value.GetValue(identity));
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
        var status = identity.GetType().GetProperty("GtinStatus");
        var value = identity.GetType().GetProperty("FormatValidatedGtin");
        Assert.NotNull(status);
        Assert.NotNull(value);
        Assert.Equal("ValidFormat", status.GetValue(identity)?.ToString());
        Assert.Equal(raw.Trim(), value.GetValue(identity));
    }

    [Fact]
    public void AbsentGtin_IsNotValidated()
    {
        var identity = new ToolProductIdentity("Synthetic", "id", "code");
        var status = identity.GetType().GetProperty("GtinStatus");
        Assert.NotNull(status);
        Assert.Equal("NotProvided", status.GetValue(identity)?.ToString());
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
