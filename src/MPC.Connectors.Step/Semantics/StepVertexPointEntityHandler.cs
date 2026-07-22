using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepVertexPointEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "VERTEX_POINT";

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
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"STEP semantic handler for '{SupportedEntityType}' cannot read entity type '{record.SourceEntityType}'.",
                nameof(record));
        }

        if (parameters.Count != 2)
        {
            throw new FormatException(
                "VERTEX_POINT must contain exactly two parameters: name and vertex geometry reference.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "VERTEX_POINT first parameter must be a STEP string.");

        var vertexGeometryReferenceParameter =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "VERTEX_POINT second parameter must be a STEP entity reference.");

        return new StepVertexPointEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            vertexGeometryReferenceId:
                vertexGeometryReferenceParameter.EntityId);
    }
}