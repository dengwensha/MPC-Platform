using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepAdvancedFaceBoundaryOrientationTests
{
    [Fact]
    public void Map_WithMixedBoundaryOrientations_PreservesIndependentCanonicalDirections()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        Loop innerLoop =
            CreateTriangleLoop(
                10.0);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var innerBound =
            new StepFaceBoundEntity(
                "#21",
                "Inner boundary",
                "#31",
                false);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound,

                    ["#21"] =
                        innerBound
                });

        var faceBuilder =
            new FaceBuilder(
                new TestSurface3D());

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>.Success(
                        faceBuilder)),
                new StubFaceBoundMapper(
                    new Dictionary<
                        string,
                        MappingResult<FaceLoopMapping>>
                    {
                        ["#20"] =
                            MappingResult<FaceLoopMapping>
                                .Success(
                                    new FaceLoopMapping(
                                        outerLoop,
                                        true,
                                        true)),

                        ["#21"] =
                            MappingResult<FaceLoopMapping>
                                .Success(
                                    new FaceLoopMapping(
                                        innerLoop,
                                        false,
                                        false))
                    }));

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepAdvancedFaceEntity(
                "#100",
                "Advanced face",
                new[]
                {
                    "#20",
                    "#21"
                },
                "#500",
                false);

        MappingResult<Face> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Face face =
            Assert.IsType<Face>(
                result.Value);

        Assert.Equal(
            FaceOrientation.Reversed,
            face.Orientation);

        Assert.Same(
            outerLoop,
            face.OuterLoopUse.Loop);

        Assert.Equal(
            LoopUseDirection.Forward,
            face.OuterLoopUse.Direction);

        LoopUse innerLoopUse =
            Assert.Single(
                face.InnerLoopUses);

        Assert.Same(
            innerLoop,
            innerLoopUse.Loop);

        Assert.Equal(
            LoopUseDirection.Reversed,
            innerLoopUse.Direction);
    }

    private static Loop CreateTriangleLoop(
        double offset)
    {
        Vertex first =
            CreateVertex(
                offset,
                0.0);

        Vertex second =
            CreateVertex(
                offset + 1.0,
                0.0);

        Vertex third =
            CreateVertex(
                offset,
                1.0);

        Edge firstEdge =
            CreateEdge(
                first,
                second);

        Edge secondEdge =
            CreateEdge(
                second,
                third);

        Edge thirdEdge =
            CreateEdge(
                third,
                first);

        return new Loop(
            [
                new EdgeUse(
                    firstEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    secondEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    thirdEdge,
                    EdgeUseDirection.Forward)
            ]);
    }

    private static Vertex CreateVertex(
        double x,
        double y)
    {
        return new Vertex(
            new Point3D(
                x,
                y,
                0.0));
    }

    private static Edge CreateEdge(
        Vertex startVertex,
        Vertex endVertex)
    {
        return new Edge(
            startVertex,
            endVertex,
            new TestCurve3D());
    }

    private sealed class TestCurve3D :
        ICurve3D
    {
    }

    private sealed class TestSurface3D :
        ISurface3D
    {
    }

    private sealed class StubFaceBuilderMapper :
        IEntityMapper<
            StepAdvancedFaceEntity,
            FaceBuilder>
    {
        private readonly MappingResult<FaceBuilder>
            _result;

        public StubFaceBuilderMapper(
            MappingResult<FaceBuilder> result)
        {
            ArgumentNullException.ThrowIfNull(
                result);

            _result =
                result;
        }

        public MappingResult<FaceBuilder> Map(
            StepAdvancedFaceEntity source,
            MappingContext context)
        {
            return _result;
        }
    }

    private sealed class StubFaceBoundMapper :
        IEntityMapper<
            StepFaceBoundEntity,
            FaceLoopMapping>
    {
        private readonly IReadOnlyDictionary<
            string,
            MappingResult<FaceLoopMapping>> _results;

        public StubFaceBoundMapper(
            IReadOnlyDictionary<
                string,
                MappingResult<FaceLoopMapping>> results)
        {
            _results =
                results
                ?? throw new ArgumentNullException(
                    nameof(results));
        }

        public MappingResult<FaceLoopMapping> Map(
            StepFaceBoundEntity source,
            MappingContext context)
        {
            string sourceEntityId =
                source.SourceEntityId
                ?? throw new InvalidOperationException(
                    "The test FACE_BOUND entity must contain a source entity ID.");

            if (!_results.TryGetValue(
                    sourceEntityId,
                    out MappingResult<FaceLoopMapping>? result))
            {
                throw new InvalidOperationException(
                    $"No FACE_BOUND mapping result was configured for '{sourceEntityId}'.");
            }

            return result;
        }
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly IReadOnlyDictionary<
            string,
            StepFaceBoundEntity> _faceBounds;

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepFaceBoundEntity> faceBounds)
        {
            _faceBounds =
                faceBounds
                ?? throw new ArgumentNullException(
                    nameof(faceBounds));
        }

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (typeof(T) !=
                typeof(StepFaceBoundEntity))
            {
                throw new NotSupportedException(
                    $"Resolution of '{typeof(T).Name}' is not supported by this test resolver.");
            }

            if (!_faceBounds.TryGetValue(
                    referenceId,
                    out StepFaceBoundEntity? faceBound))
            {
                throw new InvalidOperationException(
                    $"No FACE_BOUND test entity was configured for '{referenceId}'.");
            }

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                Assert.IsAssignableFrom<T>(
                    faceBound));
        }
    }
}