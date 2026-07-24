using System.Collections.ObjectModel;

namespace MPC.Mapping;

public sealed class MappingContext
{
    private readonly List<MappingDiagnostic> _diagnostics = new();
    private readonly ReadOnlyCollection<MappingDiagnostic> _readOnlyDiagnostics;

    public MappingContext(
        IReferenceResolver referenceResolver)
    {
        ArgumentNullException.ThrowIfNull(referenceResolver);

        ReferenceResolver = referenceResolver;
        _readOnlyDiagnostics = _diagnostics.AsReadOnly();
    }

    public IReferenceResolver ReferenceResolver { get; }

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
        ArgumentNullException.ThrowIfNull(diagnostic);

        _diagnostics.Add(diagnostic);
    }

    public void AddDiagnostics(
        IEnumerable<MappingDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        foreach (var diagnostic in diagnostics)
        {
            AddDiagnostic(diagnostic);
        }
    }
}