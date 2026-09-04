using System.Globalization;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepDegenerateToroidalSurfaceEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "DEGENERATE_TOROIDAL_SURFACE";

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
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Handler for {SupportedEntityType} cannot process entity type '{record.SourceEntityType}'.");
        }

        if (parameters.Count != 5)
        {
            throw new FormatException(
                "DEGENERATE_TOROIDAL_SURFACE must contain exactly five parameters: name, placement, major radius, minor radius, and select outer.");
        }

        if (parameters[0] is not StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first DEGENERATE_TOROIDAL_SURFACE parameter must be a STEP string.");
        }

        if (parameters[1] is not StepEntityReferenceParameter
            placementReferenceParameter)
        {
            throw new FormatException(
                "The second DEGENERATE_TOROIDAL_SURFACE parameter must be a STEP entity reference.");
        }

        double majorRadius =
            ReadNumericParameter(
                parameters[2],
                "The third DEGENERATE_TOROIDAL_SURFACE parameter, major radius, must be numeric.");

        double minorRadius =
            ReadNumericParameter(
                parameters[3],
                "The fourth DEGENERATE_TOROIDAL_SURFACE parameter, minor radius, must be numeric.");

        if (parameters[4] is not StepEnumerationParameter
            selectOuterParameter)
        {
            throw new FormatException(
                "The fifth DEGENERATE_TOROIDAL_SURFACE parameter, select outer, must be a STEP enumeration.");
        }

        bool selectOuter =
            StepBooleanEnumerationParser.Parse(
                selectOuterParameter,
                SupportedEntityType,
                "selectOuter");

        try
        {
            return new StepDegenerateToroidalSurfaceEntity(
                record.SourceEntityId,
                nameParameter.Value,
                placementReferenceParameter.EntityId,
                majorRadius,
                minorRadius,
                selectOuter);
        }
        catch (ArgumentOutOfRangeException exception)
            when (IsRadiusParameter(
                exception.ParamName))
        {
            string rejectedValue =
                string.Equals(
                    exception.ParamName,
                    nameof(majorRadius),
                    StringComparison.Ordinal)
                    ? majorRadius.ToString(
                        "R",
                        CultureInfo.InvariantCulture)
                    : minorRadius.ToString(
                        "R",
                        CultureInfo.InvariantCulture);

            throw new StepSemanticValueException(
                record.SourceEntityId,
                SupportedEntityType,
                exception.ParamName!,
                rejectedValue,
                exception);
        }
    }

    private static bool IsRadiusParameter(
        string? parameterName)
    {
        return
            string.Equals(
                parameterName,
                "majorRadius",
                StringComparison.Ordinal) ||
            string.Equals(
                parameterName,
                "minorRadius",
                StringComparison.Ordinal);
    }

    private static double ReadNumericParameter(
        StepParameter parameter,
        string errorMessage)
    {
        return parameter switch
        {
            StepRealParameter realParameter =>
                realParameter.Value,

            StepIntegerParameter integerParameter =>
                integerParameter.Value,

            _ =>
                throw new FormatException(
                    errorMessage)
        };
    }
}
