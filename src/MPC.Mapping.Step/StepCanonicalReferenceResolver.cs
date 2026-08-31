using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepCanonicalReferenceResolver :
    ICanonicalReferenceResolver
{
    private const string UnsupportedSemanticEntityDiagnosticCode =
        "MAP-STEP-CANONICAL-REFERENCE-001";

    private const string CanonicalTypeMismatchDiagnosticCode =
        "MAP-STEP-CANONICAL-REFERENCE-002";

    private readonly StepSemanticEntityResolver
        _semanticResolver;

    private readonly IEntityMapper<
        StepVertexPointEntity,
        Vertex> _vertexPointMapper;

    private readonly IEntityMapper<
        StepLineEntity,
        Line3D> _lineMapper;

    private readonly IEntityMapper<
        StepCircleEntity,
        Circle3D> _circleMapper;

    private readonly IEntityMapper<
        StepEdgeCurveEntity,
        Edge> _edgeCurveMapper;

    private readonly IEntityMapper<
        StepCylindricalSurfaceEntity,
        Cylinder3D> _cylindricalSurfaceMapper;

    private readonly Dictionary<string, object>
        _canonicalCache;

    public StepCanonicalReferenceResolver(
        StepSemanticEntityResolver semanticResolver)
        : this(
            semanticResolver,
            new StepVertexPointMapper(),
            new StepLineGeometryMapper(),
            new StepCircleGeometryMapper(),
            new StepEdgeCurveMapper(),
            new StepCylindricalSurfaceGeometryMapper())
    {
    }

    public StepCanonicalReferenceResolver(
        StepSemanticEntityResolver semanticResolver,
        IEntityMapper<
            StepVertexPointEntity,
            Vertex> vertexPointMapper,
        IEntityMapper<
            StepLineEntity,
            Line3D> lineMapper,
        IEntityMapper<
            StepCircleEntity,
            Circle3D> circleMapper,
        IEntityMapper<
            StepEdgeCurveEntity,
            Edge> edgeCurveMapper,
        IEntityMapper<
            StepCylindricalSurfaceEntity,
            Cylinder3D> cylindricalSurfaceMapper)
    {
        ArgumentNullException.ThrowIfNull(
            semanticResolver);

        ArgumentNullException.ThrowIfNull(
            vertexPointMapper);

        ArgumentNullException.ThrowIfNull(
            lineMapper);

        ArgumentNullException.ThrowIfNull(
            circleMapper);

        ArgumentNullException.ThrowIfNull(
            edgeCurveMapper);

        ArgumentNullException.ThrowIfNull(
            cylindricalSurfaceMapper);

        _semanticResolver =
            semanticResolver;

        _vertexPointMapper =
            vertexPointMapper;

        _lineMapper =
            lineMapper;

        _circleMapper =
            circleMapper;

        _edgeCurveMapper =
            edgeCurveMapper;

        _cylindricalSurfaceMapper =
            cylindricalSurfaceMapper;

        _canonicalCache =
            new Dictionary<string, object>(
                StringComparer.Ordinal);
    }

    public MappingResult<T> Resolve<T>(
        string referenceId,
        MappingContext context)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(
                referenceId))
        {
            throw new ArgumentException(
                "Canonical STEP reference identifier cannot be null, empty, or whitespace.",
                nameof(referenceId));
        }

        ArgumentNullException.ThrowIfNull(
            context);

        if (_canonicalCache.TryGetValue(
                referenceId,
                out object? cachedValue))
        {
            return CreateCachedResult<T>(
                referenceId,
                cachedValue,
                context);
        }

        StepSemanticEntity semanticEntity;

        try
        {
            semanticEntity =
                _semanticResolver.Resolve(
                    referenceId);
        }
        catch (InvalidOperationException exception)
        {
            var diagnostic =
                new MappingDiagnostic(
                    "STEP_REFERENCE_RESOLUTION_FAILED",
                    exception.Message,
                    MappingDiagnosticSeverity.Error);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<T>.Failure(
                [
                    diagnostic
                ]);
        }

        MappingResult<object> mappingResult =
            MapSemanticEntity(
                semanticEntity,
                context);

        if (!mappingResult.IsSuccessful ||
            mappingResult.Value is null)
        {
            return MappingResult<T>.Failure(
                mappingResult.Diagnostics);
        }

        object canonicalValue =
            mappingResult.Value;

        if (canonicalValue is not T typedValue)
        {
            MappingDiagnostic diagnostic =
                CreateTypeMismatchDiagnostic<T>(
                    referenceId,
                    canonicalValue);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<T>.Failure(
                [
                    diagnostic
                ]);
        }

        _canonicalCache.Add(
            referenceId,
            canonicalValue);

        if (mappingResult.Status ==
            MappingStatus.SucceededWithDiagnostics)
        {
            return MappingResult<T>
                .SuccessWithDiagnostics(
                    typedValue,
                    mappingResult.Diagnostics);
        }

        return MappingResult<T>.Success(
            typedValue);
    }

    private MappingResult<object> MapSemanticEntity(
        StepSemanticEntity semanticEntity,
        MappingContext context)
    {
        switch (semanticEntity)
        {
            case StepVertexPointEntity vertexPoint:
                {
                    MappingResult<Vertex> result =
                        _vertexPointMapper.Map(
                            vertexPoint,
                            context);

                    return ConvertResult(
                        result);
                }

            case StepLineEntity line:
                {
                    MappingResult<Line3D> result =
                        _lineMapper.Map(
                            line,
                            context);

                    return ConvertResult(
                        result);
                }

            case StepCircleEntity circle:
                {
                    MappingResult<Circle3D> result =
                        _circleMapper.Map(
                            circle,
                            context);

                    return ConvertResult(
                        result);
                }

            case StepEdgeCurveEntity edgeCurve:
                {
                    MappingResult<Edge> result =
                        _edgeCurveMapper.Map(
                            edgeCurve,
                            context);

                    return ConvertResult(
                        result);
                }

            case StepCylindricalSurfaceEntity cylindricalSurface:
                {
                    MappingResult<Cylinder3D> result =
                        _cylindricalSurfaceMapper.Map(
                            cylindricalSurface,
                            context);

                    return ConvertResult(
                        result);
                }

            default:
                {
                    MappingDiagnostic diagnostic =
                        CreateUnsupportedSemanticEntityDiagnostic(
                            semanticEntity);

                    context.AddDiagnostic(
                        diagnostic);

                    return MappingResult<object>.Failure(
                        [
                            diagnostic
                        ]);
                }
        }
    }

    private static MappingResult<object> ConvertResult<T>(
        MappingResult<T> source)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(
            source);

        if (!source.IsSuccessful ||
            source.Value is null)
        {
            return MappingResult<object>.Failure(
                source.Diagnostics);
        }

        if (source.Status ==
            MappingStatus.SucceededWithDiagnostics)
        {
            return MappingResult<object>
                .SuccessWithDiagnostics(
                    source.Value,
                    source.Diagnostics);
        }

        return MappingResult<object>.Success(
            source.Value);
    }

    private static MappingResult<T> CreateCachedResult<T>(
        string referenceId,
        object cachedValue,
        MappingContext context)
        where T : class
    {
        if (cachedValue is not T typedValue)
        {
            MappingDiagnostic diagnostic =
                CreateTypeMismatchDiagnostic<T>(
                    referenceId,
                    cachedValue);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<T>.Failure(
                [
                    diagnostic
                ]);
        }

        return MappingResult<T>.Success(
            typedValue);
    }

    private static MappingDiagnostic
        CreateUnsupportedSemanticEntityDiagnostic(
            StepSemanticEntity semanticEntity)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                semanticEntity.SourceEntityId)
                ? semanticEntity.SourceEntityType
                : semanticEntity.SourceEntityId;

        return new MappingDiagnostic(
            UnsupportedSemanticEntityDiagnosticCode,
            $"STEP semantic entity '{sourceIdentity}' of type " +
            $"'{semanticEntity.SourceEntityType}' does not have a registered " +
            "canonical mapping in the STEP canonical reference resolver.",
            MappingDiagnosticSeverity.Error);
    }

    private static MappingDiagnostic
        CreateTypeMismatchDiagnostic<T>(
            string referenceId,
            object canonicalValue)
        where T : class
    {
        return new MappingDiagnostic(
            CanonicalTypeMismatchDiagnosticCode,
            $"STEP reference '{referenceId}' resolved to canonical type " +
            $"'{canonicalValue.GetType().Name}', but " +
            $"'{typeof(T).Name}' was requested.",
            MappingDiagnosticSeverity.Error);
    }
}