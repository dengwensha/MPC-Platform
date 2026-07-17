using MPC.Connectors.Diagnostics;

namespace MPC.Tests.Connectors;

public sealed class CadDiagnosticTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesDiagnostic()
    {
        var diagnostic = new CadDiagnostic(
            code: "STEP_SURFACE_READ_FAILED",
            severity: CadDiagnosticSeverity.Error,
            message: "Surface parameters could not be read.",
            sourceEntityId: "#142",
            sourceEntityType: "B_SPLINE_SURFACE",
            affectedField: "ControlPoints",
            exceptionType: "InvalidOperationException");

        Assert.Equal(
            "STEP_SURFACE_READ_FAILED",
            diagnostic.Code);

        Assert.Equal(
            CadDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Equal(
            "Surface parameters could not be read.",
            diagnostic.Message);

        Assert.Equal("#142", diagnostic.SourceEntityId);

        Assert.Equal(
            "B_SPLINE_SURFACE",
            diagnostic.SourceEntityType);

        Assert.Equal(
            "ControlPoints",
            diagnostic.AffectedField);

        Assert.Equal(
            "InvalidOperationException",
            diagnostic.ExceptionType);
    }

    [Fact]
    public void Constructor_WithOptionalWhitespace_NormalizesToNull()
    {
        var diagnostic = new CadDiagnostic(
            code: "INFO",
            severity: CadDiagnosticSeverity.Info,
            message: "Information.",
            sourceEntityId: " ",
            sourceEntityType: null,
            affectedField: "",
            exceptionType: "   ");

        Assert.Null(diagnostic.SourceEntityId);
        Assert.Null(diagnostic.SourceEntityType);
        Assert.Null(diagnostic.AffectedField);
        Assert.Null(diagnostic.ExceptionType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCode_ThrowsArgumentException(
        string? code)
    {
        Assert.Throws<ArgumentException>(
            () => new CadDiagnostic(
                code: code!,
                severity: CadDiagnosticSeverity.Warning,
                message: "Message"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidMessage_ThrowsArgumentException(
        string? message)
    {
        Assert.Throws<ArgumentException>(
            () => new CadDiagnostic(
                code: "CODE",
                severity: CadDiagnosticSeverity.Warning,
                message: message!));
    }

    [Fact]
    public void Constructor_WithUndefinedSeverity_ThrowsArgumentOutOfRangeException()
    {
        var undefinedSeverity =
            (CadDiagnosticSeverity)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CadDiagnostic(
                code: "CODE",
                severity: undefinedSeverity,
                message: "Message"));
    }

    [Fact]
    public void CadReadStatus_ContainsExpectedStates()
    {
        Assert.Equal(0, (int)CadReadStatus.Complete);
        Assert.Equal(1, (int)CadReadStatus.Partial);
        Assert.Equal(2, (int)CadReadStatus.Failed);
    }
}
