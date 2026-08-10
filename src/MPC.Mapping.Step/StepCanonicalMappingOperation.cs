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

        _solidMapper =
            solidMapper;
    }

    public MappingContext Context
    {
        get;
    }

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

        var edgeCurveMapper =
            new StepEdgeCurveMapper();

        var cylindricalSurfaceMapper =
            new StepCylindricalSurfaceGeometryMapper();

        var canonicalReferenceResolver =
            new StepCanonicalReferenceResolver(
                semanticResolver,
                vertexPointMapper,
                lineMapper,
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

        var advancedFaceGeometryMapper =
            new StepAdvancedFaceGeometryMapper(
                planeMapper,
                cylindricalSurfaceMapper,
                conicalSurfaceMapper,
                sphericalSurfaceMapper);

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

        var closedShellMapper =
            new StepClosedShellMapper(
                advancedFaceMapper);

        var solidMapper =
            new StepManifoldSolidBrepMapper(
                closedShellMapper);

        return new StepCanonicalMappingOperation(
            context,
            solidMapper);
    }

    public MappingResult<Solid> Map(
        StepManifoldSolidBrepEntity source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        return _solidMapper.Map(
            source,
            Context);
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