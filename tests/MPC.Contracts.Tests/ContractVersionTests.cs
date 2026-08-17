using System;
using MPC.Contracts;

namespace MPC.Contracts.Tests;

public sealed class ContractVersionTests
{
    [Fact]
    public void Current_IsStableVersionOne()
    {
        Assert.Equal("1.0", ContractVersion.Current);
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData(" 1.0 ")]
    public void IsSupported_ReturnsTrue_ForCurrentVersion(
        string version)
    {
        Assert.True(ContractVersion.IsSupported(version));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0.9")]
    [InlineData("1.1")]
    [InlineData("2.0")]
    public void IsSupported_ReturnsFalse_ForUnknownVersion(
        string? version)
    {
        Assert.False(ContractVersion.IsSupported(version));
    }
}
