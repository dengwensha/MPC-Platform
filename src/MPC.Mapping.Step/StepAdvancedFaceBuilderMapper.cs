using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Topology.Building;

namespace MPC.Mapping.Step;

public sealed class StepAdvancedFaceBuilderMapper :
    IEntityMapper<
        StepAdvancedFaceEntity,
        FaceBuilder>
{
    private readonly StepAdvancedFaceGeometryMapper _geometryMapper;

    public StepAdvancedFaceBuilderMapper(
        StepAdvancedFaceGeometryMapper geometryMapper)
    {
        _geometryMapper =
            geometryMapper
            ?? throw new ArgumentNullException(
                nameof(geometryMapper));
    }

    public MappingResult<FaceBuilder> Map(
        StepAdvancedFaceEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        MappingResult<ISurface3D> geometryResult =
            _geometryMapper.Map(
                source,
                context);

        if (!geometryResult.IsSuccessful ||
            geometryResult.Value is null)
        {
            return MappingResult<FaceBuilder>.Failure(
                geometryResult.Diagnostics);
        }

        var faceBuilder =
            new FaceBuilder(
                geometryResult.Value);

        if (geometryResult.Status ==
            MappingStatus.SucceededWithDiagnostics)
        {
            return MappingResult<FaceBuilder>
                .SuccessWithDiagnostics(
                    faceBuilder,
                    geometryResult.Diagnostics);
        }

        return MappingResult<FaceBuilder>.Success(
            faceBuilder);
    }
}