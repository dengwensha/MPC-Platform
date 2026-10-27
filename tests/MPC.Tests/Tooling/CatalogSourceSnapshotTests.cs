using System.Text;
using MPC.Tooling;

namespace MPC.Tests.Tooling;

public sealed class CatalogSourceSnapshotTests
{
    private const string AbcHash =
        "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD";

    [Fact]
    public void KnownBytes_HaveIndependentSha256Vector()
    {
        var snapshot = new CatalogSourceSnapshot(Encoding.ASCII.GetBytes("abc"));
        Assert.Equal(AbcHash, snapshot.Sha256);
        Assert.True(snapshot.MatchesSha256(AbcHash.ToLowerInvariant()));
        Assert.Equal(Encoding.ASCII.GetBytes("abc"), snapshot.CopyBytes());
    }

    [Fact]
    public void EmptyBytes_HaveDefinedDigestWithoutImplyingCatalogEligibility()
    {
        var snapshot = new CatalogSourceSnapshot([]);
        Assert.Equal("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855",
            snapshot.Sha256);
        Assert.Empty(snapshot.CopyBytes());
    }

    [Fact]
    public void CallerMutation_CannotAlterRetainedSnapshot()
    {
        byte[] input = Encoding.ASCII.GetBytes("abc");
        var snapshot = new CatalogSourceSnapshot(input);
        input[0] = (byte)'x';
        byte[] returned = snapshot.CopyBytes();
        returned[1] = (byte)'y';
        Assert.Equal(Encoding.ASCII.GetBytes("abc"), snapshot.CopyBytes());
        Assert.Equal(AbcHash, snapshot.Sha256);
        Assert.True(snapshot.MatchesSha256(AbcHash));
    }

    [Fact]
    public void ChangedSourceBytes_DoNotMatchOriginalDigest()
    {
        var changed = new CatalogSourceSnapshot(Encoding.ASCII.GetBytes("abd"));
        Assert.False(changed.MatchesSha256(AbcHash));
    }

    [Fact]
    public void RawByteDifferences_AreNotNormalized()
    {
        var plain = new CatalogSourceSnapshot(Encoding.UTF8.GetBytes("abc"));
        var newline = new CatalogSourceSnapshot(Encoding.UTF8.GetBytes("abc\n"));
        Assert.NotEqual(plain.Sha256, newline.Sha256);
        Assert.False(newline.MatchesSha256(plain.Sha256));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData(" BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD")]
    [InlineData("GA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD")]
    public void MalformedExpectedHash_FailsClosed(string? expected)
    {
        var snapshot = new CatalogSourceSnapshot(Encoding.ASCII.GetBytes("abc"));
        Assert.False(snapshot.MatchesSha256(expected));
    }

    [Fact]
    public void NullSource_IsRejected() =>
        Assert.Throws<ArgumentNullException>(() => new CatalogSourceSnapshot(null!));
}
