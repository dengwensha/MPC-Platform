namespace MPC.Application.Step;

public sealed class StepImportResult
{
    private readonly IReadOnlyList<StepImportDiagnostic>
        _diagnostics;

    public StepImportStatus Status { get; }

    public StepImportSummary? Summary { get; }

    public IReadOnlyList<StepImportDiagnostic> Diagnostics =>
        _diagnostics;

    private StepImportResult(
        StepImportStatus status,
        StepImportSummary? summary,
        IEnumerable<StepImportDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "STEP import status is not defined.");
        }

        var diagnosticList =
            diagnostics.ToList();

        if (diagnosticList.Any(
                diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "STEP import diagnostics cannot contain null entries.",
                nameof(diagnostics));
        }

        ValidateState(
            status,
            summary,
            diagnosticList);

        Status = status;
        Summary = summary;
        _diagnostics = diagnosticList.AsReadOnly();
    }

    public static StepImportResult Success(
        StepImportSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new StepImportResult(
            StepImportStatus.Succeeded,
            summary,
            Array.Empty<StepImportDiagnostic>());
    }

    public static StepImportResult SuccessWithDiagnostics(
        StepImportSummary summary,
        IEnumerable<StepImportDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new StepImportResult(
            StepImportStatus.SucceededWithDiagnostics,
            summary,
            diagnostics);
    }

    public static StepImportResult Failure(
        IEnumerable<StepImportDiagnostic> diagnostics)
    {
        return new StepImportResult(
            StepImportStatus.Failed,
            summary: null,
            diagnostics);
    }

    private static void ValidateState(
        StepImportStatus status,
        StepImportSummary? summary,
        IReadOnlyCollection<StepImportDiagnostic> diagnostics)
    {
        switch (status)
        {
            case StepImportStatus.Succeeded:
                if (summary is null || diagnostics.Count != 0)
                {
                    throw new InvalidOperationException(
                        "A succeeded STEP import must contain a summary and no diagnostics.");
                }

                break;

            case StepImportStatus.SucceededWithDiagnostics:
                if (summary is null || diagnostics.Count == 0)
                {
                    throw new InvalidOperationException(
                        "A STEP import that succeeded with diagnostics must contain a summary and diagnostics.");
                }

                break;

            case StepImportStatus.Failed:
                if (summary is not null || diagnostics.Count == 0)
                {
                    throw new InvalidOperationException(
                        "A failed STEP import must contain diagnostics and no summary.");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "STEP import status is not supported.");
        }
    }
}
