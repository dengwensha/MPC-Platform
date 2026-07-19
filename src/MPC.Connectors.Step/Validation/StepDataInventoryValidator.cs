using System.Numerics;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Contracts;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Validation;

public sealed class StepDataInventoryValidator
    : IStepDataInventoryValidator
{
    private const string DuplicateSourceEntityIdCode =
        "STEP_DUPLICATE_SOURCE_ENTITY_ID";

    private const string MissingEntityReferenceCode =
        "STEP_MISSING_ENTITY_REFERENCE";

    public IReadOnlyList<CadDiagnostic> Validate(
        IReadOnlyList<StepDataEntityRecord> entities)
    {
        ArgumentNullException.ThrowIfNull(
            entities);

        var indexedEntities =
            entities
                .Select(
                    (entity, index) =>
                        CreateIndexedEntity(
                            entity,
                            index))
                .ToArray();

        var diagnosticCandidates =
            new List<DiagnosticCandidate>();

        AddDuplicateSourceEntityIdDiagnostics(
            indexedEntities,
            diagnosticCandidates);

        AddMissingEntityReferenceDiagnostics(
            indexedEntities,
            diagnosticCandidates);

        if (diagnosticCandidates.Count == 0)
        {
            return Array.Empty<CadDiagnostic>();
        }

        return diagnosticCandidates
            .OrderBy(
                candidate =>
                    candidate.NumericSourceEntityId
                        is null)
            .ThenBy(
                candidate =>
                    candidate.NumericSourceEntityId)
            .ThenBy(
                candidate =>
                    candidate.NumericTargetEntityId
                        is not null)
            .ThenBy(
                candidate =>
                    candidate.NumericTargetEntityId)
            .ThenBy(
                candidate =>
                    candidate.Code,
                StringComparer.Ordinal)
            .ThenBy(
                candidate =>
                    candidate.OriginalIndex)
            .Select(
                candidate =>
                    candidate.Diagnostic)
            .ToList()
            .AsReadOnly();
    }

    private static void
        AddDuplicateSourceEntityIdDiagnostics(
            IReadOnlyList<IndexedEntity> indexedEntities,
            ICollection<DiagnosticCandidate>
                diagnosticCandidates)
    {
        var duplicateGroups =
            indexedEntities
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
                        group.Key);

        foreach (var duplicateGroup
                 in duplicateGroups)
        {
            var numericSourceEntityId =
                duplicateGroup.Key;

            var canonicalSourceEntityId =
                CreateCanonicalSourceEntityId(
                    numericSourceEntityId);

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

            var diagnostic =
                new CadDiagnostic(
                    code:
                        DuplicateSourceEntityIdCode,
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
                        sourceEntityType);

            diagnosticCandidates.Add(
                new DiagnosticCandidate(
                    Diagnostic:
                        diagnostic,
                    Code:
                        DuplicateSourceEntityIdCode,
                    NumericSourceEntityId:
                        numericSourceEntityId,
                    NumericTargetEntityId:
                        null,
                    OriginalIndex:
                        duplicateGroup
                            .Min(
                                item =>
                                    item.OriginalIndex)));
        }
    }

    private static void
        AddMissingEntityReferenceDiagnostics(
            IReadOnlyList<IndexedEntity> indexedEntities,
            ICollection<DiagnosticCandidate>
                diagnosticCandidates)
    {
        var declaredEntityIds =
            indexedEntities
                .Where(
                    entity =>
                        entity.NumericSourceEntityId
                            is not null)
                .Select(
                    entity =>
                        entity.NumericSourceEntityId!.Value)
                .ToHashSet();

        foreach (var indexedEntity
                 in indexedEntities)
        {
            var uniqueReferences =
                indexedEntity
                    .Entity
                    .ReferencedEntityIds
                    .Select(
                        referencedEntityId =>
                            new ParsedReference(
                                OriginalEntityId:
                                    referencedEntityId,
                                NumericEntityId:
                                    TryParseNumericEntityId(
                                        referencedEntityId)))
                    .Where(
                        reference =>
                            reference.NumericEntityId
                                is not null)
                    .GroupBy(
                        reference =>
                            reference.NumericEntityId!.Value)
                    .Select(
                        group =>
                            group.First())
                    .OrderBy(
                        reference =>
                            reference.NumericEntityId!.Value)
                    .ToArray();

            foreach (var reference
                     in uniqueReferences)
            {
                var numericTargetEntityId =
                    reference
                        .NumericEntityId!.Value;

                if (declaredEntityIds.Contains(
                        numericTargetEntityId))
                {
                    continue;
                }

                var canonicalTargetEntityId =
                    CreateCanonicalSourceEntityId(
                        numericTargetEntityId);

                var canonicalSourceEntityId =
                    indexedEntity
                        .NumericSourceEntityId
                        is not null
                        ? CreateCanonicalSourceEntityId(
                            indexedEntity
                                .NumericSourceEntityId
                                .Value)
                        : indexedEntity
                            .Entity
                            .SourceEntityId;

                var sourceEntityDescription =
                    canonicalSourceEntityId ??
                    "Unknown";

                var diagnostic =
                    new CadDiagnostic(
                        code:
                            MissingEntityReferenceCode,
                        severity:
                            CadDiagnosticSeverity.Error,
                        message:
                            $"STEP entity " +
                            $"'{sourceEntityDescription}' " +
                            $"references missing entity " +
                            $"'{canonicalTargetEntityId}'. " +
                            "The source entity and reference " +
                            "were preserved.",
                        sourceEntityId:
                            canonicalSourceEntityId,
                        sourceEntityType:
                            indexedEntity
                                .Entity
                                .SourceEntityType);

                diagnosticCandidates.Add(
                    new DiagnosticCandidate(
                        Diagnostic:
                            diagnostic,
                        Code:
                            MissingEntityReferenceCode,
                        NumericSourceEntityId:
                            indexedEntity
                                .NumericSourceEntityId,
                        NumericTargetEntityId:
                            numericTargetEntityId,
                        OriginalIndex:
                            indexedEntity
                                .OriginalIndex));
            }
        }
    }

    private static IndexedEntity CreateIndexedEntity(
        StepDataEntityRecord entity,
        int index)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        return new IndexedEntity(
            Entity:
                entity,
            OriginalIndex:
                index,
            NumericSourceEntityId:
                TryParseNumericEntityId(
                    entity.SourceEntityId));
    }

    private static BigInteger?
        TryParseNumericEntityId(
            string? entityId)
    {
        if (string.IsNullOrWhiteSpace(
                entityId))
        {
            return null;
        }

        var normalized =
            entityId.Trim();

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
                out var numericEntityId))
        {
            return null;
        }

        if (numericEntityId < 0)
        {
            return null;
        }

        return numericEntityId;
    }

    private static string
        CreateCanonicalSourceEntityId(
            BigInteger numericEntityId)
    {
        return
            $"#{numericEntityId}";
    }

    private sealed record IndexedEntity(
        StepDataEntityRecord Entity,
        int OriginalIndex,
        BigInteger? NumericSourceEntityId);

    private sealed record ParsedReference(
        string OriginalEntityId,
        BigInteger? NumericEntityId);

    private sealed record DiagnosticCandidate(
        CadDiagnostic Diagnostic,
        string Code,
        BigInteger? NumericSourceEntityId,
        BigInteger? NumericTargetEntityId,
        int OriginalIndex);
}