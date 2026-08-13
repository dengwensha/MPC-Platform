using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;

namespace MPC.Benchmarks;

internal static class StepBenchmarkSupport
{
    public static string GetGoldenStepFilePath(
        string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "Golden STEP file name cannot be null, empty, or whitespace.",
                nameof(fileName));
        }

        var currentDirectory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidatePath =
                Path.Combine(
                    currentDirectory.FullName,
                    "tests",
                    "MPC.Tests",
                    "TestData",
                    "Step",
                    "Golden",
                    fileName);

            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            currentDirectory =
                currentDirectory.Parent;
        }

        throw new FileNotFoundException(
            $"Golden STEP file '{fileName}' could not be located.");
    }

    public static StepDataEntityReader CreateDataReader()
    {
        return new StepDataEntityReader(
            new StepDataInventoryValidator());
    }

    public static StepDataInventory ReadInventory(
        StepDataEntityReader dataReader,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(dataReader);

        var result =
            dataReader.Read(filePath);

        if (result.Status != CadConnectorResultStatus.Succeeded ||
            result.Diagnostics.Count != 0 ||
            result.Value is not StepDataInventory inventory)
        {
            throw new InvalidOperationException(
                $"STEP DATA inventory could not be read from '{filePath}'.");
        }

        return inventory;
    }

    public static StepSemanticEntityReader
        CreateSemanticReader()
    {
        IStepSemanticEntityHandler[] handlers =
        [
            new StepCartesianPointEntityHandler(),
            new StepDirectionEntityHandler(),
            new StepVectorEntityHandler(),
            new StepLineEntityHandler(),
            new StepVertexPointEntityHandler(),
            new StepEdgeCurveEntityHandler(),
            new StepOrientedEdgeEntityHandler(),
            new StepEdgeLoopEntityHandler(),
            new StepFaceOuterBoundEntityHandler(),
            new StepAdvancedFaceEntityHandler(),
            new StepPlaneEntityHandler(),
            new StepAxis2Placement3DEntityHandler(),
            new StepClosedShellEntityHandler(),
            new StepManifoldSolidBrepEntityHandler()
        ];

        return new StepSemanticEntityReader(
            new StepParameterParser(),
            handlers);
    }

    public static List<StepSemanticEntity>
        ReadSemanticEntities(
            StepSemanticEntityReader semanticReader,
            StepDataInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(semanticReader);
        ArgumentNullException.ThrowIfNull(inventory);

        var semanticEntities =
            new List<StepSemanticEntity>();

        foreach (StepDataEntityRecord record
                 in inventory.Entities)
        {
            StepSemanticEntity? semanticEntity =
                semanticReader.Read(record);

            if (semanticEntity is not null)
            {
                semanticEntities.Add(semanticEntity);
            }
        }

        return semanticEntities;
    }
}