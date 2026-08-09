using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalVertexPointMapperTests
{
    [Fact]
    public void Map_WithValidCartesianPoint_ReturnsCanonicalVertex()
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

        var source =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                point,
                source);

        var mapper =
            new StepVertexPointMapper();

        MappingResult<Vertex> result =
            mapper.Map(
                source,
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
            vertex.IncidentEdges);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WhenCartesianPointReferenceCannotBeResolved_ReturnsFailure()
    {
        var source =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                source);

        var mapper =
            new StepVertexPointMapper();

        MappingResult<Vertex> result =
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
            "STEP_REFERENCE_RESOLUTION_FAILED",
            diagnostic.Code);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WhenReferenceHasWrongSemanticType_ReturnsFailure()
    {
        var direction =
            new StepDirectionEntity(
                "#10",
                "Wrong entity",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var source =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                direction,
                source);

        var mapper =
            new StepVertexPointMapper();

        MappingResult<Vertex> result =
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
            "STEP_REFERENCE_TYPE_MISMATCH",
            diagnostic.Code);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WhenCartesianPointIsNotThreeDimensional_ReturnsInvalidVertexFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Invalid vertex position",
                [
                    1.0,
                    2.0
                ]);

        var source =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                point,
                source);

        var mapper =
            new StepVertexPointMapper();

        MappingResult<Vertex> result =
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
            "MAP-STEP-VERTEX-POINT-001",
            diagnostic.Code);

        Assert.Contains(
            "CARTESIAN_POINT",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "requires exactly 3 values",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingContextDiagnostic_DoesNotIncludeItInSuccessfulResult()
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

        var source =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        MappingContext context =
            CreateContext(
                point,
                source);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepVertexPointMapper();

        MappingResult<Vertex> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
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
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        var mapper =
            new StepVertexPointMapper();

        MappingContext context =
            CreateContext();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var mapper =
            new StepVertexPointMapper();

        var source =
            new StepVertexPointEntity(
                "#20",
                "Vertex",
                "#10");

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static MappingContext CreateContext(
        params StepSemanticEntity[] entities)
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        return new MappingContext(
            referenceResolver);
    }
}