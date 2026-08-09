namespace MPC.Connectors.Step.Models;

public sealed class StepDataInventory
{
    private readonly IReadOnlyList<StepDataEntityRecord> _entities;
    private readonly IReadOnlyDictionary<string, int> _typeCounts;

    public IReadOnlyList<StepDataEntityRecord> Entities =>
        _entities;

    public IReadOnlyDictionary<string, int> TypeCounts =>
        _typeCounts;

    public int TotalEntityCount =>
        _entities.Count;

    public StepDataInventory(
        IEnumerable<StepDataEntityRecord> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();

        if (entityList.Any(entity => entity is null))
        {
            throw new ArgumentException(
                "STEP DATA inventory cannot contain null entities.",
                nameof(entities));
        }

        _entities = entityList.AsReadOnly();

        _typeCounts = entityList
            .GroupBy(
                entity => entity.SourceEntityType,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.OrdinalIgnoreCase);
    }

    public int GetCount(
        string sourceEntityType)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityType))
        {
            throw new ArgumentException(
                "Source entity type cannot be null, empty, or whitespace.",
                nameof(sourceEntityType));
        }

        return _typeCounts.TryGetValue(
            sourceEntityType.Trim(),
            out var count)
                ? count
                : 0;
    }
}