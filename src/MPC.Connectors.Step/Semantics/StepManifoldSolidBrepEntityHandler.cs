using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepManifoldSolidBrepEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "MANIFOLD_SOLID_BREP";

    public StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(parameters);

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
                "MANIFOLD_SOLID_BREP must contain exactly two parameters: name and outer shell reference.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "MANIFOLD_SOLID_BREP first parameter must be a STEP string.");

        var shellReferenceParameter =
            parameters[1] as StepEntityReferenceParameter
            ?? throw new FormatException(
                "MANIFOLD_SOLID_BREP second parameter must be a STEP entity reference.");

        return new StepManifoldSolidBrepEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            outerShellReferenceId:
                shellReferenceParameter.EntityId);
    }
}