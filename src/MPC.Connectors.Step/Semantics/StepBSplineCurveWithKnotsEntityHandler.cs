using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepBSplineCurveWithKnotsEntityHandler : IStepSemanticEntityHandler
{
    public string SupportedEntityType => "B_SPLINE_CURVE_WITH_KNOTS";

    public StepSemanticEntity Read(StepDataEntityRecord record, IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(parameters);
        if (!string.Equals(record.SourceEntityType, SupportedEntityType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unsupported curve entity type.");
        return ReadParameters(record, parameters);
    }

    internal StepBSplineCurveWithKnotsEntity ReadParameters(
        StepDataEntityRecord record, IReadOnlyList<StepParameter> parameters)
    {
        try
        {
            if (parameters.Count != 9) throw new FormatException("B-spline curve requires nine parameters.");
            if (parameters[0] is not StepStringParameter name) throw new FormatException("Expected a curve name.");
            long degree = Integer(parameters[1]);
            var points = Aggregate(parameters[2]).Select(p => p is StepEntityReferenceParameter reference
                ? reference.EntityId : throw new FormatException("Expected a control-point reference.")).ToArray();
            string form = Enumeration(parameters[3]);
            StepLogicalValue closed = Logical(parameters[4]);
            StepLogicalValue intersect = Logical(parameters[5]);
            var multiplicities = Aggregate(parameters[6]).Select(Integer).ToArray();
            var knots = Aggregate(parameters[7]).Select(Numeric).ToArray();
            string specification = Enumeration(parameters[8]);
            return new StepBSplineCurveWithKnotsEntity(record.SourceEntityId, name.Value, degree,
                points, form, closed, intersect, multiplicities, knots, specification);
        }
        catch (FormatException exception)
        {
            throw new StepSemanticValueException(record.SourceEntityId, record.SourceEntityType,
                "parameters", record.RawStatement, exception);
        }
        catch (ArgumentException exception) when (!string.IsNullOrWhiteSpace(exception.ParamName))
        {
            int index = exception.ParamName switch
            {
                "degree" => 1, "controlPointReferences" => 2, "curveForm" => 3,
                "closedCurve" => 4, "selfIntersect" => 5, "multiplicities" => 6,
                "knots" => 7, "knotSpecification" => 8, _ => 0
            };
            throw new StepSemanticValueException(record.SourceEntityId, record.SourceEntityType,
                exception.ParamName!, parameters[index].RawText, exception);
        }
    }

    private static IReadOnlyList<StepParameter> Aggregate(StepParameter value) =>
        value is StepAggregateParameter aggregate ? aggregate.Items : throw new FormatException("Expected aggregate.");

    private static long Integer(StepParameter value) =>
        value is StepIntegerParameter integer ? integer.Value : throw new FormatException("Expected integer.");

    private static double Numeric(StepParameter value) => value switch
    {
        StepRealParameter real => real.Value,
        StepIntegerParameter integer => integer.Value,
        _ => throw new FormatException("Expected number.")
    };

    private static string Enumeration(StepParameter value) =>
        value is StepEnumerationParameter enumeration ? enumeration.Name : throw new FormatException("Expected enumeration.");

    private static StepLogicalValue Logical(StepParameter value) => Enumeration(value) switch
    {
        "T" => StepLogicalValue.True, "F" => StepLogicalValue.False, "U" => StepLogicalValue.Unknown,
        _ => throw new FormatException("Expected .T., .F. or .U.")
    };
}
