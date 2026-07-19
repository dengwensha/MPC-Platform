using System.Numerics;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Contracts;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Validation;

public sealed class StepDataInventoryValidator
    : IStepDataInventoryValidator
{
    public IReadOnlyList<CadDiagnostic> Validate(
        IReadOnlyList<StepDataEntityRecord> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var duplicateGroups =
            entities
                .Select(
                    (entity, index) =>
                        CreateIndexedEntity(
                            entity,
                            index))
                .Where(
                    item =>
                        item.NumericSourceEntityId
                            is not null)
                .GroupBy(
                    item =>
                        item.NumericSourceEntityId!.Value)
                .Where(
                    group =>
                        group.Count() > 1)
                .OrderBy(
                    group =>
                        group.Key)
                .ToArray();

        if (duplicateGroups.Length == 0)
        {
            return Array.Empty<CadDiagnostic>();
        }

        var diagnostics =
            new List<CadDiagnostic>(
                duplicateGroups.Length);

        foreach (var duplicateGroup
                 in duplicateGroups)
        {
            var canonicalSourceEntityId =
                CreateCanonicalSourceEntityId(
                    duplicateGroup.Key);

            var duplicateCount =
                duplicateGroup.Count();

            var sourceEntityTypes =
                duplicateGroup
                    .OrderBy(
                        item =>
                            item.OriginalIndex)
                    .Select(
                        item =>
                            item.Entity
                                .SourceEntityType)
                    .Distinct(
                        StringComparer.Ordinal)
                    .ToArray();

            var sourceEntityType =
                sourceEntityTypes.Length == 1
                    ? sourceEntityTypes[0]
                    : "MULTIPLE";

            diagnostics.Add(
                new CadDiagnostic(
                    code:
                        "STEP_DUPLICATE_SOURCE_ENTITY_ID",
                    severity:
                        CadDiagnosticSeverity.Error,
                    message:
                        $"STEP source entity ID " +
                        $"'{canonicalSourceEntityId}' " +
                        $"was declared {duplicateCount} times. " +
                        "All declarations were preserved.",
                    sourceEntityId:
                        canonicalSourceEntityId,
                    sourceEntityType:
                        sourceEntityType));
        }

        return diagnostics.AsReadOnly();
    }

    private static IndexedEntity CreateIndexedEntity(
        StepDataEntityRecord entity,
        int index)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new IndexedEntity(
            Entity: entity,
            OriginalIndex: index,
            NumericSourceEntityId:
                TryParseNumericSourceEntityId(
                    entity.SourceEntityId));
    }

    private static BigInteger?
        TryParseNumericSourceEntityId(
            string? sourceEntityId)
    {
        if (string.IsNullOrWhiteSpace(
                sourceEntityId))
        {
            return null;
        }

        var normalized =
            sourceEntityId.Trim();

        if (!normalized.StartsWith(
                '#'))
        {
            return null;
        }

        var numericPart =
            normalized[1..];

        if (numericPart.Length == 0)
        {
            return null;
        }

        if (!numericPart.All(
                char.IsDigit))
        {
            return null;
        }

        if (!BigInteger.TryParse(
                numericPart,
                out var numericSourceEntityId))
        {
            return null;
        }

        if (numericSourceEntityId < 0)
        {
            return null;
        }

        return numericSourceEntityId;
    }

    private static string
        CreateCanonicalSourceEntityId(
            BigInteger numericSourceEntityId)
    {
        return
            $"#{numericSourceEntityId}";
    }

    private sealed record IndexedEntity(
        StepDataEntityRecord Entity,
        int OriginalIndex,
        BigInteger? NumericSourceEntityId);
}