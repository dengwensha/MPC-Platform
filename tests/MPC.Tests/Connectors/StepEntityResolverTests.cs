using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Resolution;

namespace MPC.Tests.Connectors;

public sealed class StepEntityResolverTests
{
    [Fact]
    public void Resolve_WithUniqueEntityId_ReturnsResolvedResult()
    {
        var entity =
            CreateEntity(
                "#25",
                "CARTESIAN_POINT");

        var resolver =
            CreateResolver(
                entity);

        var result =
            resolver.Resolve(
                "#25");

        Assert.Equal(
            StepEntityResolutionStatus.Resolved,
            result.Status);

        Assert.Equal(
            "#25",
            result.SourceEntityId);

        Assert.Same(
            entity,
            result.Entity);

        var candidate =
            Assert.Single(
                result.Candidates);

        Assert.Same(
            entity,
            candidate);
    }

    [Theory]
    [InlineData("#25")]
    [InlineData("#025")]
    [InlineData("#00025")]
    public void Resolve_WithEquivalentNumericEntityId_ReturnsSameEntity(
        string sourceEntityId)
    {
        var entity =
            CreateEntity(
                "#25",
                "CARTESIAN_POINT");

        var resolver =
            CreateResolver(
                entity);

        var result =
            resolver.Resolve(
                sourceEntityId);

        Assert.Equal(
            StepEntityResolutionStatus.Resolved,
            result.Status);

        Assert.Equal(
            "#25",
            result.SourceEntityId);

        Assert.Same(
            entity,
            result.Entity);
    }

    [Fact]
    public void Resolve_WithMissingEntityId_ReturnsMissingResult()
    {
        var resolver =
            CreateResolver(
                CreateEntity(
                    "#1",
                    "APPLICATION_CONTEXT"));

        var result =
            resolver.Resolve(
                "#25");

        Assert.Equal(
            StepEntityResolutionStatus.Missing,
            result.Status);

        Assert.Equal(
            "#25",
            result.SourceEntityId);

        Assert.Null(
            result.Entity);

        Assert.Empty(
            result.Candidates);
    }

    [Fact]
    public void Resolve_WithDuplicateEntityId_ReturnsAmbiguousResult()
    {
        var first =
            CreateEntity(
                "#25",
                "CARTESIAN_POINT");

        var second =
            CreateEntity(
                "#25",
                "DIRECTION");

        var resolver =
            CreateResolver(
                first,
                second);

        var result =
            resolver.Resolve(
                "#25");

        Assert.Equal(
            StepEntityResolutionStatus.Ambiguous,
            result.Status);

        Assert.Equal(
            "#25",
            result.SourceEntityId);

        Assert.Null(
            result.Entity);

        Assert.Equal(
            2,
            result.Candidates.Count);

        Assert.Same(
            first,
            result.Candidates[0]);

        Assert.Same(
            second,
            result.Candidates[1]);
    }

    [Fact]
    public void Resolve_WithEquivalentDuplicateEntityIds_ReturnsAmbiguousResult()
    {
        var first =
            CreateEntity(
                "#25",
                "CARTESIAN_POINT");

        var second =
            CreateEntity(
                "#025",
                "DIRECTION");

        var resolver =
            CreateResolver(
                first,
                second);

        var result =
            resolver.Resolve(
                "#00025");

        Assert.Equal(
            StepEntityResolutionStatus.Ambiguous,
            result.Status);

        Assert.Null(
            result.Entity);

        Assert.Equal(
            new[]
            {
                first,
                second
            },
            result.Candidates);
    }

    [Fact]
    public void ResolveReferences_PreservesReferenceOrder()
    {
        var source =
            CreateEntity(
                "#100",
                "AXIS2_PLACEMENT_3D",
                referencedEntityIds:
                    new[]
                    {
                        "#5",
                        "#7",
                        "#8"
                    });

        var entity5 =
            CreateEntity(
                "#5",
                "CARTESIAN_POINT");

        var entity7 =
            CreateEntity(
                "#7",
                "DIRECTION");

        var entity8 =
            CreateEntity(
                "#8",
                "DIRECTION");

        var resolver =
            CreateResolver(
                source,
                entity5,
                entity7,
                entity8);

        var results =
            resolver.ResolveReferences(
                source);

        Assert.Equal(
            3,
            results.Count);

        Assert.Same(
            entity5,
            results[0].Entity);

        Assert.Same(
            entity7,
            results[1].Entity);

        Assert.Same(
            entity8,
            results[2].Entity);
    }

