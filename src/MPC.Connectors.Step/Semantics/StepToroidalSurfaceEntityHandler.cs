using System.Globalization;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepToroidalSurfaceEntityHandler
    : IStepSemanticEntityHandler
{
    private readonly SwStepSignedToroidalCompatibilityPolicy?
        _compatibilityPolicy;

    public string SupportedEntityType =>
        "TOROIDAL_SURFACE";

    public StepToroidalSurfaceEntityHandler()
    {
    }

    public StepToroidalSurfaceEntityHandler(
        SwStepSignedToroidalCompatibilityPolicy
            compatibilityPolicy)
    {
        _compatibilityPolicy =
            compatibilityPolicy ??
            throw new ArgumentNullException(
                nameof(compatibilityPolicy));
    }

    public StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(parameters);

        if (!string.Equals(
                record.SourceEntityType,
                SupportedEntityType,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Handler for {SupportedEntityType} cannot process entity type '{record.SourceEntityType}'.");
        }

        if (parameters.Count != 4)
        {
            throw new FormatException(
                "TOROIDAL_SURFACE must contain exactly four parameters: name, placement, major radius, and minor radius.");
        }

        if (parameters[0] is not StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first TOROIDAL_SURFACE parameter must be a STEP string.");
        }

        if (parameters[1] is not StepEntityReferenceParameter
            placementReferenceParameter)
        {
            throw new FormatException(
                "The second TOROIDAL_SURFACE parameter must be a STEP entity reference.");
        }

        double majorRadius =
            ReadNumericParameter(
                parameters[2],
                "The third TOROIDAL_SURFACE parameter, major radius, must be numeric.");

        double minorRadius =
            ReadNumericParameter(
                parameters[3],
                "The fourth TOROIDAL_SURFACE parameter, minor radius, must be numeric.");

        if (
            _compatibilityPolicy is not null &&
            _compatibilityPolicy.TryInterpret(
                majorRadius,
                minorRadius,
                out double standardMajorRadius))
        {
            return new StepDegenerateToroidalSurfaceEntity(
                record.SourceEntityId,
                SupportedEntityType,
                nameParameter.Value,
                placementReferenceParameter.EntityId,
                standardMajorRadius,
                minorRadius,
                selectOuter: false);
        }

        try
        {
            return new StepToroidalSurfaceEntity(
                record.SourceEntityId,
                nameParameter.Value,
                placementReferenceParameter.EntityId,
                majorRadius,
                minorRadius);
        }
        catch (ArgumentOutOfRangeException exception)
            when (string.Equals(
                exception.ParamName,
                nameof(majorRadius),
                StringComparison.Ordinal))
        {
            throw new StepSemanticValueException(
                record.SourceEntityId,
                SupportedEntityType,
                nameof(majorRadius),
                majorRadius.ToString(
                    "R",
                    CultureInfo.InvariantCulture),
                exception);
        }
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
