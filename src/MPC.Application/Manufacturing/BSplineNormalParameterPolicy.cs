namespace MPC.Application.Manufacturing;

/// <summary>Shared conservative UV domain policy for parametric B-spline normal queries.
/// A boundary is accepted only if the matching endpoint is clamped.
/// Interior knots are rejected where the one-sided derivative might be ambiguous.</summary>
internal static class BSplineNormalParameterPolicy
{
    public static bool TryEvaluate(double value, IReadOnlyList<double> expanded,
        int degree, int controlPointCount, IReadOnlyList<double> unique,
        IReadOnlyList<int> multiplicities, out double sample)
    {
        sample = default;
        if (!double.IsFinite(value) || expanded.Count <= controlPointCount ||
            degree < 1 || unique.Count != multiplicities.Count) return false;
        double minimum = expanded[degree], maximum = expanded[controlPointCount];
        if (!double.IsFinite(minimum) || !double.IsFinite(maximum) ||
            minimum >= maximum || value < minimum || value > maximum) return false;
        if (unique.Skip(1).SkipLast(1).Contains(value)) return false;
        if (value == minimum && multiplicities[0] != degree + 1) return false;
        if (value == maximum && multiplicities[^1] != degree + 1) return false;
        sample = value == maximum ? Math.BitDecrement(maximum) : value;
        return double.IsFinite(sample) && sample >= minimum && sample < maximum;
    }
}
