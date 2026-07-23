using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepClosedShellEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "CLOSED_SHELL";

    public StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(
            record);

        ArgumentNullException.ThrowIfNull(
            parameters);

        if (!string.Equals(
                record.SourceEntityType,
                SupportedEntityType,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"STEP semantic handler for '{SupportedEntityType}' cannot read entity type '{record.SourceEntityType}'.",
                nameof(record));
        }

        if (parameters.Count != 2)
        {
            throw new FormatException(
                "CLOSED_SHELL must contain exactly two parameters: name and face aggregate.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "CLOSED_SHELL first parameter must be a STEP string.");

        var faceAggregateParameter =
            parameters[1] as StepAggregateParameter
            ?? throw new FormatException(
                "CLOSED_SHELL second parameter must be a STEP aggregate.");

        if (faceAggregateParameter.Items.Count == 0)
        {
            throw new FormatException(
                "CLOSED_SHELL face aggregate must contain at least one entity reference.");
        }

        var faceReferenceIds =
            new List<string>(
                faceAggregateParameter.Items.Count);

        for (var index = 0;
             index < faceAggregateParameter.Items.Count;
             index++)
        {
            if (faceAggregateParameter.Items[index]
                is not StepEntityReferenceParameter referenceParameter)
            {
                throw new FormatException(
                    $"CLOSED_SHELL aggregate item at index {index} must be a STEP entity reference.");
            }

            faceReferenceIds.Add(
                referenceParameter.EntityId);
        }

        return new StepClosedShellEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            faceReferenceIds:
                faceReferenceIds);
    }
}