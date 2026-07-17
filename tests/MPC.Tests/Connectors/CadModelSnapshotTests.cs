using MPC.Connectors.Diagnostics;
using MPC.Connectors.Snapshots;
using MPC.Connectors.Snapshots.Entities;
using MPC.Connectors.Snapshots.Source;
using MPC.Connectors.Snapshots.Units;

namespace MPC.Tests.Connectors;

public sealed class CadModelSnapshotTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesSnapshot()
    {
        var source = CreateSourceInfo();
        var units = CreateUnitInfo();

        var entity = CreateCompleteEntity(
            sourceEntityId: "#42",
            sourceEntityType: "ADVANCED_FACE",
            category: CadEntityCategory.Face);

        var snapshot = new CadModelSnapshot(
            source,
            units,
            new[]
            {
                entity
            });

        Assert.Same(source, snapshot.Source);
        Assert.Same(units, snapshot.Units);
        Assert.Single(snapshot.Entities);
        Assert.Equal(1, snapshot.EntityCount);
        Assert.Empty(snapshot.Diagnostics);
    }

    [Fact]
    public void Constructor_WithEmptyEntities_CreatesEmptySnapshot()
    {
        var snapshot = new CadModelSnapshot(
            CreateSourceInfo(),
            CreateUnitInfo(),
            Array.Empty<CadEntitySnapshot>());

        Assert.Empty(snapshot.Entities);
        Assert.Equal(0, snapshot.EntityCount);
    }

    [Fact]
    public void Constructor_CopiesEntityCollection()
    {
        var entities =
            new List<CadEntitySnapshot>
            {
                CreateCompleteEntity(
                    sourceEntityId: "#1",
                    sourceEntityType: "MANIFOLD_SOLID_BREP",
                    category: CadEntityCategory.Body)
            };

        var snapshot = new CadModelSnapshot(
            CreateSourceInfo(),
            CreateUnitInfo(),
            entities);

        entities.Clear();

        Assert.Single(snapshot.Entities);
        Assert.Equal(1, snapshot.EntityCount);
    }

    [Fact]
    public void Constructor_WithDiagnostics_StoresReadOnlyCopy()
    {
        var diagnostics =
            new List<CadDiagnostic>
            {
                new(
                    code: "STEP_MODEL_PARTIAL",
                    severity: CadDiagnosticSeverity.Warning,
                    message: "Model contains partial metadata.")
            };

        var snapshot = new CadModelSnapshot(
            CreateSourceInfo(),
            CreateUnitInfo(),
            Array.Empty<CadEntitySnapshot>(),
            diagnostics);

        diagnostics.Clear();

        Assert.Single(snapshot.Diagnostics);
    }

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CadModelSnapshot(
                source: null!,
                units: CreateUnitInfo(),
                entities: Array.Empty<CadEntitySnapshot>()));
    }

    [Fact]
    public void Constructor_WithNullUnits_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CadModelSnapshot(
                source: CreateSourceInfo(),
                units: null!,
                entities: Array.Empty<CadEntitySnapshot>()));
    }

    [Fact]
    public void Constructor_WithNullEntities_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CadModelSnapshot(
                source: CreateSourceInfo(),
                units: CreateUnitInfo(),
                entities: null!));
    }

    [Fact]
    public void Constructor_WithNullEntityEntry_ThrowsArgumentException()
    {
        CadEntitySnapshot?[] entities =
        {
            null
        };

        Assert.Throws<ArgumentException>(
            () => new CadModelSnapshot(
                CreateSourceInfo(),
                CreateUnitInfo(),
                entities!));
    }

    [Fact]
    public void Constructor_WithNullDiagnosticEntry_ThrowsArgumentException()
    {
        CadDiagnostic?[] diagnostics =
        {
            null
        };

        Assert.Throws<ArgumentException>(
            () => new CadModelSnapshot(
                CreateSourceInfo(),
                CreateUnitInfo(),
                Array.Empty<CadEntitySnapshot>(),
                diagnostics!));
    }

    private static CadEntitySnapshot CreateCompleteEntity(
        string sourceEntityId,
        string sourceEntityType,
        CadEntityCategory category)
    {
        return new CadEntitySnapshot(
            sourceEntityId,
            sourceEntityType,
            category,
            CadReadStatus.Complete);
    }

    private static CadSourceInfo CreateSourceInfo()
    {
        return new CadSourceInfo(
            sourceSystem: "STEP",
            sourceFormat: "AP242",
            sourceFileName: "GoldenPart.step",
            sourceVersion: "Edition 3",
            connectorVersion: "1.0.0");
    }

    private static CadUnitInfo CreateUnitInfo()
    {
        return new CadUnitInfo(
            sourceLengthUnit: CadLengthUnit.Millimetre,
            sourceAngleUnit: CadAngleUnit.Degree);
    }
}