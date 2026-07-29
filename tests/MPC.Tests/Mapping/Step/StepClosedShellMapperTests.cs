using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Tests.Mapping.Step.TestInfrastructure;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepClosedShellMapperTests
{
    [Fact]
    public void Constructor_WithNullFaceMapper_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepClosedShellMapper(null!));
    }

    [Fact]
    public void Map_WithSingleFace_ReturnsCanonicalShell()
    {
        var sourceFace =
            new StepAdvancedFaceEntity(
                "#20",
                "Advanced face",
                new[]
                {
                    "#30"
                },
                "#40",
                true);

        Face canonicalFace =
            TopologyTestFactory.CreateFace();

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepAdvancedFaceEntity>
                {
                    ["#20"] = sourceFace
                });

        var faceMapper =
            new StubAdvancedFaceMapper(
                new Dictionary<string, MappingResult<Face>>
                {
                    ["#20"] =
                        MappingResult<Face>.Success(
                            canonicalFace)
                });

        var mapper =
            new StepClosedShellMapper(
                faceMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepClosedShellEntity(
                "#100",
                "Closed shell",
                new[]
                {
                    "#20"
                });

        MappingResult<Shell> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Shell shell =
            Assert.IsType<Shell>(
                result.Value);

        Face shellFace =
            Assert.Single(
                shell.Faces);

        Assert.Same(
            canonicalFace,
            shellFace);

        Assert.Empty(
            result.Diagnostics);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            resolver.ResolvedReferenceIds);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            faceMapper.MappedSourceEntityIds);
    }

    [Fact]
    public void Map_WithMultipleFaces_PreservesOrder()
    {
        var firstSourceFace =
            new StepAdvancedFaceEntity(
                "#20",
                "First advanced face",
                new[]
                {
                    "#30"
                },
                "#40",
                true);

        var secondSourceFace =
            new StepAdvancedFaceEntity(
                "#21",
                "Second advanced face",
                new[]
                {
                    "#31"
                },
                "#41",
                true);

        var thirdSourceFace =
            new StepAdvancedFaceEntity(
                "#22",
                "Third advanced face",
                new[]
                {
                    "#32"
                },
                "#42",
                true);

        Face firstCanonicalFace =
            TopologyTestFactory.CreateFace(
                offset: 0.0);

        Face secondCanonicalFace =
            TopologyTestFactory.CreateFace(
                offset: 10.0);

        Face thirdCanonicalFace =
            TopologyTestFactory.CreateFace(
                offset: 20.0);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepAdvancedFaceEntity>
                {
                    ["#20"] = firstSourceFace,
                    ["#21"] = secondSourceFace,
                    ["#22"] = thirdSourceFace
                });

        var faceMapper =
            new StubAdvancedFaceMapper(
                new Dictionary<string, MappingResult<Face>>
                {
                    ["#20"] =
                        MappingResult<Face>.Success(
                            firstCanonicalFace),

                    ["#21"] =
                        MappingResult<Face>.Success(
                            secondCanonicalFace),

                    ["#22"] =
                        MappingResult<Face>.Success(
                            thirdCanonicalFace)
                });

        var mapper =
            new StepClosedShellMapper(
                faceMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepClosedShellEntity(
                "#100",
                "Closed shell",
                new[]
                {
                    "#20",
                    "#21",
                    "#22"
                });

        MappingResult<Shell> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Shell shell =
            Assert.IsType<Shell>(
                result.Value);

        Assert.Collection(
            shell.Faces,
            face =>
                Assert.Same(
                    firstCanonicalFace,
                    face),
            face =>
                Assert.Same(
                    secondCanonicalFace,
                    face),
            face =>
                Assert.Same(
                    thirdCanonicalFace,
                    face));

        Assert.Empty(
            result.Diagnostics);

        Assert.Equal(
            new[]
            {
                "#20",
                "#21",
                "#22"
            },
            resolver.ResolvedReferenceIds);

        Assert.Equal(
            new[]
            {
                "#20",
                "#21",
                "#22"
            },
            faceMapper.MappedSourceEntityIds);
    }

    [Fact]
    public void Map_WithUnresolvedFaceReference_ReturnsFailure()
    {
        const string unresolvedDiagnosticCode =
            "TEST-REFERENCE-001";

        var resolvedSourceFace =
            new StepAdvancedFaceEntity(
                "#20",
                "Resolved advanced face",
                new[]
                {
                    "#30"
                },
                "#40",
                true);

        Face canonicalFace =
            TopologyTestFactory.CreateFace();

        var unresolvedDiagnostic =
            new MappingDiagnostic(
                unresolvedDiagnosticCode,
                "STEP reference '#21' could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepAdvancedFaceEntity>
                {
                    ["#20"] = resolvedSourceFace
                },
                new Dictionary<string, MappingDiagnostic>
                {
                    ["#21"] = unresolvedDiagnostic
                });

        var faceMapper =
            new StubAdvancedFaceMapper(
                new Dictionary<string, MappingResult<Face>>
                {
                    ["#20"] =
                        MappingResult<Face>.Success(
                            canonicalFace)
                });

        var mapper =
            new StepClosedShellMapper(
                faceMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepClosedShellEntity(
                "#100",
                "Closed shell",
                new[]
                {
                    "#20",
                    "#21"
                });

        MappingResult<Shell> result =
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

        Assert.Same(
            unresolvedDiagnostic,
            diagnostic);

        Assert.Equal(
            unresolvedDiagnosticCode,
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            unresolvedDiagnostic,
            contextDiagnostic);

        Assert.Equal(
            new[]
            {
                "#20",
                "#21"
            },
            resolver.ResolvedReferenceIds);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            faceMapper.MappedSourceEntityIds);
    }

    [Fact]
    public void Map_WithDuplicateFaceReference_ReturnsFailure()
    {
        const string duplicateDiagnosticCode =
            "MAP-STEP-CLOSED-SHELL-001";

        var sourceFace =
            new StepAdvancedFaceEntity(
                "#20",
                "Advanced face",
                new[]
                {
                    "#30"
                },
                "#40",
                true);

        Face canonicalFace =
            TopologyTestFactory.CreateFace();

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepAdvancedFaceEntity>
                {
                    ["#20"] = sourceFace
                });

        var faceMapper =
            new StubAdvancedFaceMapper(
                new Dictionary<string, MappingResult<Face>>
                {
                    ["#20"] =
                        MappingResult<Face>.Success(
                            canonicalFace)
                });

        var mapper =
            new StepClosedShellMapper(
                faceMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepClosedShellEntity(
                "#100",
                "Closed shell",
                new[]
                {
                    "#20",
                    "#20"
                });

        MappingResult<Shell> result =
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
            duplicateDiagnosticCode,
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Contains(
            "#20",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "occurs more than once",
            diagnostic.Message,
            StringComparison.Ordinal);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            resolver.ResolvedReferenceIds);

        Assert.Equal(
            new[]
            {
                "#20"
            },
            faceMapper.MappedSourceEntityIds);
    }

    [Fact]
    public void Map_WithDuplicateCanonicalFace_ReturnsFailure()
    {
        const string duplicateDiagnosticCode =
            "MAP-STEP-CLOSED-SHELL-002";

        var firstSourceFace =
            new StepAdvancedFaceEntity(
                "#20",
                "First advanced face",
                new[]
                {
                    "#30"
                },
                "#40",
                true);

        var secondSourceFace =
            new StepAdvancedFaceEntity(
                "#21",
                "Second advanced face",
                new[]
                {
                    "#31"
                },
                "#41",
                true);

        Face sharedCanonicalFace =
            TopologyTestFactory.CreateFace();

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, StepAdvancedFaceEntity>
                {
                    ["#20"] = firstSourceFace,
                    ["#21"] = secondSourceFace
                });

        var faceMapper =
            new StubAdvancedFaceMapper(
                new Dictionary<string, MappingResult<Face>>
                {
                    ["#20"] =
                        MappingResult<Face>.Success(
                            sharedCanonicalFace),

                    ["#21"] =
                        MappingResult<Face>.Success(
                            sharedCanonicalFace)
                });

        var mapper =
            new StepClosedShellMapper(
                faceMapper);

        var context =
            new MappingContext(
                resolver);

        var source =
            new StepClosedShellEntity(
                "#100",
                "Closed shell",
                new[]
                {
                    "#20",
                    "#21"
                });

        MappingResult<Shell> result =
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
            duplicateDiagnosticCode,
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Contains(
            "#21",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "already added",
            diagnostic.Message,
            StringComparison.Ordinal);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);

        Assert.Equal(
            new[]
            {
                "#20",
                "#21"
            },
            resolver.ResolvedReferenceIds);

        Assert.Equal(
            new[]
            {
                "#20",
                "#21"
            },
            faceMapper.MappedSourceEntityIds);
    }

    private sealed class StubAdvancedFaceMapper :
        IEntityMapper<StepAdvancedFaceEntity, Face>
    {
        private readonly IReadOnlyDictionary<
            string,
            MappingResult<Face>> _results;

        private readonly List<string> _mappedSourceEntityIds =
            new();

        public StubAdvancedFaceMapper(
            IReadOnlyDictionary<
                string,
                MappingResult<Face>> results)
        {
            _results =
                results
                ?? throw new ArgumentNullException(
                    nameof(results));
        }

        public IReadOnlyList<string> MappedSourceEntityIds =>
            _mappedSourceEntityIds;

        public MappingResult<Face> Map(
            StepAdvancedFaceEntity source,
            MappingContext context)
        {
            ArgumentNullException.ThrowIfNull(
                source);

            ArgumentNullException.ThrowIfNull(
                context);

            string sourceEntityId =
                source.SourceEntityId
                ?? throw new InvalidOperationException(
                    "The test ADVANCED_FACE entity must contain a source entity ID.");

            _mappedSourceEntityIds.Add(
                sourceEntityId);

            if (!_results.TryGetValue(
                    sourceEntityId,
                    out MappingResult<Face>? result))
            {
                throw new InvalidOperationException(
                    $"No mapping result was configured for '{sourceEntityId}'.");
            }

            return result;
        }
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly IReadOnlyDictionary<
            string,
            StepAdvancedFaceEntity> _faces;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failures;

        private readonly List<string> _resolvedReferenceIds =
            new();

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepAdvancedFaceEntity> faces)
            : this(
                faces,
                new Dictionary<string, MappingDiagnostic>())
        {
        }

        public StubReferenceResolver(
            IReadOnlyDictionary<
                string,
                StepAdvancedFaceEntity> faces,
            IReadOnlyDictionary<
                string,
                MappingDiagnostic> failures)
        {
            _faces =
                faces
                ?? throw new ArgumentNullException(
                    nameof(faces));

            _failures =
                failures
                ?? throw new ArgumentNullException(
                    nameof(failures));
        }

        public IReadOnlyList<string> ResolvedReferenceIds =>
            _resolvedReferenceIds;

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            _resolvedReferenceIds.Add(
                referenceId);

            if (_faces.TryGetValue(
                    referenceId,
                    out StepAdvancedFaceEntity? face))
            {
                return ReferenceResolutionResult<T>.Resolved(
                    referenceId,
                    Assert.IsAssignableFrom<T>(
                        face));
            }

            if (_failures.TryGetValue(
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

            throw new InvalidOperationException(
                $"No reference resolution result was configured for '{referenceId}'.");
        }
    }
}