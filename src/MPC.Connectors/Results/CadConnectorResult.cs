using MPC.Connectors.Diagnostics;

namespace MPC.Connectors.Results;

public sealed class CadConnectorResult<T>
{
    private readonly IReadOnlyList<CadDiagnostic> _diagnostics;

    public CadConnectorResultStatus Status { get; }

    public T? Value { get; }

    public IReadOnlyList<CadDiagnostic> Diagnostics =>
        _diagnostics;

    private CadConnectorResult(
        CadConnectorResultStatus status,
        T? value,
        IEnumerable<CadDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var diagnosticList = diagnostics.ToList();

        if (diagnosticList.Any(diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "Diagnostics cannot contain null entries.",
                nameof(diagnostics));
        }

        EnsureValidState(
            status,
            value,
            diagnosticList);

        Status = status;
        Value = value;
        _diagnostics = diagnosticList.AsReadOnly();
    }

    public static CadConnectorResult<T> Success(
        T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new CadConnectorResult<T>(
            CadConnectorResultStatus.Succeeded,
            value,
            Array.Empty<CadDiagnostic>());
    }

    public static CadConnectorResult<T> SuccessWithDiagnostics(
        T value,
        IEnumerable<CadDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new CadConnectorResult<T>(
            CadConnectorResultStatus.SucceededWithDiagnostics,
            value,
            diagnostics);
    }

    public static CadConnectorResult<T> Failure(
        IEnumerable<CadDiagnostic> diagnostics)
    {
        return new CadConnectorResult<T>(
            CadConnectorResultStatus.Failed,
            default,
            diagnostics);
    }

    private static void EnsureValidState(
        CadConnectorResultStatus status,
        T? value,
        IReadOnlyCollection<CadDiagnostic> diagnostics)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Connector result status is not defined.");
        }

        var hasFatalDiagnostic =
            diagnostics.Any(
                diagnostic =>
                    diagnostic.Severity ==
                    CadDiagnosticSeverity.Fatal);

        switch (status)
        {
            case CadConnectorResultStatus.Succeeded:

                if (value is null)
                {
                    throw new InvalidOperationException(
                        "Succeeded result must contain a value.");
                }

                if (diagnostics.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Succeeded result cannot contain diagnostics.");
                }

                break;

            case CadConnectorResultStatus.SucceededWithDiagnostics:

                if (value is null)
                {
                    throw new InvalidOperationException(
                        "SucceededWithDiagnostics result must contain a value.");
                }

                if (diagnostics.Count == 0)
                {
                    throw new InvalidOperationException(
                        "SucceededWithDiagnostics result must contain at least one diagnostic.");
                }

                if (hasFatalDiagnostic)
                {
                    throw new InvalidOperationException(
                        "SucceededWithDiagnostics result cannot contain a fatal diagnostic.");
                }

                break;

            case CadConnectorResultStatus.Failed:

                if (value is not null)
                {
                    throw new InvalidOperationException(
                        "Failed result cannot contain a value.");
                }

                if (!hasFatalDiagnostic)
                {
                    throw new InvalidOperationException(
                        "Failed result must contain at least one fatal diagnostic.");
                }

                break;

            default:

                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Connector result status is not supported.");
        }
    }
}
