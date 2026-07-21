using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepConicalSurfaceEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "CONICAL_SURFACE";

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

        if (parameters.Count != 4)
        {
            throw new FormatException(
                "CONICAL_SURFACE must contain exactly four parameters: name, placement, radius, and semi-angle.");
        }

        if (parameters[0] is not StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first CONICAL_SURFACE parameter must be a STEP string.");
        }

        if (parameters[1] is not StepEntityReferenceParameter placementReferenceParameter)
        {
            throw new FormatException(
                "The second CONICAL_SURFACE parameter must be a STEP entity reference.");
        }

        double radius =
            ReadNumericParameter(
                parameters[2],
                "The third CONICAL_SURFACE parameter, radius, must be numeric.");

        double semiAngle =
            ReadNumericParameter(
                parameters[3],
                "The fourth CONICAL_SURFACE parameter, semi-angle, must be numeric.");

        return new StepConicalSurfaceEntity(
            record.SourceEntityId,
            nameParameter.Value,
            placementReferenceParameter.EntityId,
            radius,
            semiAngle);
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