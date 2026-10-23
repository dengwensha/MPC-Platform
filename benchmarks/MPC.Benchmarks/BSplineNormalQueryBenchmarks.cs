using BenchmarkDotNet.Attributes;
using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;

namespace MPC.Benchmarks;

/// <summary>Reference timings for existing B-spline normal queries.
/// No algorithm alternatives or asserted speedups; run Release benchmarks on one host.</summary>
[MemoryDiagnoser]
public class BSplineNormalQueryBenchmarks
{
    private BSplineSurface3D _basis = null!;
    private RationalBSplineSurface3D _rational = null!;

    [Params(2, 3)]
    public int Degree { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        int n = Degree + 2;
        var points = Enumerable.Range(0, n).Select(i =>
            Enumerable.Range(0, n).Select(j =>
                new Point3D(i, j, 0.1 * i * j)).ToArray()).ToArray();
        var weights = Enumerable.Range(0, n).Select(i =>
            Enumerable.Range(0, n).Select(j =>
                1.0 + 0.05 * (i + j)).ToArray()).ToArray();
        _basis = new BSplineSurface3D(Degree, Degree,
            points, new[] { Degree + 1, 1, Degree + 1 },
            new[] { Degree + 1, 1, Degree + 1 },
            new[] { 0.0, 0.5, 1.0 },
            new[] { 0.0, 0.5, 1.0 });
        _rational = new RationalBSplineSurface3D(_basis, weights);
        if (!BSplineSurfaceNormalQuery.TryEvaluate(_basis, 0.37, 0.63, out _) ||
            !RationalBSplineNormalQuery.TryEvaluate(_rational, 0.37, 0.63, out _))
            throw new InvalidOperationException("Benchmark fixtures must yield regular normals.");
    }

    [Benchmark]
    public bool NonRationalNormal() =>
        BSplineSurfaceNormalQuery.TryEvaluate(_basis, 0.37, 0.63, out _);

    [Benchmark]
    public bool RationalNormal() =>
        RationalBSplineNormalQuery.TryEvaluate(_rational, 0.37, 0.63, out _);
}
