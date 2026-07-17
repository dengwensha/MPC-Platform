using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Snapshots.Units;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Resolution;

namespace MPC.Tests.Connectors;

public sealed class StepUnitResolverTests
{
    [Fact]
    public void Resolve_WithMillimetreAndRadian_ReturnsSucceededResult()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateComplexEntity(
                        "#16",
                        """
                        #16 =(
                            LENGTH_UNIT()
                            NAMED_UNIT(*)
                            SI_UNIT(.MILLI.,.METRE.)
                        );
                        """,
                        "LENGTH_UNIT",
                        "NAMED_UNIT",
                        "SI_UNIT"),

                    CreateComplexEntity(
                        "#38",
                        """
                        #38 =(
                            NAMED_UNIT(*)
                            PLANE_ANGLE_UNIT()
                            SI_UNIT($,.RADIAN.)
                        );
                        """,
                        "NAMED_UNIT",
                        "PLANE_ANGLE_UNIT",
                        "SI_UNIT")
                });

        var resolver =
            new StepUnitResolver();

        var result =
            resolver.Resolve(inventory);

        Assert.Equal(
            CadConnectorResultStatus.Succeeded,
            result.Status);

        Assert.NotNull(result.Value);

        Assert.Equal(
            CadLengthUnit.Millimetre,
            result.Value.SourceLengthUnit);

        Assert.Equal(
            CadAngleUnit.Radian,
            result.Value.SourceAngleUnit);

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Resolve_WithMetreAndRadian_MapsBaseSiUnits()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateComplexEntity(
                        "#1",
                        """
                        #1 =(
                            LENGTH_UNIT()
                            NAMED_UNIT(*)
                            SI_UNIT($,.METRE.)
                        );
                        """,
                        "LENGTH_UNIT",
                        "NAMED_UNIT",
                        "SI_UNIT"),

                    CreateComplexEntity(
                        "#2",
                        """
                        #2 =(
                            NAMED_UNIT(*)
                            PLANE_ANGLE_UNIT()
                            SI_UNIT($,.RADIAN.)
                        );
                        """,
                        "NAMED_UNIT",
                        "PLANE_ANGLE_UNIT",
                        "SI_UNIT")
                });

        var resolver =
            new StepUnitResolver();

        var result =
            resolver.Resolve(inventory);

        Assert.Equal(
            CadConnectorResultStatus.Succeeded,
            result.Status);

        Assert.NotNull(result.Value);

        Assert.Equal(
            CadLengthUnit.Metre,
            result.Value.SourceLengthUnit);

        Assert.Equal(
            CadAngleUnit.Radian,
            result.Value.SourceAngleUnit);

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Resolve_WithoutUnits_ReturnsUnknownWithDiagnostics()
    {
        var inventory =
            new StepDataInventory(
                Array.Empty<StepDataEntityRecord>());

        var resolver =
            new StepUnitResolver();

        var result =
            resolver.Resolve(inventory);

        Assert.Equal(
            CadConnectorResultStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.NotNull(result.Value);

        Assert.Equal(
            CadLengthUnit.Unknown,
            result.Value.SourceLengthUnit);

        Assert.Equal(
            CadAngleUnit.Unknown,
            result.Value.SourceAngleUnit);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code ==
                "STEP_LENGTH_UNIT_NOT_RESOLVED");

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code ==
                "STEP_ANGLE_UNIT_NOT_RESOLVED");
    }

    [Fact]
    public void Resolve_IgnoresSolidAngleUnit()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateComplexEntity(
                        "#5",
                        """
                        #5 =(
                            NAMED_UNIT(*)
                            SI_UNIT($,.STERADIAN.)
                            SOLID_ANGLE_UNIT()
                        );
                        """,
                        "NAMED_UNIT",
                        "SI_UNIT",
                        "SOLID_ANGLE_UNIT")
                });

        var resolver =
            new StepUnitResolver();

        var result =
            resolver.Resolve(inventory);

        Assert.Equal(
            CadConnectorResultStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.NotNull(result.Value);

        Assert.Equal(
            CadLengthUnit.Unknown,
            result.Value.SourceLengthUnit);

        Assert.Equal(
            CadAngleUnit.Unknown,
            result.Value.SourceAngleUnit);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code ==
                "STEP_ANGLE_UNIT_NOT_RESOLVED");
    }

    [Fact]
    public void Resolve_WithConflictingLengthUnits_ReturnsUnknownLength()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateComplexEntity(
                        "#1",
                        """
                        #1 =(
                            LENGTH_UNIT()
                            NAMED_UNIT(*)
                            SI_UNIT(.MILLI.,.METRE.)
                        );
                        """,
                        "LENGTH_UNIT",
                        "NAMED_UNIT",
                        "SI_UNIT"),

                    CreateComplexEntity(
                        "#2",
                        """
                        #2 =(
                            LENGTH_UNIT()
                            NAMED_UNIT(*)
                            SI_UNIT($,.METRE.)
                        );
                        """,
                        "LENGTH_UNIT",
                        "NAMED_UNIT",
                        "SI_UNIT")
                });

        var resolver =
            new StepUnitResolver();

        var result =
            resolver.Resolve(inventory);

        Assert.Equal(
            CadConnectorResultStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.NotNull(result.Value);

        Assert.Equal(
            CadLengthUnit.Unknown,
            result.Value.SourceLengthUnit);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code ==
                "STEP_LENGTH_UNIT_NOT_RESOLVED");
    }

    [Fact]
    public void Resolve_WithNullInventory_ThrowsArgumentNullException()
    {
        var resolver =
            new StepUnitResolver();

        Assert.Throws<ArgumentNullException>(
            () =>
            {
                resolver.Resolve(null!);
            });
    }

    private static StepDataEntityRecord CreateComplexEntity(
        string sourceEntityId,
        string rawStatement,
        params string[] componentTypes)
    {
        return new StepDataEntityRecord(
            sourceEntityId: sourceEntityId,
            sourceEntityType: "COMPLEX_ENTITY",
            rawStatement: rawStatement,
            readStatus: CadReadStatus.Complete,
            componentTypes: componentTypes);
    }
}