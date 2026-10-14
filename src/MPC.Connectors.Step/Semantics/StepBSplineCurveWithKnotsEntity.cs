namespace MPC.Connectors.Step.Semantics;

public sealed class StepBSplineCurveWithKnotsEntity : StepSemanticEntity
{
    public string Name { get; }
    public long Degree { get; }
    public IReadOnlyList<string> ControlPointReferences { get; }
    public string CurveForm { get; }
    public StepLogicalValue ClosedCurve { get; }
    public StepLogicalValue SelfIntersect { get; }
    public IReadOnlyList<long> Multiplicities { get; }
    public IReadOnlyList<double> Knots { get; }
    public string KnotSpecification { get; }

    public StepBSplineCurveWithKnotsEntity(string? sourceEntityId, string name, long degree,
        IEnumerable<string> controlPointReferences, string curveForm, StepLogicalValue closedCurve,
        StepLogicalValue selfIntersect, IEnumerable<long> multiplicities, IEnumerable<double> knots,
        string knotSpecification) : base(sourceEntityId, "B_SPLINE_CURVE_WITH_KNOTS")
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(controlPointReferences);
        ArgumentNullException.ThrowIfNull(multiplicities);
        ArgumentNullException.ThrowIfNull(knots);
        if (degree < 1) throw new ArgumentOutOfRangeException(nameof(degree));
        string[] points = controlPointReferences.ToArray();
        if (points.Length <= degree || points.Any(p => string.IsNullOrWhiteSpace(p) ||
            p.Length < 2 || p[0] != '#' || p.Skip(1).Any(c => c < '0' || c > '9')))
            throw new ArgumentException("Expected degree plus one valid control-point references.", nameof(controlPointReferences));
        if (curveForm is not ("POLYLINE_FORM" or "CIRCULAR_ARC" or "ELLIPTIC_ARC" or
            "PARABOLIC_ARC" or "HYPERBOLIC_ARC" or "UNSPECIFIED"))
            throw new ArgumentException("Unsupported curve form.", nameof(curveForm));
        if (!Enum.IsDefined(closedCurve)) throw new ArgumentOutOfRangeException(nameof(closedCurve));
        if (!Enum.IsDefined(selfIntersect)) throw new ArgumentOutOfRangeException(nameof(selfIntersect));
        if (knotSpecification is not ("UNIFORM_KNOTS" or "QUASI_UNIFORM_KNOTS" or
            "PIECEWISE_BEZIER_KNOTS" or "UNSPECIFIED"))
            throw new ArgumentException("Unsupported knot specification.", nameof(knotSpecification));
        long[] counts = multiplicities.ToArray();
        double[] values = knots.ToArray();
        if (values.Length < 2 || values.Any(x => !double.IsFinite(x)) ||
            values.Skip(1).Where((x, i) => x <= values[i]).Any())
            throw new ArgumentException("Distinct knots must be finite and strictly increasing.", nameof(knots));
        if (counts.Length != values.Length ||
            counts.Where((x, i) => x <= 0 || x > (i == 0 || i == counts.Length - 1 ? degree + 1 : degree)).Any() ||
            counts.Aggregate(0m, (sum, x) => sum + x) != points.Length + (decimal)degree + 1)
            throw new ArgumentException("Invalid degree-based knot multiplicities.", nameof(multiplicities));
        Name = name;
        Degree = degree;
        ControlPointReferences = Array.AsReadOnly(points);
        CurveForm = curveForm;
        ClosedCurve = closedCurve;
        SelfIntersect = selfIntersect;
        Multiplicities = Array.AsReadOnly(counts);
        Knots = Array.AsReadOnly(values);
        KnotSpecification = knotSpecification;
    }
}
