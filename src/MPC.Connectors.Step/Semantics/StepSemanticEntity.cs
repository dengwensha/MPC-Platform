namespace MPC.Connectors.Step.Semantics;

public abstract class StepSemanticEntity
{
    public string? SourceEntityId { get; }

    public string SourceEntityType { get; }

    protected StepSemanticEntity(
        string? sourceEntityId,
        string sourceEntityType)
    {
        if (string.IsNullOrWhiteSpace(
                sourceEntityType))
        {
            throw new ArgumentException(
                "STEP semantic entity type cannot be null, empty, or whitespace.",
                nameof(sourceEntityType));
        }

        SourceEntityId =
            string.IsNullOrWhiteSpace(
                sourceEntityId)
                ? null
                : sourceEntityId.Trim();

        SourceEntityType =
            sourceEntityType
                .Trim()
                .ToUpperInvariant();
    }
}