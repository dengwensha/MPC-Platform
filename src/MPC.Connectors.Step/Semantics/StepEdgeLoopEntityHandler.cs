using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepEdgeLoopEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "EDGE_LOOP";

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
                "EDGE_LOOP must contain exactly two parameters: name and oriented edge aggregate.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "EDGE_LOOP first parameter must be a STEP string.");

        var edgeAggregateParameter =
            parameters[1] as StepAggregateParameter
            ?? throw new FormatException(
                "EDGE_LOOP second parameter must be a STEP aggregate.");

        if (edgeAggregateParameter.Items.Count == 0)
        {
            throw new FormatException(
                "EDGE_LOOP oriented edge aggregate must contain at least one entity reference.");
        }

        var orientedEdgeReferenceIds =
            new List<string>(
                edgeAggregateParameter.Items.Count);

        for (var index = 0;
             index < edgeAggregateParameter.Items.Count;
             index++)
        {
            if (edgeAggregateParameter.Items[index]
                is not StepEntityReferenceParameter referenceParameter)
            {
                throw new FormatException(
                    $"EDGE_LOOP aggregate item at index {index} must be a STEP entity reference.");
            }

            orientedEdgeReferenceIds.Add(
                referenceParameter.EntityId);
        }

        return new StepEdgeLoopEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            orientedEdgeReferenceIds:
                orientedEdgeReferenceIds);
    }
}