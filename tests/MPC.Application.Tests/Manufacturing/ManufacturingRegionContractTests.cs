using MPC.Application.Manufacturing;
using MPC.Mapping.Step;

namespace MPC.Application.Tests.Manufacturing;

public sealed class ManufacturingRegionContractTests
{
    [Fact]
    public void UnmappedFace_IsNeverAccepted()
    {
        // No mapping has been published; unknown source identity must fail.
        var operation = StepCanonicalMappingOperation.Create(
            new MPC.Connectors.Step.Semantics.StepSemanticEntityResolver(
                Array.Empty<MPC.Connectors.Step.Semantics.StepSemanticEntity>()));
        bool ok = ManufacturingRegion.TryCreate(
            operation, new string('a', 64), "#100", out var region);
        Assert.False(ok);
        Assert.Null(region);
    }

    [Theory]
    [InlineData("")]
    [InlineData("wrong-hash")]
    public void InvalidFingerprint_IsRejected(string fingerprint)
    {
        var operation = StepCanonicalMappingOperation.Create(
            new MPC.Connectors.Step.Semantics.StepSemanticEntityResolver(
                Array.Empty<MPC.Connectors.Step.Semantics.StepSemanticEntity>()));
        Assert.False(ManufacturingRegion.TryCreate(
            operation, fingerprint, "#100", out _));
    }
    [Fact]
    public void GoldenPrism_MapsRealSourceFacesAndBuildsPlanarRegion()
    {
        string filePath = GetGoldenStepFilePath("T001_PRISM.step");
        var reader = new MPC.Connectors.Step.Readers.StepDataEntityReader(
            new MPC.Connectors.Step.Validation.StepDataInventoryValidator());
        var inventoryResult = reader.Read(filePath);
        var inventory = Assert.IsType<MPC.Connectors.Step.Models.StepDataInventory>(inventoryResult.Value);
        MPC.Connectors.Step.Semantics.IStepSemanticEntityHandler[] handlers =
        [
            new MPC.Connectors.Step.Semantics.StepCartesianPointEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepDirectionEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepVectorEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepLineEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepVertexPointEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepEdgeCurveEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepOrientedEdgeEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepEdgeLoopEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepFaceOuterBoundEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepAdvancedFaceEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepPlaneEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepAxis2Placement3DEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepClosedShellEntityHandler(),
            new MPC.Connectors.Step.Semantics.StepManifoldSolidBrepEntityHandler()
        ];
        var semanticReader = new MPC.Connectors.Step.Semantics.StepSemanticEntityReader(
            new MPC.Connectors.Step.Parsing.StepParameterParser(), handlers);
        var entities = inventory.Entities
            .Select(record => semanticReader.Read(record))
            .OfType<MPC.Connectors.Step.Semantics.StepSemanticEntity>()
            .ToArray();
        var resolver = new MPC.Connectors.Step.Semantics.StepSemanticEntityResolver(entities);
        var root = Assert.Single(entities.OfType<MPC.Connectors.Step.Semantics.StepManifoldSolidBrepEntity>());
        var operation = StepCanonicalMappingOperation.Create(resolver);

        Assert.False(operation.FaceIdentities.TryGetFace("#8", out _));
        var mapped = operation.Map(root);
        Assert.True(mapped.IsSuccessful);
        Assert.NotNull(mapped.Value);

        var fingerprint = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(filePath)));
        string[] expected = ["#8", "#30", "#62", "#76", "#143", "#174"];
        foreach (string id in expected)
        {
            Assert.True(operation.FaceIdentities.TryGetFace(id, out var face));
            Assert.NotNull(face);
            Assert.True(ManufacturingRegion.TryCreate(
                operation, fingerprint, id, out var region));
            Assert.NotNull(region);
            Assert.Equal(id, region.SourceFaceId);
            Assert.Equal(face.Id, region.CanonicalFaceId);
            Assert.Equal(fingerprint, region.DocumentFingerprint);
            Assert.True(region.OuterBoundaryEdgeCount >= 3);
            Assert.Equal(0, region.InnerBoundaryCount);
        }

        // Reprocessing the same semantic source must replace session-local GUIDs
        // without losing the persistent STEP source references.
        Assert.True(operation.FaceIdentities.TryGetFace("#8", out var firstFace));
        var secondMap = operation.Map(root);
        Assert.True(secondMap.IsSuccessful);
        Assert.True(operation.FaceIdentities.TryGetFace("#8", out var secondFace));
        Assert.NotNull(firstFace);
        Assert.NotNull(secondFace);
        Assert.NotSame(firstFace, secondFace);
        Assert.NotEqual(firstFace.Id, secondFace.Id);
        Assert.True(ManufacturingRegion.TryCreate(
            operation, fingerprint, "#8", out var secondRegion));
        Assert.Equal(secondFace.Id, secondRegion!.CanonicalFaceId);

        Assert.False(operation.FaceIdentities.TryGetFace("#999999", out _));
        Assert.False(ManufacturingRegion.TryCreate(
            operation, fingerprint, "#999999", out _));
    }

    private static string GetGoldenStepFilePath(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "TestData",
                "Step", "Golden", fileName);
            if (File.Exists(candidate)) return candidate;
            candidate = Path.Combine(directory.FullName, "tests", "MPC.Tests",
                "TestData", "Step", "Golden", fileName);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException(fileName);
    }

}
