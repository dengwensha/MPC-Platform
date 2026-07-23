using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepFaceBoundEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "FACE_BOUND";

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

        if (parameters.Count != 3)
        {
            throw new FormatException(
                "FACE_BOUND must contain exactly three parameters: name, loop reference, and orientation.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "FACE_BOUND first parameter must be a STEP string.");

        var loopReferenceParameter =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "FACE_BOUND second parameter must be a STEP entity reference.");

        var orientationParameter =
            parameters[2] as StepEnumerationParameter
            ?? throw new FormatException(
                "FACE_BOUND third parameter must be a STEP enumeration.");

        var orientation =
            StepBooleanEnumerationParser.Parse(
                orientationParameter,
                SupportedEntityType,
                "orientation");

        return new StepFaceBoundEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            loopReferenceId:
                loopReferenceParameter.EntityId,
            orientation:
                orientation);
    }
}