using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepDirectionEntityHandler
    : IStepSemanticEntityHandler
{
    public string SupportedEntityType =>
        "DIRECTION";

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
                "DIRECTION must contain exactly two parameters: name and direction ratios.");
        }

        if (parameters[0] is not
            StepStringParameter nameParameter)
        {
            throw new FormatException(
                "The first DIRECTION parameter must be a STEP string.");
        }

        if (parameters[1] is not
            StepAggregateParameter ratioAggregate)
        {
            throw new FormatException(
                "The second DIRECTION parameter must be a STEP aggregate.");
        }

        if (ratioAggregate.Items.Count == 0)
        {
            throw new FormatException(
                "DIRECTION ratio aggregate cannot be empty.");
        }

        var ratios =
            new List<double>();

        foreach (var ratioParameter
                 in ratioAggregate.Items)
        {
            ratios.Add(
                ReadRatio(
                    ratioParameter));
        }

        return new StepDirectionEntity(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            ratios:
                ratios);
    }

    private static double ReadRatio(
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
                    "DIRECTION ratios must contain only integer or real numeric parameters.")
        };
    }
}