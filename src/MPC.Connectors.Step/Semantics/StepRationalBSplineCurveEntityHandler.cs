using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepRationalBSplineCurveEntityHandler : IStepComplexSemanticEntityHandler
{
    private static readonly IReadOnlyDictionary<string, int> Arities =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["BOUNDED_CURVE"] = 0,
            ["B_SPLINE_CURVE"] = 5,
            ["B_SPLINE_CURVE_WITH_KNOTS"] = 3,
            ["CURVE"] = 0,
            ["GEOMETRIC_REPRESENTATION_ITEM"] = 0,
            ["RATIONAL_B_SPLINE_CURVE"] = 1,
            ["REPRESENTATION_ITEM"] = 1
        };

    public string SupportedEntityType => "COMPLEX_ENTITY";

    public bool CanRead(StepDataEntityRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return record.SourceEntityType == SupportedEntityType &&
            record.ComponentTypes.Contains("RATIONAL_B_SPLINE_CURVE", StringComparer.Ordinal);
    }

    public StepSemanticEntity Read(
        StepDataEntityRecord record, IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(parameters);
        if (!CanRead(record)) throw new InvalidOperationException("Unsupported complex curve family.");
        var components = new Dictionary<string, StepTypedParameter>(StringComparer.Ordinal);
        var names = new List<string>();
        foreach (StepParameter parameter in parameters)
        {
            if (parameter is not StepTypedParameter component)
                throw new FormatException("Expected a named rational curve component.");
            string name = component.TypeName.ToUpperInvariant();
            names.Add(name);
            if (!Arities.TryGetValue(name, out int arity) ||
                component.Parameters.Count != arity || !components.TryAdd(name, component))
                throw new FormatException($"Invalid or duplicate rational curve component '{name}'.");
        }
        if (components.Count != Arities.Count ||
            !names.SequenceEqual(record.ComponentTypes, StringComparer.Ordinal))
            throw new FormatException("Rational curve components are missing or inconsistent with the source record.");

        StepParameter[] basisParameters =
        [
            components["REPRESENTATION_ITEM"].Parameters[0],
            .. components["B_SPLINE_CURVE"].Parameters,
            .. components["B_SPLINE_CURVE_WITH_KNOTS"].Parameters
        ];
        var basis = new StepBSplineCurveWithKnotsEntityHandler().ReadParameters(record, basisParameters);
        StepParameter weightParameter = components["RATIONAL_B_SPLINE_CURVE"].Parameters[0];
        if (weightParameter is not StepAggregateParameter aggregate)
            throw new FormatException("Rational curve weights must be an aggregate.");
        double[] weights = aggregate.Items.Select(item => item switch
        {
            StepRealParameter real => real.Value,
            StepIntegerParameter integer => (double)integer.Value,
            _ => throw new FormatException("Weights must be numeric.")
        }).ToArray();
        try { return new StepRationalBSplineCurveEntity(basis, weights); }
        catch (ArgumentException exception)
        {
            throw new StepSemanticValueException(record.SourceEntityId, record.SourceEntityType,
                "weights", weightParameter.RawText, exception);
        }
    }
}
