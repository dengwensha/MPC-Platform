using MPC.Connectors.Diagnostics;
using MPC.Connectors.Snapshots.Entities;

namespace MPC.Tests.Connectors;

public sealed class CadEntitySnapshotTests
{
    [Fact]
    public void Constructor_WithCompleteEntity_CreatesSnapshot()
    {
        var entity = new CadEntitySnapshot(
            sourceEntityId: "#42",
            sourceEntityType: "ADVANCED_FACE",
            category: CadEntityCategory.Face,
            readStatus: CadReadStatus.Complete);

        Assert.Equal("#42", entity.SourceEntityId);
        Assert.Equal(
            "ADVANCED_FACE",
            entity.SourceEntityType);

        Assert.Equal(
            CadEntityCategory.Face,
            entity.Category);

        Assert.Equal(
            CadReadStatus.Complete,
            entity.ReadStatus);

        Assert.Empty(entity.Diagnostics);
    }

    [Fact]
    public void Constructor_WithPartialEntityAndErrorDiagnostic_CreatesSnapshot()
    {
        var diagnostic = new CadDiagnostic(
            code: "STEP_FACE_PARTIAL",
            severity: CadDiagnosticSeverity.Error,
            message: "Face parameters could not be read completely.",
            sourceEntityId: "#42",
            sourceEntityType: "ADVANCED_FACE");

        var entity = new CadEntitySnapshot(
            sourceEntityId: "#42",
            sourceEntityType: "ADVANCED_FACE",
            category: CadEntityCategory.Face,
            readStatus: CadReadStatus.Partial,
            diagnostics: new[]
            {
                diagnostic
            });

        Assert.Equal(
            CadReadStatus.Partial,
            entity.ReadStatus);

        Assert.Single(entity.Diagnostics);
    }

    [Fact]
    public void Constructor_WithFailedEntityAndFatalDiagnostic_CreatesSnapshot()
    {
        var diagnostic = new CadDiagnostic(
            code: "STEP_ENTITY_FAILED",
            severity: CadDiagnosticSeverity.Fatal,
            message: "Entity could not be read.");

        var entity = new CadEntitySnapshot(
            sourceEntityId: null,
            sourceEntityType: "UNKNOWN_ENTITY",
            category: CadEntityCategory.Unknown,
            readStatus: CadReadStatus.Failed,
            diagnostics: new[]
            {
                diagnostic
            });

        Assert.Equal(
            CadReadStatus.Failed,
            entity.ReadStatus);

        Assert.Single(entity.Diagnostics);
    }

    [Fact]
    public void Constructor_WithCompleteEntityAndErrorDiagnostic_ThrowsInvalidOperationException()
    {
        var diagnostic = new CadDiagnostic(
            code: "ERROR",
            severity: CadDiagnosticSeverity.Error,
            message: "Error.");

        Assert.Throws<InvalidOperationException>(
            () => new CadEntitySnapshot(
                sourceEntityId: "#1",
                sourceEntityType: "FACE",
                category: CadEntityCategory.Face,
                readStatus: CadReadStatus.Complete,
                diagnostics: new[]
                {
                    diagnostic
                }));
    }

    [Fact]
    public void Constructor_WithPartialEntityAndNoDiagnostics_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => new CadEntitySnapshot(
                sourceEntityId: "#1",
                sourceEntityType: "FACE",
                category: CadEntityCategory.Face,
                readStatus: CadReadStatus.Partial));
    }

    [Fact]
    public void Constructor_WithFailedEntityAndWarningOnly_ThrowsInvalidOperationException()
    {
        var diagnostic = new CadDiagnostic(
            code: "WARNING",
            severity: CadDiagnosticSeverity.Warning,
            message: "Warning.");

        Assert.Throws<InvalidOperationException>(
            () => new CadEntitySnapshot(
                sourceEntityId: "#1",
                sourceEntityType: "FACE",
                category: CadEntityCategory.Face,
                readStatus: CadReadStatus.Failed,
                diagnostics: new[]
                {
                    diagnostic
                }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSourceEntityType_ThrowsArgumentException(
        string? sourceEntityType)
    {
        Assert.Throws<ArgumentException>(
            () => new CadEntitySnapshot(
                sourceEntityId: "#1",
                sourceEntityType: sourceEntityType!,
                category: CadEntityCategory.Face,
                readStatus: CadReadStatus.Complete));
    }

    [Fact]
    public void Diagnostics_AreCopiedAsReadOnlyCollection()
    {
        var diagnostic = new CadDiagnostic(
            code: "ERROR",
            severity: CadDiagnosticSeverity.Error,
            message: "Error.");

        var diagnostics =
            new List<CadDiagnostic>
            {
                diagnostic
            };

        var entity = new CadEntitySnapshot(
            sourceEntityId: "#1",
            sourceEntityType: "FACE",
            category: CadEntityCategory.Face,
            readStatus: CadReadStatus.Partial,
            diagnostics: diagnostics);

        diagnostics.Clear();

        Assert.Single(entity.Diagnostics);
    }
}
