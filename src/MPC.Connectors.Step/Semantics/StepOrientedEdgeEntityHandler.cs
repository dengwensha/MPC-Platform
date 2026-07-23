using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepOrientedEdgeEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "ORIENTED_EDGE";

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

        if (parameters.Count != 5)
        {
            throw new FormatException(
                "ORIENTED_EDGE must contain exactly five parameters: name, derived edge start, derived edge end, edge element, and orientation.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "ORIENTED_EDGE first parameter must be a STEP string.");

        _ =
            parameters[1] as StepOmittedParameter
            ?? throw new FormatException(
                "ORIENTED_EDGE second parameter must be the derived STEP parameter '*'.");

        _ =
            parameters[2] as StepOmittedParameter
            ?? throw new FormatException(
                "ORIENTED_EDGE third parameter must be the derived STEP parameter '*'.");

        var edgeElementParameter =
            parameters[3] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "ORIENTED_EDGE fourth parameter must be a STEP entity reference.");

        var orientationParameter =
            parameters[4] as StepEnumerationParameter
            ?? throw new FormatException(
                "ORIENTED_EDGE fifth parameter must be a STEP enumeration.");

        var orientation =
            ParseBooleanEnumeration(
                orientationParameter);

        return new StepOrientedEdgeEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            edgeElementReferenceId:
                edgeElementParameter.EntityId,
            orientation:
                orientation);
    }

    private static bool ParseBooleanEnumeration(
        StepEnumerationParameter parameter)
    {
        return parameter.Name switch
        {
            "T" =>
                true,

            "F" =>
                false,

            _ =>
                throw new FormatException(
                    $"ORIENTED_EDGE orientation must be .T. or .F., but '{parameter.RawText}' was provided.")
        };
    }
}