using System.Collections.ObjectModel;

namespace MPC.Mapping;

public sealed class MappingContext
{
    private readonly List<MappingDiagnostic> _diagnostics = new();

    private readonly ReadOnlyCollection<MappingDiagnostic>
        _readOnlyDiagnostics;

    public MappingContext(
        IReferenceResolver referenceResolver)
        : this(
            referenceResolver,
            null)
    {
    }

    public MappingContext(
        IReferenceResolver referenceResolver,
        ICanonicalReferenceResolver? canonicalReferenceResolver)
    {
        ArgumentNullException.ThrowIfNull(
            referenceResolver);

        ReferenceResolver =
            referenceResolver;

        CanonicalReferenceResolver =
            canonicalReferenceResolver;

        _readOnlyDiagnostics =
            _diagnostics.AsReadOnly();
    }

    public IReferenceResolver ReferenceResolver { get; }

    public ICanonicalReferenceResolver?
        CanonicalReferenceResolver
    { get; }

    public IReadOnlyList<MappingDiagnostic> Diagnostics =>
        _readOnlyDiagnostics;

    public bool HasDiagnostics =>
        _diagnostics.Count > 0;

    public bool HasErrors =>
        _diagnostics.Any(
            static diagnostic =>
                diagnostic.Severity ==
                MappingDiagnosticSeverity.Error);

    public void AddDiagnostic(
        MappingDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostic);

        _diagnostics.Add(
            diagnostic);
    }

    public void AddDiagnostics(
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostics);

        foreach (MappingDiagnostic diagnostic
                 in diagnostics)
        {
            AddDiagnostic(
                diagnostic);
        }
    }

    public int CaptureDiagnosticCheckpoint()
    {
        return _diagnostics.Count;
    }

    public IReadOnlyList<MappingDiagnostic>
        GetDiagnosticsSince(
            int checkpoint)
    {
        ValidateDiagnosticCheckpoint(
            checkpoint);

        if (checkpoint == _diagnostics.Count)
        {
            return Array.Empty<MappingDiagnostic>();
        }

        List<MappingDiagnostic> diagnostics =
            _diagnostics.GetRange(
                checkpoint,
                _diagnostics.Count - checkpoint);

        return diagnostics.AsReadOnly();
    }

    public bool HasDiagnosticsSince(
        int checkpoint)
    {
        ValidateDiagnosticCheckpoint(
            checkpoint);

        return checkpoint < _diagnostics.Count;
    }

    public bool HasErrorsSince(
        int checkpoint)
    {
        ValidateDiagnosticCheckpoint(
            checkpoint);

        for (int index = checkpoint;
             index < _diagnostics.Count;
             index++)
        {
            if (_diagnostics[index].Severity ==
                MappingDiagnosticSeverity.Error)
            {
                return true;
            }
        }

        return false;
    }

    private void ValidateDiagnosticCheckpoint(
        int checkpoint)
    {
        if (checkpoint < 0 ||
            checkpoint > _diagnostics.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(checkpoint),
                checkpoint,
                "Diagnostic checkpoint must refer to a valid position in the mapping context diagnostic sequence.");
        }
    }
}