using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

/// <summary>Routes the supported complex B-spline families through one reader registration.</summary>
public sealed class StepComplexBSplineEntityHandler : IStepComplexSemanticEntityHandler
{
    private readonly StepRationalBSplineSurfaceEntityHandler _surface = new();
    private readonly StepRationalBSplineCurveEntityHandler _curve = new();

    public string SupportedEntityType => "COMPLEX_ENTITY";

    public bool CanRead(StepDataEntityRecord record) => _surface.CanRead(record) || _curve.CanRead(record);

    public StepSemanticEntity Read(StepDataEntityRecord record, IReadOnlyList<StepParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(parameters);
        bool surface = _surface.CanRead(record);
        bool curve = _curve.CanRead(record);
        if (surface && curve) throw new FormatException("Ambiguous rational B-spline component families.");
        if (surface) return _surface.Read(record, parameters);
        if (curve) return _curve.Read(record, parameters);
        throw new InvalidOperationException("Unsupported complex B-spline family.");
    }
}
