namespace MPC.Connectors.Step.Models;

public sealed class StepEntityReferenceParameter
    : StepParameter
{
    public string EntityId { get; }

    public StepEntityReferenceParameter(
        string rawText,
        string entityId)
        : base(
            rawText)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentException(
                "STEP entity reference ID cannot be null, empty, or whitespace.",
                nameof(entityId));
        }

        var normalizedEntityId =
            entityId.Trim();

        if (!IsValidEntityId(
                normalizedEntityId))
        {
            throw new ArgumentException(
                "STEP entity reference ID must begin with '#' and contain only digits after it.",
                nameof(entityId));
        }

        EntityId =
            normalizedEntityId;
    }

    private static bool IsValidEntityId(
        string value)
    {
        if (value.Length < 2 ||
            value[0] != '#')
        {
            return false;
        }

        for (var index = 1;
             index < value.Length;
             index++)
        {
            if (!char.IsDigit(
                    value[index]))
            {
                return false;
            }
        }

        return true;
    }
}