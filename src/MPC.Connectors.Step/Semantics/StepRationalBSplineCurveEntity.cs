namespace MPC.Connectors.Step.Semantics;

public sealed class StepRationalBSplineCurveEntity : StepSemanticEntity
{
    public StepBSplineCurveWithKnotsEntity Basis { get; }
    public IReadOnlyList<double> Weights { get; }

    public StepRationalBSplineCurveEntity(
        StepBSplineCurveWithKnotsEntity basis, IEnumerable<double> weights)
        : base(basis?.SourceEntityId, "COMPLEX_ENTITY")
    {
        ArgumentNullException.ThrowIfNull(basis);
        ArgumentNullException.ThrowIfNull(weights);
        double[] values = weights.ToArray();
        if (values.Length != basis.ControlPointReferences.Count)
            throw new ArgumentException("Weights must match the control-point count.", nameof(weights));
        if (values.Any(value => !double.IsFinite(value) || value <= 0.0))
            throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and positive.");
        Basis = basis;
        Weights = Array.AsReadOnly(values);
    }
}
