using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;

namespace MPC.Tests.Connectors;

public sealed class StepDataInventoryTests
{
    [Fact]
    public void Constructor_WithNullEntities_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new StepDataInventory(
                    null!));
    }

    [Fact]
    public void Constructor_WithNullEntityEntry_Throws()
    {
        var entities =
            new StepDataEntityRecord[]
            {
                CreateEntity(
                    "#1",
                    "PRODUCT"),

                null!
            };

        Assert.Throws<ArgumentException>(
            () =>
                new StepDataInventory(
                    entities));
    }

    [Fact]
    public void Constructor_WithEmptyEntities_CreatesEmptyInventory()
    {
        var inventory =
            new StepDataInventory(
                Array.Empty<StepDataEntityRecord>());

        Assert.Empty(
            inventory.Entities);

        Assert.Empty(
            inventory.TypeCounts);

        Assert.Equal(
            0,
            inventory.TotalEntityCount);
    }

    [Fact]
    public void Constructor_PreservesEntityOrder()
    {
        var first =
            CreateEntity(
                "#1",
                "PRODUCT");

        var second =
            CreateEntity(
                "#2",
                "CARTESIAN_POINT");

        var third =
            CreateEntity(
                "#3",
                "DIRECTION");

        var inventory =
            new StepDataInventory(
                new[]
                {
                    first,
                    second,
                    third
                });

        Assert.Equal(
            new[]
            {
                first,
                second,
                third
            },
            inventory.Entities);
    }

    [Fact]
    public void Constructor_CopiesCallerOwnedEntityCollection()
    {
        var first =
            CreateEntity(
                "#1",
                "PRODUCT");

        var second =
            CreateEntity(
                "#2",
                "DIRECTION");

        var entities =
            new List<StepDataEntityRecord>
            {
                first
            };

        var inventory =
            new StepDataInventory(
                entities);

        entities[0] =
            second;

        entities.Add(
            second);

        Assert.Same(
            first,
            Assert.Single(
                inventory.Entities));

        Assert.Equal(
            1,
            inventory.TotalEntityCount);
    }

    [Fact]
    public void Constructor_AggregatesEntityTypeCounts()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateEntity(
                        "#1",
                        "PRODUCT"),

                    CreateEntity(
                        "#2",
                        "PRODUCT"),

                    CreateEntity(
                        "#3",
                        "DIRECTION")
                });

        Assert.Equal(
            3,
            inventory.TotalEntityCount);

        Assert.Equal(
            2,
            inventory.GetCount(
                "PRODUCT"));

        Assert.Equal(
            1,
            inventory.GetCount(
                "DIRECTION"));

        Assert.Equal(
            2,
            inventory.TypeCounts.Count);
    }

    [Fact]
    public void GetCount_IsCaseInsensitiveAndTrimsInput()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateEntity(
                        "#1",
                        "CARTESIAN_POINT"),

                    CreateEntity(
                        "#2",
                        "CARTESIAN_POINT")
                });

        Assert.Equal(
            2,
            inventory.GetCount(
                "  cartesian_point  "));
    }

    [Fact]
    public void GetCount_WithUnknownType_ReturnsZero()
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateEntity(
                        "#1",
                        "PRODUCT")
                });

        Assert.Equal(
            0,
            inventory.GetCount(
                "DIRECTION"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetCount_WithInvalidType_Throws(
        string? sourceEntityType)
    {
        var inventory =
            new StepDataInventory(
                new[]
                {
                    CreateEntity(
                        "#1",
                        "PRODUCT")
                });

        Assert.Throws<ArgumentException>(
            () =>
                inventory.GetCount(
                    sourceEntityType!));
    }

    private static StepDataEntityRecord
        CreateEntity(
            string sourceEntityId,
            string sourceEntityType)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                sourceEntityId,
            sourceEntityType:
                sourceEntityType,
            rawStatement:
                $"{sourceEntityId}=" +
                $"{sourceEntityType}();",
            readStatus:
                CadReadStatus.Complete);
    }
}