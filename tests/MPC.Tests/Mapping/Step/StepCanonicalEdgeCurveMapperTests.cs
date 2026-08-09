using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalEdgeCurveMapperTests
{
    [Fact]
    public void Map_WithValidReferences_ReturnsCanonicalEdge()
    {
        var startPoint =
            new StepCartesianPointEntity(
                "#10",
                "Start point",
                [0.0, 0.0, 0.0]);

        var endPoint =
            new StepCartesianPointEntity(
                "#11",
                "End point",
                [1.0, 0.0, 0.0]);

        var startVertex =
            new StepVertexPointEntity(
                "#20",
                "Start vertex",
                "#10");

        var endVertex =
            new StepVertexPointEntity(
                "#21",
                "End vertex",
                "#11");

        var direction =
            new StepDirectionEntity(
                "#30",
                "Line direction",
                [1.0, 0.0, 0.0]);

        var vector =
            new StepVectorEntity(
                "#31",
                "Line vector",
                "#30",
                1.0);

        var line =
            new StepLineEntity(
                "#40",
                "Edge line",
                "#10",
                "#31");

        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver canonicalResolver,
                startPoint,
                endPoint,
                startVertex,
                endVertex,
                direction,
                vector,
                line,
                source);

        var mapper =
            new StepEdgeCurveMapper();

        MappingResult<Edge> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Edge edge =
            Assert.IsType<Edge>(
                result.Value);

        MappingResult<Vertex> startVertexResult =
            canonicalResolver.Resolve<Vertex>(
                "#20",
                context);

        MappingResult<Vertex> endVertexResult =
            canonicalResolver.Resolve<Vertex>(
                "#21",
                context);

        MappingResult<ICurve3D> curveResult =
            canonicalResolver.Resolve<ICurve3D>(
                "#40",
                context);

        Assert.Same(
            startVertexResult.Value,
            edge.StartVertex);

        Assert.Same(
            endVertexResult.Value,
            edge.EndVertex);

        Assert.Same(
            curveResult.Value,
            edge.Curve);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_ReusesCanonicalVertexInstances()
    {
        var startPoint =
            new StepCartesianPointEntity(
                "#10",
                "Start point",
                [0.0, 0.0, 0.0]);

        var endPoint =
            new StepCartesianPointEntity(
                "#11",
                "End point",
                [1.0, 0.0, 0.0]);

        var startVertex =
            new StepVertexPointEntity(
                "#20",
                "Start vertex",
                "#10");

        var endVertex =
            new StepVertexPointEntity(
                "#21",
                "End vertex",
                "#11");

        var direction =
            new StepDirectionEntity(
                "#30",
                "Line direction",
                [1.0, 0.0, 0.0]);

        var vector =
            new StepVectorEntity(
                "#31",
                "Line vector",
                "#30",
                1.0);

        var line =
            new StepLineEntity(
                "#40",
                "Edge line",
                "#10",
                "#31");

        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver canonicalResolver,
                startPoint,
                endPoint,
                startVertex,
                endVertex,
                direction,
                vector,
                line,
                source);

        MappingResult<Vertex> preResolvedStart =
            canonicalResolver.Resolve<Vertex>(
                "#20",
                context);

        MappingResult<Vertex> preResolvedEnd =
            canonicalResolver.Resolve<Vertex>(
                "#21",
                context);

        var mapper =
            new StepEdgeCurveMapper();

        MappingResult<Edge> result =
            mapper.Map(
                source,
                context);

        Edge edge =
            Assert.IsType<Edge>(
                result.Value);

        Assert.Same(
            preResolvedStart.Value,
            edge.StartVertex);

        Assert.Same(
            preResolvedEnd.Value,
            edge.EndVertex);
    }

    [Fact]
    public void Map_WithPreExistingContextDiagnostic_DoesNotIncludeItInSuccessfulResult()
    {
        var startPoint =
            new StepCartesianPointEntity(
                "#10",
                "Start point",
                [0.0, 0.0, 0.0]);

        var endPoint =
            new StepCartesianPointEntity(
                "#11",
                "End point",
                [1.0, 0.0, 0.0]);

        var startVertex =
            new StepVertexPointEntity(
                "#20",
                "Start vertex",
                "#10");

        var endVertex =
            new StepVertexPointEntity(
                "#21",
                "End vertex",
                "#11");

        var direction =
            new StepDirectionEntity(
                "#30",
                "Line direction",
                [1.0, 0.0, 0.0]);

        var vector =
            new StepVectorEntity(
                "#31",
                "Line vector",
                "#30",
                1.0);

        var line =
            new StepLineEntity(
                "#40",
                "Edge line",
                "#10",
                "#31");

        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        MappingContext context =
            CreateContext(
                out _,
                startPoint,
                endPoint,
                startVertex,
                endVertex,
                direction,
                vector,
                line,
                source);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-EDGE-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepEdgeCurveMapper();

        MappingResult<Edge> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.IsType<Edge>(
            result.Value);

        Assert.Empty(
            result.Diagnostics);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WhenCanonicalResolverIsMissing_ReturnsFailure()
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                Array.Empty<StepSemanticEntity>());

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver);

        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        var mapper =
            new StepEdgeCurveMapper();

        MappingResult<Edge> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-EDGE-CURVE-001",
            diagnostic.Code);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithSameSenseFalse_ReturnsFailure()
    {
        MappingContext context =
            CreateContext(
                out _);

        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                false);

        var mapper =
            new StepEdgeCurveMapper();

        MappingResult<Edge> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-EDGE-CURVE-002",
            diagnostic.Code);

        Assert.Contains(
            "SAME_SENSE = false",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WhenStartVertexCannotBeResolved_ReturnsFailure()
    {
        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        MappingContext context =
            CreateContext(
                out _,
                source);

        var mapper =
            new StepEdgeCurveMapper();

        MappingResult<Edge> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        Assert.NotEmpty(
            result.Diagnostics);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        MappingContext context =
            CreateContext(
                out _);

        var mapper =
            new StepEdgeCurveMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var source =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        var mapper =
            new StepEdgeCurveMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static MappingContext CreateContext(
        out StepCanonicalReferenceResolver canonicalResolver,
        params StepSemanticEntity[] entities)
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        canonicalResolver =
            new StepCanonicalReferenceResolver(
                semanticResolver);

        return new MappingContext(
            referenceResolver,
            canonicalResolver);
    }
}