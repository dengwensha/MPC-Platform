using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepCylindricalSurfaceHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "CYLINDRICAL_SURFACE";

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

        if (parameters.Count != 3)
        {
            throw new FormatException(
                "CYLINDRICAL_SURFACE must contain exactly three parameters.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "CYLINDRICAL_SURFACE first parameter must be a STEP string.");

        var positionReference =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "CYLINDRICAL_SURFACE second parameter must be an entity reference.");

        var radius =
            parameters[2] switch
            {
                StepRealParameter realParameter =>
                    realParameter.Value,

                StepIntegerParameter integerParameter =>
                    integerParameter.Value,

                _ =>
                    throw new FormatException(
                        "CYLINDRICAL_SURFACE third parameter must be an integer or real number.")
            };

        return new StepCylindricalSurface(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            positionReferenceId:
                positionReference.EntityId,
            radius:
                radius);
    }
}