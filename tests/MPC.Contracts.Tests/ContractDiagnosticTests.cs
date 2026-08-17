using System;
using MPC.Contracts;

namespace MPC.Contracts.Tests;

public sealed class ContractDiagnosticTests
{
    [Fact]
    public void Constructor_PreservesTransportFields()
    {
        var diagnostic = new ContractDiagnostic(
            " CONTRACT-001 ",
            ContractDiagnosticSeverity.Warning,
            " Warning message ");

        Assert.Equal("CONTRACT-001", diagnostic.Code);
        Assert.Equal(
            ContractDiagnosticSeverity.Warning,
            diagnostic.Severity);
        Assert.Equal("Warning message", diagnostic.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RejectsMissingCode(string? code)
    {
        Assert.Throws<ArgumentException>(
            () => new ContractDiagnostic(
                code!,
                ContractDiagnosticSeverity.Error,
                "Message"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RejectsMissingMessage(string? message)
    {
        Assert.Throws<ArgumentException>(
            () => new ContractDiagnostic(
                "CONTRACT-001",
                ContractDiagnosticSeverity.Error,
                message!));
    }

    [Fact]
    public void Constructor_RejectsUndefinedSeverity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ContractDiagnostic(
                "CONTRACT-001",
                (ContractDiagnosticSeverity)999,
                "Message"));
    }
}
