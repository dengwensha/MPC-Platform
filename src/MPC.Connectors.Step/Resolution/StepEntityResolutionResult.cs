using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Resolution;

public sealed class StepEntityResolutionResult
{
    private readonly IReadOnlyList<StepDataEntityRecord> _candidates;

    public StepEntityResolutionStatus Status { get; }

    public string SourceEntityId { get; }

    public StepDataEntityRecord? Entity { get; }

    public IReadOnlyList<StepDataEntityRecord> Candidates =>
        _candidates;

    private StepEntityResolutionResult(
        StepEntityResolutionStatus status,
        string sourceEntityId,
        StepDataEntityRecord? entity,
        IEnumerable<StepDataEntityRecord>? candidates)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityId))
        {
            throw new ArgumentException(
                "Source entity ID cannot be null, empty, or whitespace.",
                nameof(sourceEntityId));
        }

        var candidateList =
            candidates?.ToList() ??
            new List<StepDataEntityRecord>();

        if (candidateList.Any(
                candidate =>
                    candidate is null))
        {
            throw new ArgumentException(
                "Candidates cannot contain null entries.",
                nameof(candidates));
        }

        ValidateState(
            status,
            entity,
            candidateList);

        Status =
            status;

        SourceEntityId =
            sourceEntityId.Trim();

        Entity =
            entity;

        _candidates =
            candidateList.AsReadOnly();
    }

    public static StepEntityResolutionResult Resolved(
        string sourceEntityId,
        StepDataEntityRecord entity)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        return new StepEntityResolutionResult(
            StepEntityResolutionStatus.Resolved,
            sourceEntityId,
            entity,
            new[]
            {
                entity
            });
    }

    public static StepEntityResolutionResult Missing(
        string sourceEntityId)
    {
        return new StepEntityResolutionResult(
            StepEntityResolutionStatus.Missing,
            sourceEntityId,
            entity:
                null,
            candidates:
                null);
    }

    public static StepEntityResolutionResult Ambiguous(
        string sourceEntityId,
        IEnumerable<StepDataEntityRecord> candidates)
    {
        ArgumentNullException.ThrowIfNull(
            candidates);

        return new StepEntityResolutionResult(
            StepEntityResolutionStatus.Ambiguous,
            sourceEntityId,
            entity:
                null,
            candidates);
    }

    private static void ValidateState(
        StepEntityResolutionStatus status,
        StepDataEntityRecord? entity,
        IReadOnlyList<StepDataEntityRecord> candidates)
    {
        switch (status)
        {
            case StepEntityResolutionStatus.Resolved:
                if (entity is null)
                {
                    throw new ArgumentException(
                        "Resolved result must contain an entity.",
                        nameof(entity));
                }

                if (candidates.Count != 1)
                {
                    throw new ArgumentException(
                        "Resolved result must contain exactly one candidate.",
                        nameof(candidates));
                }

                if (!ReferenceEquals(
                        entity,
                        candidates[0]))
                {
                    throw new ArgumentException(
                        "Resolved entity must be the same instance as the single candidate.",
                        nameof(candidates));
                }

                break;

            case StepEntityResolutionStatus.Missing:
                if (entity is not null)
                {
                    throw new ArgumentException(
                        "Missing result cannot contain an entity.",
                        nameof(entity));
                }

                if (candidates.Count != 0)
                {
                    throw new ArgumentException(
                        "Missing result cannot contain candidates.",
                        nameof(candidates));
                }

                break;

            case StepEntityResolutionStatus.Ambiguous:
                if (entity is not null)
                {
                    throw new ArgumentException(
                        "Ambiguous result cannot contain a resolved entity.",
                        nameof(entity));
                }

                if (candidates.Count < 2)
                {
                    throw new ArgumentException(
                        "Ambiguous result must contain at least two candidates.",
                        nameof(candidates));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Unsupported entity resolution status.");
        }
    }
}