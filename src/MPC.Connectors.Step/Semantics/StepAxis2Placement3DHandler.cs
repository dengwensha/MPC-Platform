using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepAxis2Placement3DHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "AXIS2_PLACEMENT_3D";

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
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"STEP semantic handler for '{SupportedEntityType}' cannot read entity type '{record.SourceEntityType}'.",
                nameof(record));
        }

        if (parameters.Count != 4)
        {
            throw new FormatException(
                "AXIS2_PLACEMENT_3D must contain exactly four parameters.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "AXIS2_PLACEMENT_3D first parameter must be a STEP string.");

        var locationReference =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "AXIS2_PLACEMENT_3D second parameter must be an entity reference.");

        var axisReference =
            parameters[2] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "AXIS2_PLACEMENT_3D third parameter must be an entity reference.");

        var referenceDirection =
            parameters[3] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "AXIS2_PLACEMENT_3D fourth parameter must be an entity reference.");

        return new StepAxis2Placement3D(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            locationReferenceId:
                locationReference.EntityId,
            axisReferenceId:
                axisReference.EntityId,
            referenceDirectionId:
                referenceDirection.EntityId);
    }
}