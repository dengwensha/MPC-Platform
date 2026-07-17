using MPC.Connectors.Diagnostics;
using MPC.Connectors.Snapshots.Entities;
using MPC.Connectors.Snapshots.Source;
using MPC.Connectors.Snapshots.Units;

namespace MPC.Connectors.Snapshots;

public sealed class CadModelSnapshot
{
    private readonly IReadOnlyList<CadEntitySnapshot> _entities;
    private readonly IReadOnlyList<CadDiagnostic> _diagnostics;

    public CadSourceInfo Source { get; }

    public CadUnitInfo Units { get; }

    public IReadOnlyList<CadEntitySnapshot> Entities =>
        _entities;

    public IReadOnlyList<CadDiagnostic> Diagnostics =>
        _diagnostics;

    public int EntityCount =>
        _entities.Count;

    public CadModelSnapshot(
        CadSourceInfo source,
        CadUnitInfo units,
        IEnumerable<CadEntitySnapshot> entities,
        IEnumerable<CadDiagnostic>? diagnostics = null)
    {
        Source = source ??
            throw new ArgumentNullException(nameof(source));

        Units = units ??
            throw new ArgumentNullException(nameof(units));

        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();

        if (entityList.Any(entity => entity is null))
        {
            throw new ArgumentException(
                "Entities cannot contain null entries.",
                nameof(entities));
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

        _entities = entityList.AsReadOnly();
        _diagnostics = diagnosticList.AsReadOnly();
    }
}
