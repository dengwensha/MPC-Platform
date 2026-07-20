using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepVectorEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "VECTOR";

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
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Handler for {SupportedEntityType} cannot process entity type '{record.SourceEntityType}'.");
        }

        if (parameters.Count != 3)
        {
            throw new FormatException(
                "VECTOR must contain exactly three parameters: name, orientation, and magnitude.");
        }

        if (parameters[0] is not StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first VECTOR parameter must be a STEP string.");
        }

        if (parameters[1] is not StepEntityReferenceParameter orientationReferenceParameter)
        {
            throw new FormatException(
                "The second VECTOR parameter must be a STEP entity reference.");
        }

        var magnitude =
            ReadNumericParameter(
                parameters[2]);

        return new StepVectorEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            orientationReferenceId:
                orientationReferenceParameter.EntityId,
            magnitude:
                magnitude);
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
                    "The third VECTOR parameter must be numeric.")
        };
    }
}