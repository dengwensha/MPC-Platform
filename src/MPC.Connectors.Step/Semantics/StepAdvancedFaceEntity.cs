namespace MPC.Connectors.Step.Semantics;

public sealed class StepAdvancedFaceEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public IReadOnlyList<string> BoundReferenceIds { get; }

    public string SurfaceReferenceId { get; }

    public bool SameSense { get; }

    public StepAdvancedFaceEntity(
        string? sourceEntityId,
        string name,
        IEnumerable<string> boundReferenceIds,
        string surfaceReferenceId,
        bool sameSense)
        : base(
            sourceEntityId,
            "ADVANCED_FACE")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        ArgumentNullException.ThrowIfNull(
            boundReferenceIds);

        if (string.IsNullOrWhiteSpace(
                surfaceReferenceId))
        {
            throw new ArgumentException(
                "Surface reference ID cannot be null, empty, or whitespace.",
                nameof(surfaceReferenceId));
        }

        var boundReferenceIdList =
            boundReferenceIds.ToList();

        if (boundReferenceIdList.Count == 0)
        {
            throw new ArgumentException(
                "ADVANCED_FACE must contain at least one bound reference.",
                nameof(boundReferenceIds));
        }

        for (var index = 0;
             index < boundReferenceIdList.Count;
             index++)
        {
            if (string.IsNullOrWhiteSpace(
                    boundReferenceIdList[index]))
            {
                throw new ArgumentException(
                    $"Bound reference ID at index {index} cannot be null, empty, or whitespace.",
                    nameof(boundReferenceIds));
            }
        }

        Name =
            name;

        BoundReferenceIds =
            boundReferenceIdList.AsReadOnly();

        SurfaceReferenceId =
            surfaceReferenceId;

        SameSense =
            sameSense;
    }
}