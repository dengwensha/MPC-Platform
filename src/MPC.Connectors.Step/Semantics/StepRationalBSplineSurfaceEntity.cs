namespace MPC.Connectors.Step.Semantics;

public sealed class StepRationalBSplineSurfaceEntity : StepSemanticEntity
{
    public StepBSplineSurfaceWithKnotsEntity Basis { get; }
    public IReadOnlyList<IReadOnlyList<double>> Weights { get; }

    public StepRationalBSplineSurfaceEntity(
        StepBSplineSurfaceWithKnotsEntity basis,
        IEnumerable<IEnumerable<double>> weights)
        : base(basis?.SourceEntityId, "COMPLEX_ENTITY")
    {
        ArgumentNullException.ThrowIfNull(basis);
        ArgumentNullException.ThrowIfNull(weights);
        var rows = new List<IReadOnlyList<double>>();
        foreach (IEnumerable<double> source in weights)
        {
            if (source is null) throw new ArgumentException("Weight rows cannot be null.", nameof(weights));
            double[] row = source.ToArray();
            if (row.Length != basis.ControlPointReferenceGrid[0].Count)
                throw new ArgumentException("Weights must match the control-point grid.", nameof(weights));
            if (row.Any(value => !double.IsFinite(value) || value <= 0.0))
                throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and positive.");
            rows.Add(Array.AsReadOnly(row));
        }
        if (rows.Count != basis.ControlPointReferenceGrid.Count)
            throw new ArgumentException("Weights must match the control-point grid.", nameof(weights));
        Basis = basis;
        Weights = Array.AsReadOnly(rows.ToArray());
    }
}
