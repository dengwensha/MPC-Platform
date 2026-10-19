using System.Security.Cryptography;
using MPC.Application.Step;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping.Step;

namespace MPC.Application.Manufacturing;

/// <summary>Analyzes a private byte-for-byte STEP snapshot, so the region's
/// document identity and mapped surfaces derive from the same input bytes.</summary>
public static class ManufacturingRegionImportService
{
    public static bool TryCreate(
        string inputFilePath,
        string sourceFaceId,
        out ManufacturingRegion? region)
    {
        region = null;
        if (string.IsNullOrWhiteSpace(inputFilePath)
            || string.IsNullOrWhiteSpace(sourceFaceId))
            return false;

        // Read the source once. A later modification cannot change this snapshot.
        byte[] sourceBytes = File.ReadAllBytes(inputFilePath);
        string fingerprint = Convert.ToHexString(SHA256.HashData(sourceBytes));
        string snapshotPath = Path.Combine(
            Path.GetTempPath(),
            "mpc-face-" + Guid.NewGuid().ToString("N") + ".step");

        try
        {
            File.WriteAllBytes(snapshotPath, sourceBytes);
            var reader = new StepDataEntityReader(new StepDataInventoryValidator());
            var inventoryResult = reader.Read(snapshotPath);
            StepDataInventory? inventory = inventoryResult.Value;
            if (inventory is null || inventoryResult.Status ==
                MPC.Connectors.Results.CadConnectorResultStatus.Failed)
                return false;

            var semanticReader = new StepSemanticEntityReader(
                new StepParameterParser(), StepSemanticHandlerCatalog.Create());
            StepSemanticEntity[] entities = inventory.Entities
                .Select(record => semanticReader.Read(record))
                .OfType<StepSemanticEntity>()
                .ToArray();
            StepManifoldSolidBrepEntity[] roots = entities
                .OfType<StepManifoldSolidBrepEntity>().ToArray();
            if (roots.Length != 1) return false;

            var operation = StepCanonicalMappingOperation.Create(
                new StepSemanticEntityResolver(entities));
            var mapped = operation.Map(roots[0]);
            if (!mapped.IsSuccessful || mapped.Value is null) return false;

            return ManufacturingRegion.TryCreate(
                operation, fingerprint, sourceFaceId, out region);
        }
        finally
        {
            File.Delete(snapshotPath);
        }
    }
}
