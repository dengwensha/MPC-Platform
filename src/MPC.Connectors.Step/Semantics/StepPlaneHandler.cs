using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepPlaneHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "PLANE";

    public StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters)
    {
        if (record is null)
        {
            throw new ArgumentNullException(
                nameof(record));
        }

        if (parameters is null)
        {
            throw new ArgumentNullException(
                nameof(parameters));
        }

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
                "PLANE must contain exactly two parameters.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "PLANE first parameter must be a STEP string.");

        var positionReference =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "PLANE second parameter must be an entity reference.");

        return new StepPlane(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            positionReferenceId:
                positionReference.EntityId);
    }
}