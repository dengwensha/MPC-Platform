using MPC.Connectors.Results;
using MPC.Connectors.Step;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Resolution;
using MPC.Connectors.Step.Validation;

if (args.Length != 1)
{
    Console.Error.WriteLine(
        "Usage: MPC.Connectors.Step.Runner <file.step|file.stp>");

    return 1;
}

var filePath =
    args[0];

var headerReader =
    new StepHeaderReader();

var inventoryValidator =
    new StepDataInventoryValidator();

var dataReader =
    new StepDataEntityReader(
        inventoryValidator);

var unitResolver =
    new StepUnitResolver();

var connector =
    new StepConnector(
        headerReader,
        dataReader,
        unitResolver);

var connectorResult =
    connector.Read(filePath);

var dataResult =
    dataReader.Read(filePath);

Console.WriteLine(
    "STEP Connector Result");

Console.WriteLine(
    "---------------------");

Console.WriteLine(
    $"Status: {connectorResult.Status}");

if (connectorResult.Value is not null)
{
    var snapshot =
        connectorResult.Value;

    Console.WriteLine();
    Console.WriteLine("Source");
    Console.WriteLine("------");

    Console.WriteLine(
        $"System: " +
        $"{snapshot.Source.SourceSystem}");

    Console.WriteLine(
        $"Format: " +
        $"{snapshot.Source.SourceFormat}");

    Console.WriteLine(
        $"File: " +
        $"{snapshot.Source.SourceFileName}");

    Console.WriteLine(
        $"Source Version: " +
        $"{snapshot.Source.SourceVersion ?? "Unknown"}");

    Console.WriteLine(
        $"Connector Version: " +
        $"{snapshot.Source.ConnectorVersion}");

    Console.WriteLine();
    Console.WriteLine("Units");
    Console.WriteLine("-----");

    Console.WriteLine(
        $"Source Length Unit: " +
        $"{snapshot.Units.SourceLengthUnit}");

    Console.WriteLine(
        $"Source Angle Unit: " +
        $"{snapshot.Units.SourceAngleUnit}");

    Console.WriteLine(
        $"Internal Length Unit: " +
        $"{snapshot.Units.InternalLengthUnit}");

    Console.WriteLine(
        $"Internal Angle Unit: " +
        $"{snapshot.Units.InternalAngleUnit}");

    Console.WriteLine();
    Console.WriteLine("Snapshot");
    Console.WriteLine("--------");

    Console.WriteLine(
        $"Entity Count: " +
        $"{snapshot.EntityCount}");
}

Console.WriteLine();
Console.WriteLine(
    "Connector Diagnostics");

Console.WriteLine(
    "---------------------");

if (connectorResult.Diagnostics.Count == 0)
{
    Console.WriteLine("None");
}
else
{
    foreach (var diagnostic
             in connectorResult.Diagnostics)
    {
        Console.WriteLine(
            $"[{diagnostic.Severity}] " +
            $"{diagnostic.Code}: " +
            $"{diagnostic.Message}");
    }
}

Console.WriteLine();
Console.WriteLine(
    "STEP DATA Inventory");

Console.WriteLine(
    "-------------------");

Console.WriteLine(
    $"Status: {dataResult.Status}");

if (dataResult.Value is not null)
{
    Console.WriteLine(
        $"Total Entities: " +
        $"{dataResult.Value.TotalEntityCount}");

    Console.WriteLine();
    Console.WriteLine(
        "Entity Type Counts");

    Console.WriteLine(
        "------------------");

    foreach (var entry
             in dataResult.Value.TypeCounts
                 .OrderByDescending(
                     item => item.Value)
                 .ThenBy(
                     item => item.Key))
    {
        Console.WriteLine(
            $"{entry.Key,-48} " +
            $"{entry.Value,8}");
    }

    var complexEntities =
        dataResult.Value.Entities
            .Where(
                entity =>
                    entity.SourceEntityType ==
                    "COMPLEX_ENTITY")
            .ToList();

    if (complexEntities.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Complex STEP Entities");

        Console.WriteLine(
            "---------------------");

        foreach (StepDataEntityRecord entity
                 in complexEntities)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Source ID: " +
                $"{entity.SourceEntityId ?? "Unknown"}");

            Console.WriteLine(
                $"Components: " +
                $"{string.Join(", ", entity.ComponentTypes)}");

            Console.WriteLine(
                entity.RawStatement);
        }
    }

    var unknownEntities =
        dataResult.Value.Entities
            .Where(
                entity =>
                    entity.SourceEntityType ==
                    "UNKNOWN")
            .ToList();

    if (unknownEntities.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Unknown STEP Statements");

        Console.WriteLine(
            "-----------------------");

        foreach (StepDataEntityRecord entity
                 in unknownEntities)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Source ID: " +
                $"{entity.SourceEntityId ?? "Unknown"}");

            Console.WriteLine(
                entity.RawStatement);
        }
    }
}

Console.WriteLine();
Console.WriteLine(
    "DATA Diagnostics");

Console.WriteLine(
    "----------------");

if (dataResult.Diagnostics.Count == 0)
{
    Console.WriteLine("None");
}
else
{
    foreach (var diagnostic
             in dataResult.Diagnostics)
    {
        Console.WriteLine(
            $"[{diagnostic.Severity}] " +
            $"{diagnostic.Code}: " +
            $"{diagnostic.Message}");
    }
}

var failed =
    connectorResult.Status ==
        CadConnectorResultStatus.Failed ||
    dataResult.Status ==
        CadConnectorResultStatus.Failed;

return failed
    ? 1
    : 0;