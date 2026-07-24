using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class MappingDiagnosticTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesDiagnostic()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP001",
            "Mapping completed with a warning.",
            MappingDiagnosticSeverity.Warning);

        Assert.Equal("MAP001", diagnostic.Code);
        Assert.Equal(
            "Mapping completed with a warning.",
            diagnostic.Message);
        Assert.Equal(
            MappingDiagnosticSeverity.Warning,
            diagnostic.Severity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCode_ThrowsArgumentException(
        string? code)
    {
        Assert.Throws<ArgumentException>(
            () => new MappingDiagnostic(
                code!,
                "Diagnostic message.",
                MappingDiagnosticSeverity.Error));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidMessage_ThrowsArgumentException(
        string? message)
    {
        Assert.Throws<ArgumentException>(
            () => new MappingDiagnostic(
                "MAP001",
                message!,
                MappingDiagnosticSeverity.Error));
    }

    [Fact]
    public void Constructor_WithUndefinedSeverity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MappingDiagnostic(
                "MAP001",
                "Diagnostic message.",
                (MappingDiagnosticSeverity)999));
    }
}