using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepAxis2Placement3DEntityHandler
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
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Handler for {SupportedEntityType} cannot process entity type '{record.SourceEntityType}'.");
        }

        if (parameters.Count != 4)
        {
            throw new FormatException(
                "AXIS2_PLACEMENT_3D must contain exactly four parameters: name, location, axis, and reference direction.");
        }

        if (parameters[0] is not
            StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first AXIS2_PLACEMENT_3D parameter must be a STEP string.");
        }

        if (parameters[1] is not
            StepEntityReferenceParameter locationReferenceParameter)
        {
            throw new FormatException(
                "The second AXIS2_PLACEMENT_3D parameter must be a STEP entity reference.");
        }

        var axisReferenceId =
            ReadOptionalReferenceId(
                parameters[2],
                "The third AXIS2_PLACEMENT_3D parameter must be a STEP entity reference or null.");

        var refDirectionReferenceId =
            ReadOptionalReferenceId(
                parameters[3],
                "The fourth AXIS2_PLACEMENT_3D parameter must be a STEP entity reference or null.");

        return new StepAxis2Placement3DEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            locationReferenceId:
                locationReferenceParameter.EntityId,
            axisReferenceId:
                axisReferenceId,
            refDirectionReferenceId:
                refDirectionReferenceId);
    }

    private static string?
        ReadOptionalReferenceId(
            StepParameter parameter,
            string errorMessage)
    {
        return parameter switch
        {
            StepEntityReferenceParameter
                referenceParameter =>
                referenceParameter.EntityId,

            StepNullParameter =>
                null,

            _ =>
                throw new FormatException(
                    errorMessage)
        };
    }
}