using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepEdgeCurveEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "EDGE_CURVE";

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
                "EDGE_CURVE must contain exactly five parameters: name, edge start, edge end, edge geometry, and same sense.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "EDGE_CURVE first parameter must be a STEP string.");

        var edgeStartParameter =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "EDGE_CURVE second parameter must be a STEP entity reference.");

        var edgeEndParameter =
            parameters[2] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "EDGE_CURVE third parameter must be a STEP entity reference.");

        var edgeGeometryParameter =
            parameters[3] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "EDGE_CURVE fourth parameter must be a STEP entity reference.");

        var sameSenseParameter =
            parameters[4] as StepEnumerationParameter
            ?? throw new FormatException(
                "EDGE_CURVE fifth parameter must be a STEP enumeration.");

        var sameSense =
            ParseBooleanEnumeration(
                sameSenseParameter,
                "EDGE_CURVE same-sense parameter");

        return new StepEdgeCurveEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            edgeStartReferenceId:
                edgeStartParameter.EntityId,
            edgeEndReferenceId:
                edgeEndParameter.EntityId,
            edgeGeometryReferenceId:
                edgeGeometryParameter.EntityId,
            sameSense:
                sameSense);
    }

    private static bool ParseBooleanEnumeration(
        StepEnumerationParameter parameter,
        string parameterDescription)
    {
        return parameter.Name switch
        {
            "T" =>
                true,

            "F" =>
                false,

            _ =>
                throw new FormatException(
                    $"{parameterDescription} must be .T. or .F., but '{parameter.RawText}' was provided.")
        };
    }
}