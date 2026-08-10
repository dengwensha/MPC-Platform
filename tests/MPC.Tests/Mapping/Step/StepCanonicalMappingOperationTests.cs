using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalMappingOperationTests
{
    [Fact]
    public void Resolve_SameReferenceWithinOperation_ReturnsSameCanonicalInstance()
    {
        StepSemanticEntityResolver semanticResolver =
            CreateVertexSemanticResolver();

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Vertex> first =
            operation.Resolve<Vertex>(
                "#20");

        MappingResult<Vertex> second =
            operation.Resolve<Vertex>(
                "#20");

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
            first.Diagnostics);

        Assert.Empty(
            second.Diagnostics);

        Assert.Empty(
            operation.Context.Diagnostics);
    }

    [Fact]
    public void Resolve_SameReferenceAcrossOperations_ReturnsDifferentCanonicalInstances()
    {
        StepSemanticEntityResolver semanticResolver =
            CreateVertexSemanticResolver();

        StepCanonicalMappingOperation firstOperation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        StepCanonicalMappingOperation secondOperation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Vertex> first =
            firstOperation.Resolve<Vertex>(
                "#20");

        MappingResult<Vertex> second =
            secondOperation.Resolve<Vertex>(
                "#20");

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

        Assert.NotSame(
            firstVertex,
            secondVertex);

        Assert.Equal(
            firstVertex.Position,
            secondVertex.Position);

        Assert.Empty(
            first.Diagnostics);

        Assert.Empty(
            second.Diagnostics);

        Assert.Empty(
            firstOperation.Context.Diagnostics);

        Assert.Empty(
            secondOperation.Context.Diagnostics);
    }

    [Fact]
    public void Create_WithNullSemanticResolver_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                StepCanonicalMappingOperation.Create(
                    null!));
    }

    private static StepSemanticEntityResolver CreateVertexSemanticResolver()
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

        StepSemanticEntity[] entities =
        [
            point,
            vertexPoint
        ];

        return new StepSemanticEntityResolver(
            entities);
    }
}