using MPC.Connectors.Diagnostics;

namespace MPC.Connectors.Snapshots.Entities;

public sealed class CadEntitySnapshot
{
    private readonly IReadOnlyList<CadDiagnostic> _diagnostics;

    public string? SourceEntityId { get; }

    public string SourceEntityType { get; }

    public CadEntityCategory Category { get; }

    public CadReadStatus ReadStatus { get; }

    public IReadOnlyList<CadDiagnostic> Diagnostics =>
        _diagnostics;

    public CadEntitySnapshot(
        string? sourceEntityId,
        string sourceEntityType,
        CadEntityCategory category,
        CadReadStatus readStatus,
        IEnumerable<CadDiagnostic>? diagnostics = null)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityType))
        {
            throw new ArgumentException(
                "Source entity type cannot be null, empty, or whitespace.",
                nameof(sourceEntityType));
        }

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(
                nameof(category),
                category,
                "Entity category is not defined.");
        }

        if (!Enum.IsDefined(readStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(readStatus),
                readStatus,
                "Read status is not defined.");
        }

        var diagnosticList =
            diagnostics?.ToList() ??
            new List<CadDiagnostic>();

        if (diagnosticList.Any(
                diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "Diagnostics cannot contain null entries.",
                nameof(diagnostics));
        }

        EnsureReadStatusMatchesDiagnostics(
            readStatus,
            diagnosticList);

        SourceEntityId = NormalizeOptional(
            sourceEntityId);

        SourceEntityType = sourceEntityType.Trim();
        Category = category;
        ReadStatus = readStatus;
        _diagnostics = diagnosticList.AsReadOnly();
    }

    private static void EnsureReadStatusMatchesDiagnostics(
        CadReadStatus readStatus,
        IReadOnlyCollection<CadDiagnostic> diagnostics)
    {
        var hasErrorOrFatal =
            diagnostics.Any(
                diagnostic =>
                    diagnostic.Severity ==
                    CadDiagnosticSeverity.Error ||
                    diagnostic.Severity ==
                    CadDiagnosticSeverity.Fatal);

        switch (readStatus)
        {
            case CadReadStatus.Complete:

                if (hasErrorOrFatal)
                {
                    throw new InvalidOperationException(
                        "Complete entity cannot contain error or fatal diagnostics.");
                }

                break;

            case CadReadStatus.Partial:

                if (!hasErrorOrFatal)
                {
                    throw new InvalidOperationException(
                        "Partial entity must contain at least one error or fatal diagnostic.");
                }

                break;

            case CadReadStatus.Failed:

                if (!hasErrorOrFatal)
                {
                    throw new InvalidOperationException(
                        "Failed entity must contain at least one error or fatal diagnostic.");
                }

                break;

            default:

                throw new ArgumentOutOfRangeException(
                    nameof(readStatus),
                    readStatus,
                    "Read status is not supported.");
        }
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
