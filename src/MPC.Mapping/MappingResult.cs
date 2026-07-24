using System.Collections.ObjectModel;

namespace MPC.Mapping;

public sealed class MappingResult<T>
    where T : class
{
    private readonly ReadOnlyCollection<MappingDiagnostic> _diagnostics;

    private MappingResult(
        MappingStatus status,
        T? value,
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Mapping status is not defined.");
        }

        ArgumentNullException.ThrowIfNull(diagnostics);

        var diagnosticList = diagnostics.ToList();

        if (diagnosticList.Any(static diagnostic => diagnostic is null))
        {
            throw new ArgumentException(
                "Mapping diagnostics cannot contain null items.",
                nameof(diagnostics));
        }

        ValidateState(status, value, diagnosticList);

        Status = status;
        Value = value;
        _diagnostics = diagnosticList.AsReadOnly();
    }

    public MappingStatus Status { get; }

    public T? Value { get; }

    public IReadOnlyList<MappingDiagnostic> Diagnostics => _diagnostics;

    public bool IsSuccessful =>
        Status is MappingStatus.Succeeded
            or MappingStatus.SucceededWithDiagnostics;

    public static MappingResult<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new MappingResult<T>(
            MappingStatus.Succeeded,
            value,
            Array.Empty<MappingDiagnostic>());
    }

    public static MappingResult<T> SuccessWithDiagnostics(
        T value,
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(diagnostics);

        return new MappingResult<T>(
            MappingStatus.SucceededWithDiagnostics,
            value,
            diagnostics);
    }

    public static MappingResult<T> Failure(
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return new MappingResult<T>(
            MappingStatus.Failed,
            null,
            diagnostics);
    }

    private static void ValidateState(
        MappingStatus status,
        T? value,
        IReadOnlyCollection<MappingDiagnostic> diagnostics)
    {
        switch (status)
        {
            case MappingStatus.Succeeded:
                if (value is null)
                {
                    throw new InvalidOperationException(
                        "A succeeded mapping result must contain a value.");
                }

                if (diagnostics.Count != 0)
                {
                    throw new InvalidOperationException(
                        "A succeeded mapping result cannot contain diagnostics.");
                }

                break;

            case MappingStatus.SucceededWithDiagnostics:
                if (value is null)
                {
                    throw new InvalidOperationException(
                        "A mapping result that succeeded with diagnostics must contain a value.");
                }

                if (diagnostics.Count == 0)
                {
                    throw new InvalidOperationException(
                        "A mapping result that succeeded with diagnostics must contain at least one diagnostic.");
                }

                break;

            case MappingStatus.Failed:
                if (value is not null)
                {
                    throw new InvalidOperationException(
                        "A failed mapping result cannot contain a value.");
                }

                if (diagnostics.Count == 0)
                {
                    throw new InvalidOperationException(
                        "A failed mapping result must contain at least one diagnostic.");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Mapping status is not defined.");
        }
    }
}