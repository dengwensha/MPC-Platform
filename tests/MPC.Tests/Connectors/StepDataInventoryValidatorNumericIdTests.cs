using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Validation;

namespace MPC.Tests.Connectors;

public sealed class
    StepDataInventoryValidatorNumericIdTests
{
    [Fact]
    public void
        Validate_WithVeryLargeEquivalentSourceIds_ReportsDuplicate()
    {
        const string numericId =
            "1234567890123456789012345678901234567890";

        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    $"#{numericId}"),

                CreateEntity(
                    $"#000{numericId}")
            };

        var diagnostic =
            Assert.Single(
                validator.Validate(
                    entities));

        Assert.Equal(
            "STEP_DUPLICATE_SOURCE_ENTITY_ID",
            diagnostic.Code);

        Assert.Equal(
            $"#{numericId}",
            diagnostic.SourceEntityId);
    }

    [Fact]
    public void
        Validate_WithVeryLargeDeclaredReference_ReturnsNoDiagnostics()
    {
        const string numericId =
            "1234567890123456789012345678901234567890";

        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#1",
                    $"#{numericId}"),

                CreateEntity(
                    $"#{numericId}")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void
        Validate_WithVeryLargeMissingReference_ReturnsCanonicalDiagnostic()
    {
        const string numericId =
            "1234567890123456789012345678901234567890";

        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#1",
                    $"#000{numericId}")
            };

        var diagnostic =
            Assert.Single(
                validator.Validate(
                    entities));

        Assert.Equal(
            "STEP_MISSING_ENTITY_REFERENCE",
            diagnostic.Code);

        Assert.Equal(
            "#1",
            diagnostic.SourceEntityId);

        Assert.Contains(
            $"#{numericId}",
            diagnostic.Message);
    }

    [Fact]
    public void
        Validate_WithMalformedSourceIds_IgnoresNumericValidation()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity("#"),
                CreateEntity("#12A"),
                CreateEntity("12"),
                CreateEntity("#-1"),
                CreateEntity("#+1")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void
        Validate_WithMalformedReferences_IgnoresThoseReferences()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#1",
                    "#",
                    "#12A",
                    "12",
                    "#-1",
                    "#+1")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void
        Validate_WithLeadingZeroReference_RecognizesEquivalentTarget()
    {
        var validator =
            new StepDataInventoryValidator();

        var entities =
            new[]
            {
                CreateEntity(
                    "#1",
                    "#00025"),

                CreateEntity(
                    "#25")
            };

        var diagnostics =
            validator.Validate(
                entities);

        Assert.Empty(
            diagnostics);
    }

    private static StepDataEntityRecord
        CreateEntity(
            string sourceEntityId,
            params string[] referencedEntityIds)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                sourceEntityId,
            sourceEntityType:
                "PRODUCT",
            rawStatement:
                $"{sourceEntityId}=PRODUCT();",
            readStatus:
                CadReadStatus.Complete,
            referencedEntityIds:
                referencedEntityIds);
    }
}