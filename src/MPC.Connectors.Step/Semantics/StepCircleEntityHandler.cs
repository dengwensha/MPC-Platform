using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepCircleEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "CIRCLE";

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

        if (parameters.Count != 3)
        {
            throw new FormatException(
                "CIRCLE must contain exactly three parameters: name, placement, and radius.");
        }

        if (parameters[0] is not
            StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first CIRCLE parameter must be a STEP string.");
        }

        if (parameters[1] is not
            StepEntityReferenceParameter placementReferenceParameter)
        {
            throw new FormatException(
                "The second CIRCLE parameter must be a STEP entity reference.");
        }

        double radius =
            ReadNumericParameter(
                parameters[2]);

        return new StepCircleEntity(
            record.SourceEntityId,
            nameParameter.Value,
            placementReferenceParameter.EntityId,
            radius);
    }

    private static double ReadNumericParameter(
        StepParameter parameter)
    {
        return parameter switch
        {
            StepRealParameter realParameter =>
                realParameter.Value,

            StepIntegerParameter integerParameter =>
                integerParameter.Value,

            _ =>
                throw new FormatException(
                    "The third CIRCLE parameter must be numeric.")
        };
    }
}