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
        ArgumentNullException.ThrowIfNull(
            entities);

        var entityList =
            entities is
                ICollection<StepDataEntityRecord> collection
                ? new List<StepDataEntityRecord>(
                    collection.Count)
                : new List<StepDataEntityRecord>();

        var typeCounts =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var entity in entities)
        {
            if (entity is null)
            {
                throw new ArgumentException(
                    "STEP DATA inventory cannot contain null entities.",
                    nameof(entities));
            }

            entityList.Add(
                entity);

            if (typeCounts.TryGetValue(
                    entity.SourceEntityType,
                    out var currentCount))
            {
                typeCounts[
                    entity.SourceEntityType
                ] =
                    currentCount + 1;
            }
            else
            {
                typeCounts.Add(
                    entity.SourceEntityType,
                    1);
            }
        }

        _entities =
            entityList.AsReadOnly();

        _typeCounts =
            typeCounts;
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