using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Tests.Mapping.Step.TestInfrastructure;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepManifoldSolidBrepMapperTests
{
    [Fact]
    public void Constructor_WithNullClosedShellMapper_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepManifoldSolidBrepMapper(
                null!));
    }

    [Fact]
    public void Map_WithValidOuterShell_ReturnsCanonicalSolid()
    {
        StepClosedShellEntity closedShellEntity =
            CreateClosedShellEntity();

        Shell canonicalShell =
            CreateCanonicalShell();

        var resolver =
            new StubReferenceResolver(
                closedShellEntity);

        var shellMapper =
            new StubClosedShellMapper(
                MappingResult<Shell>.Success(
                    canonicalShell));

        var mapper =
            new StepManifoldSolidBrepMapper(
                shellMapper);

        var context =
            new MappingContext(
                resolver);

        StepManifoldSolidBrepEntity source =
            CreateManifoldSolidBrepEntity();

        MappingResult<Solid> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Solid solid =
            Assert.IsType<Solid>(
                result.Value);

        Assert.Same(
            canonicalShell,
            solid.OuterShell);

        Assert.Empty(
            solid.InnerShells);

        Assert.Same(
            closedShellEntity,
            shellMapper.LastSource);

        Assert.Same(
            context,
            shellMapper.LastContext);

        Assert.Equal(
            [
                "#20"
            ],
            resolver.ResolutionOrder);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WhenOuterShellReferenceCannotBeResolved_ReturnsFailure()
    {
        var diagnostic =
            new MappingDiagnostic(
                "TEST-CLOSED-SHELL-REFERENCE-001",
                "STEP CLOSED_SHELL reference '#20' could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                diagnostic);

        var shellMapper =
            new StubClosedShellMapper(
                MappingResult<Shell>.Success(
                    CreateCanonicalShell()));

        var mapper =
            new StepManifoldSolidBrepMapper(
                shellMapper);

        var context =
            new MappingContext(
                resolver);

        StepManifoldSolidBrepEntity source =
            CreateManifoldSolidBrepEntity();

        MappingResult<Solid> result =
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

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Same(
            diagnostic,
            resultDiagnostic);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);

        Assert.Null(
            shellMapper.LastSource);

        Assert.Null(
            shellMapper.LastContext);

        Assert.Equal(
            [
                "#20"
            ],
            resolver.ResolutionOrder);
    }

    [Fact]
    public void Map_WhenClosedShellMappingFails_PropagatesFailure()
    {
        StepClosedShellEntity closedShellEntity =
            CreateClosedShellEntity();

        var diagnostic =
            new MappingDiagnostic(
                "TEST-CLOSED-SHELL-MAPPING-001",
                "STEP CLOSED_SHELL mapping failed.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                closedShellEntity);

        var shellMapper =
            new StubClosedShellMapper(
                MappingResult<Shell>.Failure(
                    [
                        diagnostic
                    ]));

        var mapper =
            new StepManifoldSolidBrepMapper(
                shellMapper);

        var context =
            new MappingContext(
                resolver);

        StepManifoldSolidBrepEntity source =
            CreateManifoldSolidBrepEntity();

        MappingResult<Solid> result =
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

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Same(
            diagnostic,
            resultDiagnostic);

        Assert.Same(
            closedShellEntity,
            shellMapper.LastSource);

        Assert.Same(
            context,
            shellMapper.LastContext);

        Assert.Equal(
            [
                "#20"
            ],
            resolver.ResolutionOrder);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithExistingContextDiagnostic_ReturnsSuccessWithDiagnostics()
    {
        StepClosedShellEntity closedShellEntity =
            CreateClosedShellEntity();

        Shell canonicalShell =
            CreateCanonicalShell();

        var resolver =
            new StubReferenceResolver(
                closedShellEntity);

        var shellMapper =
            new StubClosedShellMapper(
                MappingResult<Shell>.Success(
                    canonicalShell));

        var mapper =
            new StepManifoldSolidBrepMapper(
                shellMapper);

        var context =
            new MappingContext(
                resolver);

        var diagnostic =
            new MappingDiagnostic(
                "TEST-MAPPING-INFORMATION-001",
                "Existing mapping information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            diagnostic);

        StepManifoldSolidBrepEntity source =
            CreateManifoldSolidBrepEntity();

        MappingResult<Solid> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Solid solid =
            Assert.IsType<Solid>(
                result.Value);

        Assert.Same(
            canonicalShell,
            solid.OuterShell);

        Assert.Empty(
            solid.InnerShells);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Same(
            diagnostic,
            resultDiagnostic);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            contextDiagnostic);

        Assert.Same(
            closedShellEntity,
            shellMapper.LastSource);

        Assert.Same(
            context,
            shellMapper.LastContext);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        StepClosedShellEntity closedShellEntity =
            CreateClosedShellEntity();

        var mapper =
            new StepManifoldSolidBrepMapper(
                new StubClosedShellMapper(
                    MappingResult<Shell>.Success(
                        CreateCanonicalShell())));

        var context =
            new MappingContext(
                new StubReferenceResolver(
                    closedShellEntity));

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var mapper =
            new StepManifoldSolidBrepMapper(
                new StubClosedShellMapper(
                    MappingResult<Shell>.Success(
                        CreateCanonicalShell())));

        StepManifoldSolidBrepEntity source =
            CreateManifoldSolidBrepEntity();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static StepClosedShellEntity CreateClosedShellEntity()
    {
        return new StepClosedShellEntity(
            "#20",
            "Outer shell",
            [
                "#30"
            ]);
    }

    private static StepManifoldSolidBrepEntity
        CreateManifoldSolidBrepEntity()
    {
        return new StepManifoldSolidBrepEntity(
            "#100",
            "Manifold solid",
            "#20");
    }

    private static Shell CreateCanonicalShell()
    {
        Face canonicalFace =
            TopologyTestFactory.CreateFace();

        return new Shell(
            [
                canonicalFace
            ]);
    }

    private sealed class StubClosedShellMapper :
        IEntityMapper<
            StepClosedShellEntity,
            Shell>
    {
        private readonly MappingResult<Shell>
            _result;

        public StubClosedShellMapper(
            MappingResult<Shell> result)
        {
            _result =
                result
                ?? throw new ArgumentNullException(
                    nameof(result));
        }

        public StepClosedShellEntity? LastSource
        {
            get;
            private set;
        }

        public MappingContext? LastContext
        {
            get;
            private set;
        }

        public MappingResult<Shell> Map(
            StepClosedShellEntity source,
            MappingContext context)
        {
            ArgumentNullException.ThrowIfNull(
                source);

            ArgumentNullException.ThrowIfNull(
                context);

            LastSource =
                source;

            LastContext =
                context;

            return _result;
        }
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly StepClosedShellEntity?
            _closedShell;

        private readonly MappingDiagnostic?
            _failureDiagnostic;

        private readonly List<string>
            _resolutionOrder = [];

        public StubReferenceResolver(
            StepClosedShellEntity closedShell)
        {
            _closedShell =
                closedShell
                ?? throw new ArgumentNullException(
                    nameof(closedShell));
        }

        public StubReferenceResolver(
            MappingDiagnostic failureDiagnostic)
        {
            _failureDiagnostic =
                failureDiagnostic
                ?? throw new ArgumentNullException(
                    nameof(failureDiagnostic));
        }

        public IReadOnlyList<string> ResolutionOrder =>
            _resolutionOrder;

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (typeof(T) !=
                typeof(StepClosedShellEntity))
            {
                throw new NotSupportedException(
                    $"Resolution of '{typeof(T).Name}' is not supported by this test resolver.");
            }

            _resolutionOrder.Add(
                referenceId);

            if (_failureDiagnostic is not null)
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    [
                        _failureDiagnostic
                    ]);
            }

            StepClosedShellEntity closedShell =
                _closedShell
                ?? throw new InvalidOperationException(
                    "No successful CLOSED_SHELL resolution was configured.");

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                Assert.IsAssignableFrom<T>(
                    closedShell));
        }
    }
}