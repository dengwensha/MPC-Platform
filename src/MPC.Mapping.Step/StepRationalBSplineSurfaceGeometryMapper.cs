using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepRationalBSplineSurfaceGeometryMapper :
    IEntityMapper<StepRationalBSplineSurfaceEntity, RationalBSplineSurface3D>
{
    private readonly StepBSplineSurfaceWithKnotsGeometryMapper _basisMapper = new();

    public MappingResult<RationalBSplineSurface3D> Map(
        StepRationalBSplineSurfaceEntity source, MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);
        MappingResult<BSplineSurface3D> basis = _basisMapper.Map(source.Basis, context);
        if (!basis.IsSuccessful || basis.Value is null)
            return MappingResult<RationalBSplineSurface3D>.Failure(basis.Diagnostics);

        // Both immutable contracts validate identical grid dimensions and positive finite weights.
        var surface = new RationalBSplineSurface3D(basis.Value, source.Weights);
        return basis.Diagnostics.Count == 0
            ? MappingResult<RationalBSplineSurface3D>.Success(surface)
            : MappingResult<RationalBSplineSurface3D>.SuccessWithDiagnostics(surface, basis.Diagnostics);
    }
}
