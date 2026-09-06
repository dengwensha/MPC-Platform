using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Tests.Mapping.Step.TestInfrastructure;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepClosedShellDiagnosticPropagationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Map_WithOpenClosedShell_PropagatesShellOwnedDiagnostic(
        bool includePreExistingDiagnostic)
    {
        var sourceFace = new StepAdvancedFaceEntity(
            "#20",
            "Open face",
            new[] { "#30" },
            "#40",
            true);

        var sourceShell = new StepClosedShellEntity(
            "#100",
            "Declared closed shell",
            new[] { "#20" });

        var sourceSolid = new StepManifoldSolidBrepEntity(
            "#200",
            "Solid",
            "#100");

        Face canonicalFace = TopologyTestFactory.CreateFace();

        Assert.False(
            new Shell(new[] { canonicalFace }).IsClosed);

        var resolver = new StubReferenceResolver(
            sourceShell,
            sourceFace);

        var faceMapper = new StubAdvancedFaceMapper(
            sourceFace,
            canonicalFace);

        var mapper = new StepManifoldSolidBrepMapper(
            new StepClosedShellMapper(faceMapper));

        var context = new MappingContext(resolver);

        var priorDiagnostic = new MappingDiagnostic(
            "TEST-PRIOR-001",
            "Information from a previous operation.",
            MappingDiagnosticSeverity.Information);

        if (includePreExistingDiagnostic)
        {
            context.AddDiagnostic(priorDiagnostic);
        }

        MappingResult<Solid> result =
            mapper.Map(sourceSolid, context);

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-CLOSED-SHELL-004",
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Contains(
            "STEP CLOSED_SHELL '#100'",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "not topologically closed",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "#200",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            context.Diagnostics,
            item => item.Code == "MAP-STEP-MANIFOLD-SOLID-BREP-001");

        Assert.Equal(
            includePreExistingDiagnostic ? 2 : 1,
            context.Diagnostics.Count);

        Assert.Same(
            diagnostic,
            context.Diagnostics[context.Diagnostics.Count - 1]);

        if (includePreExistingDiagnostic)
        {
            Assert.Same(
                priorDiagnostic,
                context.Diagnostics[0]);

            Assert.DoesNotContain(
                result.Diagnostics,
                item => ReferenceEquals(item, priorDiagnostic));
        }

        Assert.Equal(
            new[] { "#100", "#20" },
            resolver.ResolutionOrder);

        Assert.Equal(1, faceMapper.CallCount);
        Assert.Same(context, faceMapper.LastContext);
    }

    private sealed class StubAdvancedFaceMapper :
        IEntityMapper<StepAdvancedFaceEntity, Face>
    {
        private readonly StepAdvancedFaceEntity _expectedSource;
        private readonly Face _canonicalFace;

        public int CallCount { get; private set; }

        public MappingContext? LastContext { get; private set; }

        public StubAdvancedFaceMapper(
            StepAdvancedFaceEntity expectedSource,
            Face canonicalFace)
        {
            _expectedSource = expectedSource;
            _canonicalFace = canonicalFace;
        }

        public MappingResult<Face> Map(
            StepAdvancedFaceEntity source,
            MappingContext context)
        {
            Assert.Same(_expectedSource, source);

            CallCount++;
            LastContext = context;

            return MappingResult<Face>.Success(_canonicalFace);
        }
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly StepClosedShellEntity _shell;
        private readonly StepAdvancedFaceEntity _face;

        private readonly List<string> _resolutionOrder = new();

        public IReadOnlyList<string> ResolutionOrder =>
            _resolutionOrder;

        public StubReferenceResolver(
            StepClosedShellEntity shell,
            StepAdvancedFaceEntity face)
        {
            _shell = shell;
            _face = face;
        }

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            _resolutionOrder.Add(referenceId);

            if (typeof(T) == typeof(StepClosedShellEntity))
            {
                Assert.Equal("#100", referenceId);

                return ReferenceResolutionResult<T>.Resolved(
                    referenceId,
                    Assert.IsAssignableFrom<T>(_shell));
            }

            if (typeof(T) == typeof(StepAdvancedFaceEntity))
            {
                Assert.Equal("#20", referenceId);

                return ReferenceResolutionResult<T>.Resolved(
                    referenceId,
                    Assert.IsAssignableFrom<T>(_face));
            }

            throw new NotSupportedException(
                $"Unexpected reference type: {typeof(T).Name}");
        }
    }
}
