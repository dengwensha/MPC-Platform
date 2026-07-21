using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepCartesianPointHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "CARTESIAN_POINT";

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

        if (parameters.Count != 2)
        {
            throw new FormatException(
                "CARTESIAN_POINT must contain exactly two parameters.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "CARTESIAN_POINT first parameter must be a STEP string.");

        var coordinateAggregate =
            parameters[1] as StepAggregateParameter
            ?? throw new FormatException(
                "CARTESIAN_POINT second parameter must be a STEP aggregate.");

        if (coordinateAggregate.Items.Count == 0)
        {
            throw new FormatException(
                "CARTESIAN_POINT coordinate aggregate cannot be empty.");
        }

        var coordinates =
            new List<double>(
                coordinateAggregate.Items.Count);

        foreach (var coordinateParameter
                 in coordinateAggregate.Items)
        {
            switch (coordinateParameter)
            {
                case StepRealParameter realParameter:
                    coordinates.Add(
                        realParameter.Value);
                    break;

                case StepIntegerParameter integerParameter:
                    coordinates.Add(
                        integerParameter.Value);
                    break;

                default:
                    throw new FormatException(
                        "CARTESIAN_POINT coordinates must be integer or real parameters.");
            }
        }

        return new StepCartesianPoint(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            coordinates:
                coordinates);
    }
}