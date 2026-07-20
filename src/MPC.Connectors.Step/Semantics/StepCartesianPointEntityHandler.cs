using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepCartesianPointEntityHandler
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
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Handler for {SupportedEntityType} cannot process entity type '{record.SourceEntityType}'.");
        }

        if (parameters.Count != 2)
        {
            throw new FormatException(
                "CARTESIAN_POINT must contain exactly two parameters: name and coordinates.");
        }

        if (parameters[0] is not
            StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first CARTESIAN_POINT parameter must be a STEP string.");
        }

        if (parameters[1] is not
            StepAggregateParameter coordinateAggregate)
        {
            throw new FormatException(
                "The second CARTESIAN_POINT parameter must be a STEP aggregate.");
        }

        if (coordinateAggregate.Items.Count == 0)
        {
            throw new FormatException(
                "CARTESIAN_POINT coordinate aggregate cannot be empty.");
        }

        var coordinates =
            new List<double>();

        foreach (var coordinateParameter
                 in coordinateAggregate.Items)
        {
            coordinates.Add(
                ReadCoordinate(
                    coordinateParameter));
        }

        return new StepCartesianPointEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            coordinates:
                coordinates);
    }

    private static double ReadCoordinate(
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
                    "CARTESIAN_POINT coordinates must contain only integer or real numeric parameters.")
        };
    }
}