using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepLineEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "LINE";

    public StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (parameters is null)
        {
            throw new ArgumentNullException(nameof(parameters));
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
                "LINE must contain exactly three parameters.");
        }

        if (parameters[0] is not StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first LINE parameter must be a STEP string.");
        }

        if (parameters[1] is not StepEntityReferenceParameter pointReference)
        {
            throw new FormatException(
                "The second LINE parameter must be a STEP entity reference.");
        }

        if (parameters[2] is not StepEntityReferenceParameter vectorReference)
        {
            throw new FormatException(
                "The third LINE parameter must be a STEP entity reference.");
        }

        return new StepLineEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            pointReferenceId:
                pointReference.EntityId,
            vectorReferenceId:
                vectorReference.EntityId);
    }
}