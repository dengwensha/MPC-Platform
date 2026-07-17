using MPC.Connectors.Results;
using MPC.Connectors.Snapshots;
using MPC.Connectors.Snapshots.Entities;
using MPC.Connectors.Snapshots.Source;
using MPC.Connectors.Step.Contracts;

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

        var snapshot =
            new CadModelSnapshot(
                source: source,
                units: unitResult.Value!,
                entities:
                    Array.Empty<CadEntitySnapshot>(),
                diagnostics:
                    diagnostics);

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