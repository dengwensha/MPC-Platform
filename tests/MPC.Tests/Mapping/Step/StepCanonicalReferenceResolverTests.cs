using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalReferenceResolverTests
{
    [Fact]
    public void Resolve_VertexPoint_ReturnsCanonicalVertex()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Vertex position",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var vertexPoint =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                point,
                vertexPoint);

        MappingResult<Vertex> result =
            resolver.Resolve<Vertex>(
                "#20",
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Vertex vertex =
            Assert.IsType<Vertex>(
                result.Value);

        Assert.Equal(
            1.0,
            vertex.Position.X);

        Assert.Equal(
            2.0,
            vertex.Position.Y);

        Assert.Equal(
            3.0,
            vertex.Position.Z);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Resolve_SameVertexReferenceTwice_ReturnsSameCanonicalInstance()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Vertex position",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var vertexPoint =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                point,
                vertexPoint);

        MappingResult<Vertex> first =
            resolver.Resolve<Vertex>(
                "#20",
                context);

        MappingResult<Vertex> second =
            resolver.Resolve<Vertex>(
                "#20",
                context);

        Assert.True(
            first.IsSuccessful);

        Assert.True(
            second.IsSuccessful);

        Vertex firstVertex =
            Assert.IsType<Vertex>(
                first.Value);

        Vertex secondVertex =
            Assert.IsType<Vertex>(
                second.Value);

        Assert.Same(
            firstVertex,
            secondVertex);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Resolve_LineAsCurve_ReturnsCanonicalLine()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var line =
            new StepLineEntity(
                "#40",
                "Line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                point,
                direction,
                vector,
                line);

        MappingResult<ICurve3D> result =
            resolver.Resolve<ICurve3D>(
                "#40",
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Line3D canonicalLine =
            Assert.IsType<Line3D>(
                result.Value);

        Assert.Equal(
            new Point3D(
                0.0,
                0.0,
                0.0),
            canonicalLine.Origin);

        Assert.Equal(
            new Vector3D(
                1.0,
                0.0,
                0.0),
            canonicalLine.Direction);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Resolve_LineFirstAsLineThenAsCurve_ReturnsSameCanonicalInstance()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    0.0,
                    1.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var line =
            new StepLineEntity(
                "#40",
                "Line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                point,
                direction,
                vector,
                line);

        MappingResult<Line3D> first =
            resolver.Resolve<Line3D>(
                "#40",
                context);

        MappingResult<ICurve3D> second =
            resolver.Resolve<ICurve3D>(
                "#40",
                context);

        Assert.True(
            first.IsSuccessful);

        Assert.True(
            second.IsSuccessful);

        Line3D firstLine =
            Assert.IsType<Line3D>(
                first.Value);

        ICurve3D secondCurve =
            Assert.IsAssignableFrom<ICurve3D>(
                second.Value);

        Assert.Same(
            firstLine,
            secondCurve);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Resolve_EdgeCurve_ReturnsCanonicalEdge()
    {
        var startPoint =
            new StepCartesianPointEntity(
                "#10",
                "Start point",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var endPoint =
            new StepCartesianPointEntity(
                "#11",
                "End point",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

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
                [
                    1.0,
                    0.0,
                    0.0
                ]);

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

        var edgeCurve =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                startPoint,
                endPoint,
                startVertex,
                endVertex,
                direction,
                vector,
                line,
                edgeCurve);

        MappingResult<Edge> result =
            resolver.Resolve<Edge>(
                "#50",
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
            resolver.Resolve<Vertex>(
                "#20",
                context);

        MappingResult<Vertex> endVertexResult =
            resolver.Resolve<Vertex>(
                "#21",
                context);

        MappingResult<ICurve3D> curveResult =
            resolver.Resolve<ICurve3D>(
                "#40",
                context);

        Assert.True(
            startVertexResult.IsSuccessful);

        Assert.True(
            endVertexResult.IsSuccessful);

        Assert.True(
            curveResult.IsSuccessful);

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
    public void Resolve_SameEdgeReferenceTwice_ReturnsSameCanonicalInstance()
    {
        var startPoint =
            new StepCartesianPointEntity(
                "#10",
                "Start point",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var endPoint =
            new StepCartesianPointEntity(
                "#11",
                "End point",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

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
                [
                    1.0,
                    0.0,
                    0.0
                ]);

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

        var edgeCurve =
            new StepEdgeCurveEntity(
                "#50",
                "Edge curve",
                "#20",
                "#21",
                "#40",
                true);

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                startPoint,
                endPoint,
                startVertex,
                endVertex,
                direction,
                vector,
                line,
                edgeCurve);

        MappingResult<Edge> first =
            resolver.Resolve<Edge>(
                "#50",
                context);

        MappingResult<Edge> second =
            resolver.Resolve<Edge>(
                "#50",
                context);

        Assert.True(
            first.IsSuccessful);

        Assert.True(
            second.IsSuccessful);

        Edge firstEdge =
            Assert.IsType<Edge>(
                first.Value);

        Edge secondEdge =
            Assert.IsType<Edge>(
                second.Value);

        Assert.Same(
            firstEdge,
            secondEdge);

        Assert.Same(
            firstEdge.StartVertex,
            secondEdge.StartVertex);

        Assert.Same(
            firstEdge.EndVertex,
            secondEdge.EndVertex);

        Assert.Same(
            firstEdge.Curve,
            secondEdge.Curve);

        Assert.Empty(
            first.Diagnostics);

        Assert.Empty(
            second.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Resolve_CacheHitWithPreExistingContextDiagnostic_DoesNotIncludeItInResult()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Vertex position",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var vertexPoint =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                point,
                vertexPoint);

        MappingResult<Vertex> first =
            resolver.Resolve<Vertex>(
                "#20",
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            first.Status);

        Assert.Empty(
            first.Diagnostics);

        Vertex firstVertex =
            Assert.IsType<Vertex>(
                first.Value);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        MappingResult<Vertex> second =
            resolver.Resolve<Vertex>(
                "#20",
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            second.Status);

        Assert.True(
            second.IsSuccessful);

        Vertex secondVertex =
            Assert.IsType<Vertex>(
                second.Value);

        Assert.Same(
            firstVertex,
            secondVertex);

        Assert.Empty(
            second.Diagnostics);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Resolve_WhenReferenceDoesNotExist_ReturnsFailure()
    {
        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver);

        MappingResult<Vertex> result =
            resolver.Resolve<Vertex>(
                "#999",
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
            "STEP_REFERENCE_RESOLUTION_FAILED",
            diagnostic.Code);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Resolve_WhenSemanticEntityIsUnsupported_ReturnsFailure()
    {
        var direction =
            new StepDirectionEntity(
                "#10",
                "Direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                direction);

        MappingResult<Vertex> result =
            resolver.Resolve<Vertex>(
                "#10",
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
            "MAP-STEP-CANONICAL-REFERENCE-001",
            diagnostic.Code);

        Assert.Contains(
            "DIRECTION",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Resolve_WhenRequestedCanonicalTypeDoesNotMatch_ReturnsFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var line =
            new StepLineEntity(
                "#40",
                "Line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver,
                point,
                direction,
                vector,
                line);

        MappingResult<Vertex> result =
            resolver.Resolve<Vertex>(
                "#40",
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-CANONICAL-REFERENCE-002",
            diagnostic.Code);

        Assert.Contains(
            "Line3D",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "Vertex",
            diagnostic.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_WithNullContext_ThrowsArgumentNullException()
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                Array.Empty<StepSemanticEntity>());

        var resolver =
            new StepCanonicalReferenceResolver(
                semanticResolver);

        Assert.Throws<ArgumentNullException>(
            () => resolver.Resolve<Vertex>(
                "#10",
                null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Resolve_WithInvalidReferenceId_ThrowsArgumentException(
        string referenceId)
    {
        MappingContext context =
            CreateContext(
                out StepCanonicalReferenceResolver resolver);

        Assert.Throws<ArgumentException>(
            () => resolver.Resolve<Vertex>(
                referenceId,
                context));
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