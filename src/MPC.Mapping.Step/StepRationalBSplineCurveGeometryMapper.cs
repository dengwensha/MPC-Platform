using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepRationalBSplineCurveGeometryMapper :
    IEntityMapper<StepRationalBSplineCurveEntity, RationalBSplineCurve3D>
{
    private readonly StepBSplineCurveWithKnotsGeometryMapper _basisMapper = new();

    public MappingResult<RationalBSplineCurve3D> Map(
        StepRationalBSplineCurveEntity source, MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);
        MappingResult<BSplineCurve3D> basis = _basisMapper.Map(source.Basis, context);
        if (!basis.IsSuccessful || basis.Value is null)
            return MappingResult<RationalBSplineCurve3D>.Failure(basis.Diagnostics);

        // Both immutable contracts validate identical control-point counts and positive finite weights.
        var curve = new RationalBSplineCurve3D(basis.Value, source.Weights);
        return basis.Diagnostics.Count == 0
            ? MappingResult<RationalBSplineCurve3D>.Success(curve)
            : MappingResult<RationalBSplineCurve3D>.SuccessWithDiagnostics(curve, basis.Diagnostics);
    }
}
