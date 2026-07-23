using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

internal static class StepBooleanEnumerationParser
{
    public static bool Parse(
        StepEnumerationParameter parameter,
        string entityType,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(
            parameter);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            entityType);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            parameterName);

        return parameter.Name switch
        {
            "T" =>
                true,

            "F" =>
                false,

            _ =>
                throw new FormatException(
                    $"{entityType} {parameterName} must be .T. or .F., but '{parameter.RawText}' was provided.")
        };
    }
}