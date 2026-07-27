using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Mapping;

namespace MPC.Mapping.Step;

public sealed class StepAdvancedFaceGeometryMapper :
    IEntityMapper<StepAdvancedFaceEntity, ISurface3D>
{
    private const string UnsupportedSurfaceDiagnosticCode =
        "MAP-STEP-FACE-001";

    private readonly StepPlaneGeometryMapper _planeMapper;

    private readonly StepCylindricalSurfaceGeometryMapper
        _cylindricalSurfaceMapper;

    private readonly StepConicalSurfaceGeometryMapper
        _conicalSurfaceMapper;

    private readonly StepSphericalSurfaceGeometryMapper
        _sphericalSurfaceMapper;

    public StepAdvancedFaceGeometryMapper()
        : this(
            new StepPlaneGeometryMapper(),
            new StepCylindricalSurfaceGeometryMapper(),
            new StepConicalSurfaceGeometryMapper(),
            new StepSphericalSurfaceGeometryMapper())
    {
    }

    public StepAdvancedFaceGeometryMapper(
        StepPlaneGeometryMapper planeMapper,
        StepCylindricalSurfaceGeometryMapper
            cylindricalSurfaceMapper,
        StepConicalSurfaceGeometryMapper
            conicalSurfaceMapper,
        StepSphericalSurfaceGeometryMapper
            sphericalSurfaceMapper)
    {
        _planeMapper =
            planeMapper
            ?? throw new ArgumentNullException(
                nameof(planeMapper));

        _cylindricalSurfaceMapper =
            cylindricalSurfaceMapper
            ?? throw new ArgumentNullException(
                nameof(cylindricalSurfaceMapper));

        _conicalSurfaceMapper =
            conicalSurfaceMapper
            ?? throw new ArgumentNullException(
                nameof(conicalSurfaceMapper));

        _sphericalSurfaceMapper =
            sphericalSurfaceMapper
            ?? throw new ArgumentNullException(
                nameof(sphericalSurfaceMapper));
    }

    public MappingResult<ISurface3D> Map(
        StepAdvancedFaceEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        ReferenceResolutionResult<StepSemanticEntity>
            surfaceResolution =
                context.ReferenceResolver
                    .Resolve<StepSemanticEntity>(
                        source.SurfaceReferenceId);

        if (!surfaceResolution.IsResolved)
        {
            return CreateResolutionFailure(
                context,
                surfaceResolution.Diagnostics);
        }

        StepSemanticEntity surfaceEntity =
            surfaceResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved ADVANCED_FACE surface reference " +
                "must contain a semantic entity.");

        return surfaceEntity switch
        {
            StepPlaneEntity plane =>
                ConvertResult(
                    _planeMapper.Map(
                        plane,
                        context)),

            StepCylindricalSurfaceEntity cylindricalSurface =>
                ConvertResult(
                    _cylindricalSurfaceMapper.Map(
                        cylindricalSurface,
                        context)),

            StepConicalSurfaceEntity conicalSurface =>
                ConvertResult(
                    _conicalSurfaceMapper.Map(
                        conicalSurface,
                        context)),

            StepSphericalSurfaceEntity sphericalSurface =>
                ConvertResult(
                    _sphericalSurfaceMapper.Map(
                        sphericalSurface,
                        context)),

            _ =>
                CreateUnsupportedSurfaceFailure(
                    source,
                    surfaceEntity,
                    context)
        };
    }

    private static MappingResult<ISurface3D>
        ConvertResult<TSurface>(
            MappingResult<TSurface> result)
        where TSurface : class, ISurface3D
    {
        ArgumentNullException.ThrowIfNull(
            result);

        if (!result.IsSuccessful ||
            result.Value is null)
        {
            return MappingResult<ISurface3D>.Failure(
                result.Diagnostics);
        }

        if (result.Status ==
            MappingStatus.SucceededWithDiagnostics)
        {
            return MappingResult<ISurface3D>
                .SuccessWithDiagnostics(
                    result.Value,
                    result.Diagnostics);
        }

        return MappingResult<ISurface3D>.Success(
            result.Value);
    }

    private static MappingResult<ISurface3D>
        CreateResolutionFailure(
            MappingContext context,
            IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        context.AddDiagnostics(
            diagnostics);

        return MappingResult<ISurface3D>.Failure(
            diagnostics);
    }

    private static MappingResult<ISurface3D>
        CreateUnsupportedSurfaceFailure(
            StepAdvancedFaceEntity source,
            StepSemanticEntity surfaceEntity,
            MappingContext context)
    {
        string faceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "ADVANCED_FACE"
                : source.SourceEntityId;

        string surfaceIdentity =
            string.IsNullOrWhiteSpace(
                surfaceEntity.SourceEntityId)
                ? surfaceEntity.GetType().Name
                : surfaceEntity.SourceEntityId;

        var diagnostic =
            new MappingDiagnostic(
                UnsupportedSurfaceDiagnosticCode,
                $"STEP ADVANCED_FACE '{faceIdentity}' references " +
                $"unsupported surface '{surfaceIdentity}' of semantic type " +
                $"'{surfaceEntity.GetType().Name}'.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            diagnostic);

        return MappingResult<ISurface3D>.Failure(
            [diagnostic]);
    }
}