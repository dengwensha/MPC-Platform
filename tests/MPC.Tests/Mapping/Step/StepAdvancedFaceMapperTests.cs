using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepAdvancedFaceMapperTests
{
    [Fact]
    public void Constructor_WithNullFaceBuilderMapper_ThrowsArgumentNullException()
    {
        var faceBoundMapper =
            new StubFaceBoundMapper(
                new Dictionary<
                    string,
                    MappingResult<FaceLoopMapping>>());

        Assert.Throws<ArgumentNullException>(
            () => new StepAdvancedFaceMapper(
                null!,
                faceBoundMapper));
    }

    [Fact]
    public void Constructor_WithNullFaceBoundMapper_ThrowsArgumentNullException()
    {
        var faceBuilderMapper =
            new StubFaceBuilderMapper(
                MappingResult<FaceBuilder>.Success(
                    new FaceBuilder(
                        new TestSurface3D())));

        Assert.Throws<ArgumentNullException>(
            () => new StepAdvancedFaceMapper(
                faceBuilderMapper,
                null!));
    }

    [Fact]
    public void Map_WithSingleOuterBound_ReturnsCanonicalFace()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound
                });

        var surface =
            new TestSurface3D();

        var faceBuilder =
            new FaceBuilder(
                surface);

        var faceBuilderMapper =
            new StubFaceBuilderMapper(
                MappingResult<FaceBuilder>.Success(
                    faceBuilder));

        var faceBoundMapper =
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
                                    true))
                });

        var mapper =
            new StepAdvancedFaceMapper(
                faceBuilderMapper,
                faceBoundMapper);

        var context =
            new MappingContext(
                resolver);

        StepAdvancedFaceEntity source =
            CreateAdvancedFace(
                "#100",
                "#20");

        MappingResult<Face> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Face face =
            Assert.IsType<Face>(
                result.Value);

        Assert.Same(
            surface,
            face.Surface);

        Assert.Same(
            outerLoop,
            face.OuterLoop);

        Assert.Empty(
            face.InnerLoops);

        Assert.True(
            faceBuilder.IsBuilt);

        Assert.Same(
            source,
            faceBuilderMapper.LastSource);

        Assert.Same(
            context,
            faceBuilderMapper.LastContext);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            resolver.ResolutionOrder);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            faceBoundMapper.MappingOrder);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WhenSameSenseIsTrue_SetsForwardFaceOrientation()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound
                });

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>.Success(
                        new FaceBuilder(
                            new TestSurface3D()))),
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
                                        true))
                    }));

        var context =
            new MappingContext(
                resolver);

        StepAdvancedFaceEntity source =
            CreateAdvancedFace(
                "#100",
                true,
                "#20");

        MappingResult<Face> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Face face =
            Assert.IsType<Face>(
                result.Value);

        Assert.Equal(
            FaceOrientation.Forward,
            face.Orientation);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WhenSameSenseIsFalse_SetsReversedFaceOrientation()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound
                });

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>.Success(
                        new FaceBuilder(
                            new TestSurface3D()))),
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
                                        true))
                    }));

        var context =
            new MappingContext(
                resolver);

        StepAdvancedFaceEntity source =
            CreateAdvancedFace(
                "#100",
                false,
                "#20");

        MappingResult<Face> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Face face =
            Assert.IsType<Face>(
                result.Value);

        Assert.Equal(
            FaceOrientation.Reversed,
            face.Orientation);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithOuterAndInnerBounds_AssignsLoopsToCorrectRoles()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        Loop firstInnerLoop =
            CreateTriangleLoop(
                10.0);

        Loop secondInnerLoop =
            CreateTriangleLoop(
                20.0);

        var firstInnerBound =
            new StepFaceBoundEntity(
                "#21",
                "First inner boundary",
                "#31",
                true);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var secondInnerBound =
            new StepFaceBoundEntity(
                "#22",
                "Second inner boundary",
                "#32",
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
                        firstInnerBound,

                    ["#22"] =
                        secondInnerBound
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
                                        firstInnerLoop,
                                        true,
                                        false)),

                        ["#22"] =
                            MappingResult<FaceLoopMapping>
                                .Success(
                                    new FaceLoopMapping(
                                        secondInnerLoop,
                                        false,
                                        false))
                    }));

        var context =
            new MappingContext(
                resolver);

        StepAdvancedFaceEntity source =
            CreateAdvancedFace(
                "#100",
                "#21",
                "#20",
                "#22");

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

        Assert.Same(
            outerLoop,
            face.OuterLoop);

        Assert.Equal(
            2,
            face.InnerLoops.Count);

        Assert.Same(
            firstInnerLoop,
            face.InnerLoops[0]);

        Assert.Same(
            secondInnerLoop,
            face.InnerLoops[1]);

        Assert.Equal(
            new[]
            {
                "#21",
                "#20",
                "#22"
            },
            resolver.ResolutionOrder);

        Assert.True(
            faceBuilder.IsBuilt);
    }

    [Fact]
    public void Map_WithNoOuterBound_ReturnsFailure()
    {
        Loop innerLoop =
            CreateTriangleLoop(
                10.0);

        var innerBound =
            new StepFaceBoundEntity(
                "#21",
                "Inner boundary",
                "#31",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
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
                        ["#21"] =
                            MappingResult<FaceLoopMapping>
                                .Success(
                                    new FaceLoopMapping(
                                        innerLoop,
                                        true,
                                        false))
                    }));

        var context =
            new MappingContext(
                resolver);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#21"),
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
            "MAP-STEP-ADVANCED-FACE-001",
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));

        Assert.False(
            faceBuilder.IsBuilt);
    }

    [Fact]
    public void Map_WithMultipleOuterBounds_ReturnsFailure()
    {
        Loop firstOuterLoop =
            CreateTriangleLoop(
                0.0);

        Loop secondOuterLoop =
            CreateTriangleLoop(
                10.0);

        var firstOuterBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "First outer boundary",
                "#30",
                true);

        var secondOuterBound =
            new StepFaceOuterBoundEntity(
                "#21",
                "Second outer boundary",
                "#31",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        firstOuterBound,

                    ["#21"] =
                        secondOuterBound
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
                                        firstOuterLoop,
                                        true,
                                        true)),

                        ["#21"] =
                            MappingResult<FaceLoopMapping>
                                .Success(
                                    new FaceLoopMapping(
                                        secondOuterLoop,
                                        true,
                                        true))
                    }));

        var context =
            new MappingContext(
                resolver);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20",
                    "#21"),
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
            "MAP-STEP-ADVANCED-FACE-002",
            diagnostic.Code);

        Assert.Contains(
            "2 FACE_OUTER_BOUND",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.False(
            faceBuilder.IsBuilt);
    }

    [Fact]
    public void Map_WithUnresolvedBoundReference_ReturnsFailure()
    {
        var diagnostic =
            new MappingDiagnostic(
                "MAP-REF-FACE-BOUND-001",
                "Referenced FACE_BOUND could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>(),
                new Dictionary<
                    string,
                    MappingDiagnostic>
                {
                    ["#20"] =
                        diagnostic
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
                        MappingResult<FaceLoopMapping>>()));

        var context =
            new MappingContext(
                resolver);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20"),
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));

        Assert.False(
            faceBuilder.IsBuilt);
    }

    [Fact]
    public void Map_WhenFaceBoundMappingFails_PropagatesFailure()
    {
        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-STEP-FACE-BOUND-001",
                "FACE_BOUND loop mapping failed.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound
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
                                .Failure(
                                    new[]
                                    {
                                        diagnostic
                                    })
                    }));

        var context =
            new MappingContext(
                resolver);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20"),
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));

        Assert.False(
            faceBuilder.IsBuilt);
    }

    [Fact]
    public void Map_WhenFaceBuilderMappingFails_PropagatesFailure()
    {
        var diagnostic =
            new MappingDiagnostic(
                "MAP-STEP-FACE-SURFACE-001",
                "ADVANCED_FACE surface mapping failed.",
                MappingDiagnosticSeverity.Error);

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>
                        .Failure(
                            new[]
                            {
                                diagnostic
                            })),
                new StubFaceBoundMapper(
                    new Dictionary<
                        string,
                        MappingResult<FaceLoopMapping>>()));

        var context =
            new MappingContext(
                new StubReferenceResolver(
                    new Dictionary<
                        string,
                        StepFaceBoundEntity>()));

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20"),
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        Assert.Same(
            diagnostic,
            Assert.Single(
                result.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingInformation_DoesNotIncludeItInSuccessfulResult()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound
                });

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>.Success(
                        new FaceBuilder(
                            new TestSurface3D()))),
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
                                        true))
                    }));

        var context =
            new MappingContext(
                resolver);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-INFO-001",
                "Existing mapping information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            diagnostic);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20"),
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

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingError_DoesNotFailSuccessfulOperation()
    {
        Loop outerLoop =
            CreateTriangleLoop(
                0.0);

        var outerBound =
            new StepFaceOuterBoundEntity(
                "#20",
                "Outer boundary",
                "#30",
                true);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>
                {
                    ["#20"] =
                        outerBound
                });

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>.Success(
                        new FaceBuilder(
                            new TestSurface3D()))),
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
                                        true))
                    }));

        var context =
            new MappingContext(
                resolver);

        var diagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-ERROR-001",
                "A previous mapping operation failed.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            diagnostic);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20"),
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

        Assert.True(
            context.HasErrors);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithUnresolvedBoundReferenceAndPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        var currentDiagnostic =
            new MappingDiagnostic(
                "MAP-REF-FACE-BOUND-001",
                "Referenced FACE_BOUND could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<
                    string,
                    StepFaceBoundEntity>(),
                new Dictionary<
                    string,
                    MappingDiagnostic>
                {
                    ["#20"] =
                        currentDiagnostic
                });

        var mapper =
            new StepAdvancedFaceMapper(
                new StubFaceBuilderMapper(
                    MappingResult<FaceBuilder>.Success(
                        new FaceBuilder(
                            new TestSurface3D()))),
                new StubFaceBoundMapper(
                    new Dictionary<
                        string,
                        MappingResult<FaceLoopMapping>>()));

        var context =
            new MappingContext(
                resolver);

        var priorDiagnostic =
            new MappingDiagnostic(
                "MAP-PRIOR-INFO-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            priorDiagnostic);

        MappingResult<Face> result =
            mapper.Map(
                CreateAdvancedFace(
                    "#100",
                    "#20"),
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Same(
            currentDiagnostic,
            resultDiagnostic);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            priorDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            currentDiagnostic,
            context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        var mapper =
            CreateMinimalMapper();

        var context =
            new MappingContext(
                new StubReferenceResolver(
                    new Dictionary<
                        string,
                        StepFaceBoundEntity>()));

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        StepAdvancedFaceEntity source =
            CreateAdvancedFace(
                "#100",
                "#20");

        Assert.Throws<ArgumentNullException>(
            () => CreateMinimalMapper().Map(
                source,
                null!));
    }

    private static StepAdvancedFaceMapper CreateMinimalMapper()
    {
        return new StepAdvancedFaceMapper(
            new StubFaceBuilderMapper(
                MappingResult<FaceBuilder>.Success(
                    new FaceBuilder(
                        new TestSurface3D()))),
            new StubFaceBoundMapper(
                new Dictionary<
                    string,
                    MappingResult<FaceLoopMapping>>()));
    }

    private static StepAdvancedFaceEntity CreateAdvancedFace(
        string sourceEntityId,
        params string[] boundReferenceIds)
    {
        return CreateAdvancedFace(
            sourceEntityId,
            true,
            boundReferenceIds);
    }

    private static StepAdvancedFaceEntity CreateAdvancedFace(
        string sourceEntityId,
        bool sameSense,
        params string[] boundReferenceIds)
    {
        return new StepAdvancedFaceEntity(
            sourceEntityId,
            "Advanced face",
            boundReferenceIds,
            "#500",
            sameSense);
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
            new[]
            {
                new EdgeUse(
                    firstEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    secondEdge,
                    EdgeUseDirection.Forward),

                new EdgeUse(
                    thirdEdge,
                    EdgeUseDirection.Forward)
            });
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

        public StepAdvancedFaceEntity? LastSource { get; private set; }

        public MappingContext? LastContext { get; private set; }

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
            LastSource =
                source;

            LastContext =
                context;

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

        private readonly List<string>
            _mappingOrder = new();

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

        public IReadOnlyList<string> MappingOrder =>
            _mappingOrder;

        public MappingResult<FaceLoopMapping> Map(
            StepFaceBoundEntity source,
            MappingContext context)
        {
            string sourceEntityId =
                source.SourceEntityId
                ?? throw new InvalidOperationException(
                    "The test FACE_BOUND entity must contain a source entity ID.");

            _mappingOrder.Add(
                sourceEntityId);

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

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failedReferences;

        private readonly List<string>
            _resolutionOrder = new();

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepFaceBoundEntity> faceBounds,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic>? failedReferences = null)
        {
            _faceBounds =
                faceBounds
                ?? throw new ArgumentNullException(
                    nameof(faceBounds));

            _failedReferences =
                failedReferences
                ?? new Dictionary<
                    string,
                    MappingDiagnostic>();
        }

        public IReadOnlyList<string> ResolutionOrder =>
            _resolutionOrder;

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

            _resolutionOrder.Add(
                referenceId);

            if (_failedReferences.TryGetValue(
                    referenceId,
                    out MappingDiagnostic? diagnostic))
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    new[]
                    {
                        diagnostic
                    });
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