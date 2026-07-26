namespace MPC.Connectors.Step.Semantics;

public sealed class StepSemanticEntityResolver
{
    private readonly Dictionary<string, StepSemanticEntity> _entitiesById;

    public StepSemanticEntityResolver(
        IEnumerable<StepSemanticEntity> semanticEntities)
    {
        ArgumentNullException.ThrowIfNull(
            semanticEntities);

        _entitiesById =
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

            if (!_entitiesById.TryAdd(
                    entity.SourceEntityId,
                    entity))
            {
                throw new InvalidOperationException(
                    $"Duplicate STEP semantic entity id '{entity.SourceEntityId}'.");
            }
        }
    }

    public StepSemanticEntity Resolve(
        string referenceId)
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

        return entity;
    }

    public TEntity Resolve<TEntity>(
        string referenceId)
        where TEntity : StepSemanticEntity
    {
        var entity =
            Resolve(
                referenceId);

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidOperationException(
                $"STEP semantic entity '{referenceId}' is '{entity.SourceEntityType}', but '{typeof(TEntity).Name}' was required.");
        }

        return typedEntity;
    }
}