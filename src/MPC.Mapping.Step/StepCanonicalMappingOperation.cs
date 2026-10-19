using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepCanonicalMappingOperation
{
    private readonly IEntityMapper<
        StepManifoldSolidBrepEntity,
        Solid> _solidMapper;

    private StepCanonicalMappingOperation(
        MappingContext context,
        StepFaceIdentityIndex faceIdentities,
        IEntityMapper<
            StepManifoldSolidBrepEntity,
            Solid> solidMapper)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            solidMapper);

        Context =
            context;
        FaceIdentities = faceIdentities;

        _solidMapper =
            solidMapper;
    }

    public MappingContext Context
    {
        get;
    }

    public StepFaceIdentityIndex FaceIdentities { get; }

    public static StepCanonicalMappingOperation Create(
        StepSemanticEntityResolver semanticResolver)
    {
        ArgumentNullException.ThrowIfNull(
            semanticResolver);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var vertexPointMapper =
            new StepVertexPointMapper();

        var lineMapper =
            new StepLineGeometryMapper();

        var circleMapper =
            new StepCircleGeometryMapper();

        var edgeCurveMapper =
            new StepEdgeCurveMapper();

        var cylindricalSurfaceMapper =
            new StepCylindricalSurfaceGeometryMapper();

        var canonicalReferenceResolver =
            new StepCanonicalReferenceResolver(
                semanticResolver,
                vertexPointMapper,
                lineMapper,
                circleMapper,
                edgeCurveMapper,
                cylindricalSurfaceMapper);

        var context =
            new MappingContext(
                referenceResolver,
                canonicalReferenceResolver);

        var planeMapper =
            new StepPlaneGeometryMapper();

        var conicalSurfaceMapper =
            new StepConicalSurfaceGeometryMapper();

        var sphericalSurfaceMapper =
            new StepSphericalSurfaceGeometryMapper();

        var degenerateToroidalSurfaceMapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        var toroidalSurfaceMapper =
            new StepToroidalSurfaceGeometryMapper();

        var bSplineSurfaceMapper =
            new StepBSplineSurfaceWithKnotsGeometryMapper();

        var advancedFaceGeometryMapper =
            new StepAdvancedFaceGeometryMapper(
                planeMapper,
                cylindricalSurfaceMapper,
                conicalSurfaceMapper,
                sphericalSurfaceMapper,
                degenerateToroidalSurfaceMapper,
                toroidalSurfaceMapper,
                bSplineSurfaceMapper);

        var advancedFaceBuilderMapper =
            new StepAdvancedFaceBuilderMapper(
                advancedFaceGeometryMapper);

        var orientedEdgeMapper =
            new StepOrientedEdgeMapper();

        var edgeLoopMapper =
            new StepEdgeLoopMapper(
                orientedEdgeMapper);

        var faceBoundMapper =
            new StepFaceBoundMapper(
                edgeLoopMapper);

        var advancedFaceMapper =
            new StepAdvancedFaceMapper(
                advancedFaceBuilderMapper,
                faceBoundMapper);

        var faceIdentities = new StepFaceIdentityIndex();
        var identityTrackingMapper = new StepFaceIdentityTrackingMapper(
            advancedFaceMapper, faceIdentities);

        var closedShellMapper =
            new StepClosedShellMapper(
                identityTrackingMapper);

        var solidMapper =
            new StepManifoldSolidBrepMapper(
                closedShellMapper);

        return new StepCanonicalMappingOperation(
            context,
            faceIdentities,
            solidMapper);
    }

    public MappingResult<Solid> Map(
        StepManifoldSolidBrepEntity source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        FaceIdentities.Reset();
        MappingResult<Solid> result = _solidMapper.Map(source, Context);
        if (result.IsSuccessful && result.Value is not null)
        {
            FaceIdentities.Seal();
        }
        else
        {
            FaceIdentities.Reset();
        }
        return result;
    }

    public MappingResult<T> Resolve<T>(
        string referenceId)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(
                referenceId))
        {
            throw new ArgumentException(
                "Canonical STEP reference identifier cannot be null, empty, or whitespace.",
                nameof(referenceId));
        }

        ICanonicalReferenceResolver canonicalResolver =
            Context.CanonicalReferenceResolver
            ?? throw new InvalidOperationException(
                "The STEP canonical mapping operation requires a canonical reference resolver.");

        return canonicalResolver.Resolve<T>(
            referenceId,
            Context);
    }
}
