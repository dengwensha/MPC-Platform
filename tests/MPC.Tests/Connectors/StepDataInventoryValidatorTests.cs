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
            "STEP_DUPLICATE_SOURCE_ENTITY_ID",
            diagnostic.Code);

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
                        diagnostic.SourceEntityId)
                .ToArray());

        Assert.All(
            diagnostics,
            diagnostic =>
                Assert.Equal(
                    "STEP_DUPLICATE_SOURCE_ENTITY_ID",
                    diagnostic.Code));
    }

    [Fact]
    public void Validate_WithExistingReference_ReturnsNoDiagnostics()
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
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#25"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void Validate_WithMissingReference_ReturnsDiagnostic()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#999"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        var diagnostic =
            Assert.Single(
                diagnostics);

        Assert.Equal(
            "STEP_MISSING_ENTITY_REFERENCE",
            diagnostic.Code);

        Assert.Equal(
            CadDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Equal(
            "#42",
            diagnostic.SourceEntityId);

        Assert.Equal(
            "TEST_ENTITY",
            diagnostic.SourceEntityType);

        Assert.Contains(
            "#999",
            diagnostic.Message);

        Assert.Contains(
            "preserved",
            diagnostic.Message);
    }

    [Fact]
    public void Validate_WithEquivalentNumericReference_RecognizesDeclaredTarget()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#025",
                    "CARTESIAN_POINT"),

                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#25"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void Validate_WithDuplicateTargetIds_DoesNotReportMissingReference()
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
                    "DIRECTION"),

                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#25"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        var duplicateDiagnostic =
            Assert.Single(
                diagnostics);

        Assert.Equal(
            "STEP_DUPLICATE_SOURCE_ENTITY_ID",
            duplicateDiagnostic.Code);

        Assert.Equal(
            "#25",
            duplicateDiagnostic.SourceEntityId);

        Assert.DoesNotContain(
            diagnostics,
            diagnostic =>
                diagnostic.Code ==
                "STEP_MISSING_ENTITY_REFERENCE");
    }

    [Fact]
    public void Validate_WithRepeatedMissingReference_ReturnsSingleDiagnostic()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#999",
                            "#999",
                            "#999"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        var diagnostic =
            Assert.Single(
                diagnostics);

        Assert.Equal(
            "STEP_MISSING_ENTITY_REFERENCE",
            diagnostic.Code);

        Assert.Equal(
            "#42",
            diagnostic.SourceEntityId);

        Assert.Contains(
            "#999",
            diagnostic.Message);
    }

    [Fact]
    public void Validate_WithEquivalentRepeatedMissingReferences_ReturnsSingleDiagnostic()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#999",
                            "#0999",
                            "#000999"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        var diagnostic =
            Assert.Single(
                diagnostics);

        Assert.Equal(
            "STEP_MISSING_ENTITY_REFERENCE",
            diagnostic.Code);

        Assert.Equal(
            "#42",
            diagnostic.SourceEntityId);

        Assert.Contains(
            "#999",
            diagnostic.Message);
    }

    [Fact]
    public void Validate_WithMultipleMissingReferences_ReturnsNumericTargetOrder()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#100",
                            "#75",
                            "#100"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Equal(
            2,
            diagnostics.Count);

        Assert.All(
            diagnostics,
            diagnostic =>
                Assert.Equal(
                    "STEP_MISSING_ENTITY_REFERENCE",
                    diagnostic.Code));

        Assert.Contains(
            "#75",
            diagnostics[0].Message);

        Assert.Contains(
            "#100",
            diagnostics[1].Message);
    }

    [Fact]
    public void Validate_WithMissingReferencesFromMultipleSources_ReturnsNumericSourceOrder()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#100",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#999"
                        }),

                CreateEntity(
                    "#2",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#500"
                        }),

                CreateEntity(
                    "#10",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#800"
                        })
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
                        diagnostic.SourceEntityId)
                .ToArray());

        Assert.Contains(
            "#500",
            diagnostics[0].Message);

        Assert.Contains(
            "#800",
            diagnostics[1].Message);

        Assert.Contains(
            "#999",
            diagnostics[2].Message);
    }

    [Fact]
    public void Validate_WithDuplicateAndMissingReference_ReturnsDeterministicOrder()
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
                    "DIRECTION"),

                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#999"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Equal(
            2,
            diagnostics.Count);

        Assert.Equal(
            "STEP_DUPLICATE_SOURCE_ENTITY_ID",
            diagnostics[0].Code);

        Assert.Equal(
            "#25",
            diagnostics[0].SourceEntityId);

        Assert.Equal(
            "STEP_MISSING_ENTITY_REFERENCE",
            diagnostics[1].Code);

        Assert.Equal(
            "#42",
            diagnostics[1].SourceEntityId);

        Assert.Contains(
            "#999",
            diagnostics[1].Message);
    }

    [Fact]
    public void Validate_WithDuplicateAndMissingReferenceOnSameSource_ReturnsDuplicateFirst()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#25",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#999"
                        }),

                CreateEntity(
                    "#025",
                    "TEST_ENTITY")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Equal(
            2,
            diagnostics.Count);

        Assert.Equal(
            "STEP_DUPLICATE_SOURCE_ENTITY_ID",
            diagnostics[0].Code);

        Assert.Equal(
            "#25",
            diagnostics[0].SourceEntityId);

        Assert.Equal(
            "STEP_MISSING_ENTITY_REFERENCE",
            diagnostics[1].Code);

        Assert.Equal(
            "#25",
            diagnostics[1].SourceEntityId);

        Assert.Contains(
            "#999",
            diagnostics[1].Message);
    }

    [Fact]
    public void Validate_WithReferenceToLaterEntity_ReturnsNoDiagnostics()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#1",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "#100"
                        }),

                CreateEntity(
                    "#100",
                    "CARTESIAN_POINT")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
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
                    sourceEntityId:
                        null,
                    sourceEntityType:
                        "UNKNOWN"),

                CreateEntity(
                    sourceEntityId:
                        null,
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
    public void Validate_WithUnrecognizedReferenceId_IgnoresThatReference()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#42",
                    "TEST_ENTITY",
                    referencedEntityIds:
                        new[]
                        {
                            "INVALID_REFERENCE",
                            "#",
                            "#ABC"
                        })
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void Validate_WithNullEntityEntry_ThrowsArgumentNullException()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new StepDataEntityRecord[]
            {
                CreateEntity(
                    "#1",
                    "PRODUCT"),

                null!
            };

        Assert.Throws<ArgumentNullException>(
            () =>
                validator.Validate(
                    entities));
    }

    [Fact]
    public void Validate_WithNullEntities_ThrowsArgumentNullException()
    {
        var validator =
            new StepDataInventoryValidator();

        Assert.Throws<ArgumentNullException>(
            () =>
                validator.Validate(
                    entities:
                        null!));
    }

    private static StepDataEntityRecord
        CreateEntity(
            string? sourceEntityId,
            string sourceEntityType,
            IEnumerable<string>? referencedEntityIds = null)
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
                CadReadStatus.Complete,
            referencedEntityIds:
                referencedEntityIds);
    }
}