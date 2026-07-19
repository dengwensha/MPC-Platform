using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Validation;

namespace MPC.Tests.Connectors;

public sealed class StepDataInventoryValidatorTests
{
    [Fact]
    public void Validate_WithUniqueEntityIds_ReturnsNoDiagnostics()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#1",
                    "APPLICATION_CONTEXT"),

                CreateEntity(
                    "#2",
                    "PRODUCT"),

                CreateEntity(
                    "#25",
                    "CARTESIAN_POINT")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void Validate_WithDuplicateEntityId_ReturnsSingleDiagnostic()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#25",
                    "CARTESIAN_POINT"),

                CreateEntity(
                    "#25",
                    "DIRECTION")
            };

        var diagnostics =
            validator.Validate(
                entities);

        var diagnostic =
            Assert.Single(
                diagnostics);

        Assert.Equal(
            "STEP_DUPLICATE_SOURCE_ENTITY_ID",
            diagnostic.Code);

        Assert.Equal(
            CadDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Equal(
            "#25",
            diagnostic.SourceEntityId);

        Assert.Equal(
            "MULTIPLE",
            diagnostic.SourceEntityType);

        Assert.Contains(
            "declared 2 times",
            diagnostic.Message);

        Assert.Contains(
            "preserved",
            diagnostic.Message);
    }

    [Fact]
    public void Validate_WithEquivalentNumericIds_ReportsDuplicate()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#25",
                    "CARTESIAN_POINT"),

                CreateEntity(
                    "#025",
                    "CARTESIAN_POINT"),

                CreateEntity(
                    "#00025",
                    "CARTESIAN_POINT")
            };

        var diagnostics =
            validator.Validate(
                entities);

        var diagnostic =
            Assert.Single(
                diagnostics);

        Assert.Equal(
            "#25",
            diagnostic.SourceEntityId);

        Assert.Equal(
            "CARTESIAN_POINT",
            diagnostic.SourceEntityType);

        Assert.Contains(
            "declared 3 times",
            diagnostic.Message);
    }

    [Fact]
    public void Validate_WithMultipleDuplicateGroups_ReturnsNumericOrder()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#100",
                    "PRODUCT"),

                CreateEntity(
                    "#2",
                    "PRODUCT"),

                CreateEntity(
                    "#100",
                    "PRODUCT"),

                CreateEntity(
                    "#10",
                    "PRODUCT"),

                CreateEntity(
                    "#2",
                    "PRODUCT"),

                CreateEntity(
                    "#10",
                    "PRODUCT")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Equal(
            3,
            diagnostics.Count);

        Assert.Equal(
            new[]
            {
                "#2",
                "#10",
                "#100"
            },
            diagnostics
                .Select(
                    diagnostic =>
                        diagnostic
                            .SourceEntityId)
                .ToArray());
    }

    [Fact]
    public void Validate_WithNullAndUnrecognizedIds_IgnoresThoseIds()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    sourceEntityId: null,
                    sourceEntityType:
                        "UNKNOWN"),

                CreateEntity(
                    sourceEntityId: null,
                    sourceEntityType:
                        "UNKNOWN")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void Validate_WithNullEntities_ThrowsArgumentNullException()
    {
        var validator =
            new StepDataInventoryValidator();

        Assert.Throws<ArgumentNullException>(
            () => validator.Validate(
                entities: null!));
    }

    private static StepDataEntityRecord
        CreateEntity(
            string? sourceEntityId,
            string sourceEntityType)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                sourceEntityId,
            sourceEntityType:
                sourceEntityType,
            rawStatement:
                sourceEntityId is null
                    ? "INVALID DATA RECORD;"
                    : $"{sourceEntityId} = " +
                      $"{sourceEntityType}();",
            readStatus:
                CadReadStatus.Complete);
    }
}