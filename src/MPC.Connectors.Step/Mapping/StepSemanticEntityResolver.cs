using MPC.Connectors.Step.Semantics;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepSemanticEntityResolver
{
    private readonly IReadOnlyDictionary<string, StepSemanticEntity> _entitiesById;

    public StepSemanticEntityResolver(
        IEnumerable<StepSemanticEntity> semanticEntities)
    {
        if (semanticEntities is null)
        {
            throw new ArgumentNullException(
                nameof(semanticEntities));
        }

        var entitiesById =
            new Dictionary<string, StepSemanticEntity>(
                StringComparer.Ordinal);

        foreach (var entity in semanticEntities)
        {
            if (entity is null)
            {
                throw new ArgumentException(
                    "STEP semantic entity collection cannot contain null.",
                    nameof(semanticEntities));
            }

            if (string.IsNullOrWhiteSpace(
                    entity.SourceEntityId))
            {
                continue;
            }

            if (!entitiesById.TryAdd(
                    entity.SourceEntityId,
                    entity))
            {
                throw new InvalidOperationException(
                    $"Duplicate STEP semantic entity id '{entity.SourceEntityId}'.");
            }
        }

        _entitiesById =
            entitiesById;
    }

    public TEntity Resolve<TEntity>(
        string referenceId)
        where TEntity : StepSemanticEntity
    {
        if (string.IsNullOrWhiteSpace(
                referenceId))
        {
            throw new InvalidOperationException(
                "STEP entity reference cannot be empty.");
        }

        if (!_entitiesById.TryGetValue(
                referenceId,
                out var entity))
        {
            throw new InvalidOperationException(
                $"STEP semantic entity '{referenceId}' could not be resolved.");
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidOperationException(
                $"STEP semantic entity '{referenceId}' is '{entity.SourceEntityType}', but '{typeof(TEntity).Name}' was required.");
        }

        return typedEntity;
    }
}