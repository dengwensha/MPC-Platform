using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;

namespace MPC.Tests.Connectors;

public sealed class CadConnectorResultTests
{
    [Fact]
    public void Success_WithValue_CreatesSucceededResult()
    {
        var result =
            CadConnectorResult<string>.Success(
                "snapshot");

        Assert.Equal(
            CadConnectorResultStatus.Succeeded,
            result.Status);

        Assert.Equal(
            "snapshot",
            result.Value);

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void SuccessWithDiagnostics_WithWarning_CreatesPartialSuccessResult()
    {
        var diagnostic = new CadDiagnostic(
            code: "STEP_ENTITY_PARTIAL",
            severity: CadDiagnosticSeverity.Warning,
            message: "Entity was read with limited metadata.");

        var result =
            CadConnectorResult<string>.SuccessWithDiagnostics(
                "snapshot",
                new[]
                {
                    diagnostic
                });

        Assert.Equal(
            CadConnectorResultStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.Equal(
            "snapshot",
            result.Value);

        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void Failure_WithFatalDiagnostic_CreatesFailedResult()
    {
        var diagnostic = new CadDiagnostic(
            code: "STEP_PARSE_FAILED",
            severity: CadDiagnosticSeverity.Fatal,
            message: "STEP file could not be parsed.");

        var result =
            CadConnectorResult<string>.Failure(
                new[]
                {
                    diagnostic
                });

        Assert.Equal(
            CadConnectorResultStatus.Failed,
            result.Status);

        Assert.Null(result.Value);
        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => CadConnectorResult<string>.Success(
                null!));
    }

    [Fact]
    public void SuccessWithDiagnostics_WithEmptyDiagnostics_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                CadConnectorResult<string>
                    .SuccessWithDiagnostics(
                        "snapshot",
                        Array.Empty<CadDiagnostic>()));
    }

    [Fact]
    public void SuccessWithDiagnostics_WithFatalDiagnostic_ThrowsInvalidOperationException()
    {
        var diagnostic = new CadDiagnostic(
            code: "FATAL",
            severity: CadDiagnosticSeverity.Fatal,
            message: "Fatal error.");

        Assert.Throws<InvalidOperationException>(
            () =>
                CadConnectorResult<string>
                    .SuccessWithDiagnostics(
                        "snapshot",
                        new[]
                        {
                            diagnostic
                        }));
    }

    [Fact]
    public void Failure_WithoutFatalDiagnostic_ThrowsInvalidOperationException()
    {
        var diagnostic = new CadDiagnostic(
            code: "WARNING",
            severity: CadDiagnosticSeverity.Warning,
            message: "Warning.");

        Assert.Throws<InvalidOperationException>(
            () => CadConnectorResult<string>.Failure(
                new[]
                {
                    diagnostic
                }));
    }

    [Fact]
    public void Failure_WithEmptyDiagnostics_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => CadConnectorResult<string>.Failure(
                Array.Empty<CadDiagnostic>()));
    }

    [Fact]
    public void Diagnostics_AreExposedAsReadOnlyCollection()
    {
        var diagnostic = new CadDiagnostic(
            code: "WARNING",
            severity: CadDiagnosticSeverity.Warning,
            message: "Warning.");

        var sourceDiagnostics =
            new List<CadDiagnostic>
            {
                diagnostic
            };

        var result =
            CadConnectorResult<string>.SuccessWithDiagnostics(
                "snapshot",
                sourceDiagnostics);

        sourceDiagnostics.Clear();

        Assert.Single(result.Diagnostics);
    }
}
