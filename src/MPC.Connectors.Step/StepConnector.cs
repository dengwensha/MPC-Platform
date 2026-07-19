using MPC.Connectors.Results;
using MPC.Connectors.Snapshots;
using MPC.Connectors.Snapshots.Entities;
using MPC.Connectors.Snapshots.Source;
using MPC.Connectors.Step.Contracts;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step;

public sealed class StepConnector
{
    private readonly IStepHeaderReader _headerReader;
    private readonly IStepDataEntityReader _dataReader;
    private readonly IStepUnitResolver _unitResolver;

    public StepConnector(
        IStepHeaderReader headerReader,
        IStepDataEntityReader dataReader,
        IStepUnitResolver unitResolver)
    {
        _headerReader = headerReader ??
            throw new ArgumentNullException(
                nameof(headerReader));

        _dataReader = dataReader ??
            throw new ArgumentNullException(
                nameof(dataReader));

        _unitResolver = unitResolver ??
            throw new ArgumentNullException(
                nameof(unitResolver));
    }

    public CadConnectorResult<CadModelSnapshot> Read(
        string filePath)
    {
        var headerResult =
            _headerReader.Read(filePath);

        if (headerResult.Status ==
            CadConnectorResultStatus.Failed)
        {
            return CadConnectorResult<CadModelSnapshot>
                .Failure(headerResult.Diagnostics);
        }

        var dataResult =
            _dataReader.Read(filePath);

        if (dataResult.Status ==
            CadConnectorResultStatus.Failed)
        {
            return CadConnectorResult<CadModelSnapshot>
                .Failure(dataResult.Diagnostics);
        }

        var unitResult =
            _unitResolver.Resolve(
                dataResult.Value!);

        if (unitResult.Status ==
            CadConnectorResultStatus.Failed)
        {
            return CadConnectorResult<CadModelSnapshot>
                .Failure(unitResult.Diagnostics);
        }

        var header =
            headerResult.Value!;

        var source =
            new CadSourceInfo(
                sourceSystem: "STEP",
                sourceFormat:
                    ResolveSourceFormat(
                        header.Schemas),
                sourceFileName:
                    header.FileName ??
                    Path.GetFileName(filePath),
                sourceVersion:
                    header.ImplementationLevel,
                connectorVersion: "0.2.0");

        var diagnostics =
            headerResult.Diagnostics
                .Concat(dataResult.Diagnostics)
                .Concat(unitResult.Diagnostics)
                .ToList();

        var entities =
            ProjectEntities(
                dataResult.Value!);

        var snapshot =
            new CadModelSnapshot(
                source: source,
                units: unitResult.Value!,
                entities: entities,
                diagnostics: diagnostics);

        if (diagnostics.Count > 0)
        {
            return CadConnectorResult<CadModelSnapshot>
                .SuccessWithDiagnostics(
                    snapshot,
                    diagnostics);
        }

        return CadConnectorResult<CadModelSnapshot>
            .Success(snapshot);
    }

    private static IReadOnlyList<CadEntitySnapshot> ProjectEntities(
        StepDataInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return inventory.Entities
            .OrderBy(
                entity => ResolveEntityIdGroup(
                    entity.SourceEntityId))
            .ThenBy(
                entity => ResolveNumericEntityId(
                    entity.SourceEntityId))
            .ThenBy(
                entity => entity.SourceEntityId,
                StringComparer.Ordinal)
            .ThenBy(
                entity => entity.SourceEntityType,
                StringComparer.Ordinal)
            .ThenBy(
                entity => entity.RawStatement,
                StringComparer.Ordinal)
            .Select(
                entity =>
                    new CadEntitySnapshot(
                        sourceEntityId:
                            entity.SourceEntityId,
                        sourceEntityType:
                            entity.SourceEntityType,
                        category:
                            CadEntityCategory.Unknown,
                        readStatus:
                            entity.ReadStatus,
                        diagnostics:
                            entity.Diagnostics))
            .ToArray();
    }

    private static int ResolveEntityIdGroup(
        string? sourceEntityId)
    {
        if (TryResolveNumericEntityId(
                sourceEntityId,
                out _))
        {
            return 0;
        }

        return sourceEntityId is null
            ? 2
            : 1;
    }

    private static long ResolveNumericEntityId(
        string? sourceEntityId)
    {
        return TryResolveNumericEntityId(
            sourceEntityId,
            out var numericEntityId)
                ? numericEntityId
                : long.MaxValue;
    }

    private static bool TryResolveNumericEntityId(
        string? sourceEntityId,
        out long numericEntityId)
    {
        numericEntityId = default;

        if (string.IsNullOrWhiteSpace(
                sourceEntityId))
        {
            return false;
        }

        var normalizedId =
            sourceEntityId.Trim();

        if (normalizedId.StartsWith(
                '#'))
        {
            normalizedId =
                normalizedId[1..];
        }

        return long.TryParse(
            normalizedId,
            System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture,
            out numericEntityId);
    }

    private static string ResolveSourceFormat(
        IReadOnlyList<string> schemas)
    {
        if (schemas.Any(
                schema =>
                    schema.Contains(
                        "AP242",
                        StringComparison.OrdinalIgnoreCase)))
        {
            return "AP242";
        }

        if (schemas.Any(
                schema =>
                    schema.Contains(
                        "AP214",
                        StringComparison.OrdinalIgnoreCase) ||
                    schema.Contains(
                        "AUTOMOTIVE_DESIGN",
                        StringComparison.OrdinalIgnoreCase)))
        {
            return "AP214";
        }

        if (schemas.Any(
                schema =>
                    schema.Contains(
                        "AP203",
                        StringComparison.OrdinalIgnoreCase) ||
                    schema.Contains(
                        "CONFIG_CONTROL_DESIGN",
                        StringComparison.OrdinalIgnoreCase)))
        {
            return "AP203";
        }

        return "Unknown";
    }
}