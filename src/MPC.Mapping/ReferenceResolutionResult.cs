using System.Collections.ObjectModel;

namespace MPC.Mapping;

public sealed class ReferenceResolutionResult<T>
    where T : class
{
    private readonly ReadOnlyCollection<MappingDiagnostic> _diagnostics;

    private ReferenceResolutionResult(
        string referenceId,
        ReferenceResolutionStatus status,
        T? value,
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
        {
            throw new ArgumentException(
                "Reference identifier cannot be null, empty, or whitespace.",
                nameof(referenceId));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Reference resolution status is not defined.");
        }

        ArgumentNullException.ThrowIfNull(diagnostics);

        var diagnosticList = diagnostics.ToList();

        if (diagnosticList.Any(static diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "Reference resolution diagnostics cannot contain null items.",
                nameof(diagnostics));
        }

        ValidateState(status, value, diagnosticList);

        ReferenceId = referenceId;
        Status = status;
        Value = value;
        _diagnostics = diagnosticList.AsReadOnly();
    }

    public string ReferenceId { get; }

    public ReferenceResolutionStatus Status { get; }

    public T? Value { get; }

    public IReadOnlyList<MappingDiagnostic> Diagnostics =>
        _diagnostics;

    public bool IsResolved =>
        Status == ReferenceResolutionStatus.Resolved;

    public static ReferenceResolutionResult<T> Resolved(
        string referenceId,
        T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ReferenceResolutionResult<T>(
            referenceId,
            ReferenceResolutionStatus.Resolved,
            value,
            Array.Empty<MappingDiagnostic>());
    }

    public static ReferenceResolutionResult<T> Failure(
        string referenceId,
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return new ReferenceResolutionResult<T>(
            referenceId,
            ReferenceResolutionStatus.Failed,
            null,
            diagnostics);
    }

    private static void ValidateState(
        ReferenceResolutionStatus status,
        T? value,
        IReadOnlyCollection<MappingDiagnostic> diagnostics)
    {
        switch (status)
        {
            case ReferenceResolutionStatus.Resolved:
                if (value is null)
                {
                    throw new InvalidOperationException(
                        "A resolved reference must contain a value.");
                }

                if (diagnostics.Count != 0)
                {
                    throw new InvalidOperationException(
                        "A resolved reference cannot contain diagnostics.");
                }

                break;

            case ReferenceResolutionStatus.Failed:
                if (value is not null)
                {
                    throw new InvalidOperationException(
                        "A failed reference resolution cannot contain a value.");
                }

                if (diagnostics.Count == 0)
                {
                    throw new InvalidOperationException(
                        "A failed reference resolution must contain at least one diagnostic.");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Reference resolution status is not defined.");
        }
    }
}