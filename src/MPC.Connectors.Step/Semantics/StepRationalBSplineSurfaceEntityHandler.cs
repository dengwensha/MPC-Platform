using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepRationalBSplineSurfaceEntityHandler : IStepComplexSemanticEntityHandler
{
    private static readonly IReadOnlyDictionary<string, int> Arities =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["BOUNDED_SURFACE"] = 0,
            ["B_SPLINE_SURFACE"] = 7,
            ["B_SPLINE_SURFACE_WITH_KNOTS"] = 5,
            ["GEOMETRIC_REPRESENTATION_ITEM"] = 0,
            ["RATIONAL_B_SPLINE_SURFACE"] = 1,
            ["REPRESENTATION_ITEM"] = 1,
            ["SURFACE"] = 0
        };

    public string SupportedEntityType => "COMPLEX_ENTITY";

    public bool CanRead(StepDataEntityRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return record.SourceEntityType == SupportedEntityType &&
            record.ComponentTypes.Contains("RATIONAL_B_SPLINE_SURFACE", StringComparer.Ordinal);
    }

    public StepSemanticEntity Read(
        StepDataEntityRecord record, IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(parameters);
        if (!CanRead(record)) throw new InvalidOperationException("Unsupported complex surface family.");
        var components = new Dictionary<string, StepTypedParameter>(StringComparer.Ordinal);
        var names = new List<string>();
        foreach (StepParameter parameter in parameters)
        {
            if (parameter is not StepTypedParameter component)
                throw new FormatException("Expected a named rational surface component.");
            string name = component.TypeName.ToUpperInvariant();
            names.Add(name);
            if (!Arities.TryGetValue(name, out int arity) ||
                component.Parameters.Count != arity || !components.TryAdd(name, component))
                throw new FormatException($"Invalid or duplicate rational surface component '{name}'.");
        }
        if (components.Count != Arities.Count ||
            !names.SequenceEqual(record.ComponentTypes, StringComparer.Ordinal))
            throw new FormatException("Rational surface components are missing or inconsistent with the source record.");

        StepParameter[] basisParameters =
        [
            components["REPRESENTATION_ITEM"].Parameters[0],
            .. components["B_SPLINE_SURFACE"].Parameters,
            .. components["B_SPLINE_SURFACE_WITH_KNOTS"].Parameters
        ];
        var basis = new StepBSplineSurfaceWithKnotsEntityHandler().ReadParameters(record, basisParameters);
        StepParameter weightParameter = components["RATIONAL_B_SPLINE_SURFACE"].Parameters[0];
        if (weightParameter is not StepAggregateParameter grid)
            throw new FormatException("Rational surface weights must be a nested aggregate.");
        var weights = new List<double[]>();
        foreach (StepParameter parameter in grid.Items)
        {
            if (parameter is not StepAggregateParameter row)
                throw new FormatException("Each weight row must be an aggregate.");
            weights.Add(row.Items.Select(item => item switch
            {
                StepRealParameter real => real.Value,
                StepIntegerParameter integer => (double)integer.Value,
                _ => throw new FormatException("Weights must be numeric.")
            }).ToArray());
        }
        try { return new StepRationalBSplineSurfaceEntity(basis, weights); }
        catch (ArgumentException exception)
        {
            throw new StepSemanticValueException(record.SourceEntityId, record.SourceEntityType,
                "weights", weightParameter.RawText, exception);
        }
    }
}
