using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepDirectionHandler
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
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"STEP semantic handler for '{SupportedEntityType}' cannot read entity type '{record.SourceEntityType}'.",
                nameof(record));
        }

        if (parameters.Count != 2)
        {
            throw new FormatException(
                "DIRECTION must contain exactly two parameters.");
        }

        var nameParameter =
            parameters[0] as StepStringParameter
            ?? throw new FormatException(
                "DIRECTION first parameter must be a STEP string.");

        var ratioAggregate =
            parameters[1] as StepAggregateParameter
            ?? throw new FormatException(
                "DIRECTION second parameter must be a STEP aggregate.");

        if (ratioAggregate.Items.Count == 0)
        {
            throw new FormatException(
                "DIRECTION ratio aggregate cannot be empty.");
        }

        var directionRatios =
            new List<double>(
                ratioAggregate.Items.Count);

        foreach (var ratioParameter
                 in ratioAggregate.Items)
        {
            switch (ratioParameter)
            {
                case StepRealParameter realParameter:
                    directionRatios.Add(
                        realParameter.Value);
                    break;

                case StepIntegerParameter integerParameter:
                    directionRatios.Add(
                        integerParameter.Value);
                    break;

                default:
                    throw new FormatException(
                        "DIRECTION ratios must be integer or real parameters.");
            }
        }

        return new StepDirection(
            sourceEntityId:
                record.SourceEntityId,
            name:
                nameParameter.Value,
            directionRatios:
                directionRatios);
    }
}