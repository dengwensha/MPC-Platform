using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepPlaneEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "PLANE";

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

        if (parameters.Count != 2)
        {
            throw new FormatException(
                "PLANE must contain exactly two parameters: name and placement.");
        }

        if (parameters[0] is not
            StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first PLANE parameter must be a STEP string.");
        }

        if (parameters[1] is not
            StepEntityReferenceParameter placementReferenceParameter)
        {
            throw new FormatException(
                "The second PLANE parameter must be a STEP entity reference.");
        }

        return new StepPlaneEntity(
            record.SourceEntityId,
            nameParameter.Value,
            placementReferenceParameter.EntityId);
    }
}