using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepAdvancedFaceEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "ADVANCED_FACE";

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

        if (parameters.Count != 4)
        {
            throw new FormatException(
                "ADVANCED_FACE must contain exactly four parameters: name, face bound aggregate, face geometry reference, and same sense.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "ADVANCED_FACE first parameter must be a STEP string.");

        var boundAggregateParameter =
            parameters[1] as StepAggregateParameter
            ?? throw new FormatException(
                "ADVANCED_FACE second parameter must be a STEP aggregate.");

        if (boundAggregateParameter.Items.Count == 0)
        {
            throw new FormatException(
                "ADVANCED_FACE face bound aggregate must contain at least one entity reference.");
        }

        var boundReferenceIds =
            new List<string>(
                boundAggregateParameter.Items.Count);

        for (var index = 0;
             index < boundAggregateParameter.Items.Count;
             index++)
        {
            if (boundAggregateParameter.Items[index]
                is not StepEntityReferenceParameter referenceParameter)
            {
                throw new FormatException(
                    $"ADVANCED_FACE aggregate item at index {index} must be a STEP entity reference.");
            }

            boundReferenceIds.Add(
                referenceParameter.EntityId);
        }

        var surfaceReferenceParameter =
            parameters[2] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "ADVANCED_FACE third parameter must be a STEP entity reference.");

        var sameSenseParameter =
            parameters[3] as StepEnumerationParameter
            ?? throw new FormatException(
                "ADVANCED_FACE fourth parameter must be a STEP enumeration.");

        var sameSense =
            StepBooleanEnumerationParser.Parse(
                sameSenseParameter,
                SupportedEntityType,
                "same sense");

        return new StepAdvancedFaceEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            boundReferenceIds:
                boundReferenceIds,
            surfaceReferenceId:
                surfaceReferenceParameter.EntityId,
            sameSense:
                sameSense);
    }
}