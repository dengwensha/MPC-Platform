using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Resolution;

public sealed class StepEntityResolver
{
    private readonly IReadOnlyDictionary<
        string,
        IReadOnlyList<StepDataEntityRecord>> _entitiesByNormalizedId;

    public StepEntityResolver(
        StepDataInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(
            inventory);

        var index =
            new Dictionary<
                string,
                List<StepDataEntityRecord>>(
                StringComparer.Ordinal);

        foreach (var entity in inventory.Entities)
        {
            if (entity.SourceEntityId is null)
            {
                continue;
            }

            var normalizedId =
                NormalizeEntityId(
                    entity.SourceEntityId);

            if (!index.TryGetValue(
                    normalizedId,
                    out var candidates))
            {
                candidates =
                    new List<StepDataEntityRecord>();

                index.Add(
                    normalizedId,
                    candidates);
            }

            candidates.Add(
                entity);
        }

        _entitiesByNormalizedId =
            index.ToDictionary(
                pair =>
                    pair.Key,
                pair =>
                    (IReadOnlyList<StepDataEntityRecord>)
                    pair.Value
                        .ToList()
                        .AsReadOnly(),
                StringComparer.Ordinal);
    }

    public StepEntityResolutionResult Resolve(
        string sourceEntityId)
    {
        var normalizedId =
            NormalizeEntityId(
                sourceEntityId);

        if (!_entitiesByNormalizedId.TryGetValue(
                normalizedId,
                out var candidates))
        {
            return StepEntityResolutionResult.Missing(
                normalizedId);
        }

        if (candidates.Count == 1)
        {
            return StepEntityResolutionResult.Resolved(
                normalizedId,
                candidates[0]);
        }

        return StepEntityResolutionResult.Ambiguous(
            normalizedId,
            candidates);
    }

    public IReadOnlyList<StepEntityResolutionResult> ResolveReferences(
        StepDataEntityRecord entity)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        var results =
            entity.ReferencedEntityIds
                .Select(
                    Resolve)
                .ToList();

        return results.AsReadOnly();
    }

    private static string NormalizeEntityId(
        string sourceEntityId)
    {
        if (string.IsNullOrWhiteSpace(
                sourceEntityId))
        {
            throw new ArgumentException(
                "Source entity ID cannot be null, empty, or whitespace.",
                nameof(sourceEntityId));
        }

        var trimmed =
            sourceEntityId.Trim();

        if (trimmed.Length < 2 ||
            trimmed[0] != '#')
        {
            throw new ArgumentException(
                "Source entity ID must begin with '#'.",
                nameof(sourceEntityId));
        }

        var numericPart =
            trimmed[1..];

        if (numericPart.Length == 0 ||
            numericPart.Any(
                character =>
                    !char.IsDigit(
                        character)))
        {
            throw new ArgumentException(
                "Source entity ID must contain only decimal digits after '#'.",
                nameof(sourceEntityId));
        }

        var firstNonZeroIndex =
            0;

        while (firstNonZeroIndex <
               numericPart.Length - 1 &&
               numericPart[firstNonZeroIndex] == '0')
        {
            firstNonZeroIndex++;
        }

        return
            $"#{numericPart[firstNonZeroIndex..]}";
    }
}