    [Fact]
    public void ResolveReferences_PreservesRepeatedReferences()
    {
        var source =
            CreateEntity(
                "#100",
                "AXIS2_PLACEMENT_3D",
                referencedEntityIds:
                    new[]
                    {
                        "#5",
                        "#7",
                        "#5"
                    });

        var entity5 =
            CreateEntity(
                "#5",
                "CARTESIAN_POINT");

        var entity7 =
            CreateEntity(
                "#7",
                "DIRECTION");

        var resolver =
            CreateResolver(
                source,
                entity5,
                entity7);

        var results =
            resolver.ResolveReferences(
                source);

        Assert.Equal(
            3,
            results.Count);

        Assert.Same(
            entity5,
            results[0].Entity);

        Assert.Same(
            entity7,
            results[1].Entity);

        Assert.Same(
            entity5,
            results[2].Entity);
    }

    [Fact]
    public void ResolveReferences_WithMixedResults_PreservesEveryResult()
    {
        var source =
            CreateEntity(
                "#100",
                "AXIS2_PLACEMENT_3D",
                referencedEntityIds:
                    new[]
                    {
                        "#5",
                        "#7",
                        "#9"
                    });

        var resolved =
            CreateEntity(
                "#5",
                "CARTESIAN_POINT");

        var ambiguousFirst =
            CreateEntity(
                "#7",
                "DIRECTION");

        var ambiguousSecond =
            CreateEntity(
                "#007",
                "VECTOR");

        var resolver =
            CreateResolver(
                source,
                resolved,
                ambiguousFirst,
                ambiguousSecond);

        var results =
            resolver.ResolveReferences(
                source);

        Assert.Equal(
            new[]
            {
                StepEntityResolutionStatus.Resolved,
                StepEntityResolutionStatus.Ambiguous,
                StepEntityResolutionStatus.Missing
            },
            results
                .Select(
                    result =>
                        result.Status)
                .ToArray());
    }

    [Fact]
    public void Constructor_CopiesInventoryEntityCollection()
    {
        var first =
            CreateEntity(
                "#1",
                "APPLICATION_CONTEXT");

        var entities =
            new List<StepDataEntityRecord>
            {
                first
            };

        var inventory =
            new StepDataInventory(
                entities);

        var resolver =
            new StepEntityResolver(
                inventory);

        entities.Add(
            CreateEntity(
                "#2",
                "PRODUCT"));

        var result =
            resolver.Resolve(
                "#2");

        Assert.Equal(
            StepEntityResolutionStatus.Missing,
            result.Status);
    }

    [Fact]
    public void AmbiguousResult_CopiesCandidateCollection()
    {
        var first =
            CreateEntity(
                "#25",
                "CARTESIAN_POINT");

        var second =
            CreateEntity(
                "#25",
                "DIRECTION");

        var candidates =
            new List<StepDataEntityRecord>
            {
                first,
                second
            };

        var result =
            StepEntityResolutionResult.Ambiguous(
                "#25",
                candidates);

        candidates.Clear();

        Assert.Equal(
            2,
            result.Candidates.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("25")]
    [InlineData("#")]
    [InlineData("#ABC")]
    [InlineData("#25A")]
    [InlineData("##25")]
    public void Resolve_WithInvalidEntityId_ThrowsArgumentException(
        string? sourceEntityId)
    {
        var resolver =
            CreateResolver(
                CreateEntity(
                    "#1",
                    "APPLICATION_CONTEXT"));

        Assert.Throws<ArgumentException>(
            () =>
                resolver.Resolve(
                    sourceEntityId!));
    }

    [Fact]
    public void ResolveReferences_WithNullEntity_ThrowsArgumentNullException()
    {
        var resolver =
            CreateResolver(
                CreateEntity(
                    "#1",
                    "APPLICATION_CONTEXT"));

        Assert.Throws<ArgumentNullException>(
            () =>
                resolver.ResolveReferences(
                    null!));
    }

    [Fact]
    public void Resolve_WithSameQuery_ReturnsEquivalentDeterministicResults()
    {
        var entity =
            CreateEntity(
                "#25",
                "CARTESIAN_POINT");

        var resolver =
            CreateResolver(
                entity);

        var first =
            resolver.Resolve(
                "#025");

        var second =
            resolver.Resolve(
                "#00025");

        Assert.Equal(
            first.Status,
            second.Status);

        Assert.Equal(
            first.SourceEntityId,
            second.SourceEntityId);

        Assert.Same(
            first.Entity,
            second.Entity);

        Assert.Equal(
            first.Candidates,
            second.Candidates);
    }

    private static StepEntityResolver CreateResolver(
        params StepDataEntityRecord[] entities)
    {
        var inventory =
            new StepDataInventory(
                entities);

        return new StepEntityResolver(
            inventory);
    }

    private static StepDataEntityRecord CreateEntity(
        string? sourceEntityId,
        string sourceEntityType,
        IEnumerable<string>? referencedEntityIds = null)
    {
        return new StepDataEntityRecord(
            sourceEntityId,
            sourceEntityType,
            sourceEntityId is null
                ? $"{sourceEntityType}();"
                : $"{sourceEntityId} = {sourceEntityType}();",
            CadReadStatus.Complete,
            referencedEntityIds:
                referencedEntityIds);
    }
}