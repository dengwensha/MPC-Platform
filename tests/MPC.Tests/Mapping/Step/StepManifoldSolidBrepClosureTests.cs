using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Tests.Mapping.Step.TestInfrastructure;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepManifoldSolidBrepClosureTests
{
    [Fact]
    public void Map_WithOpenOuterShell_ReturnsClosureDiagnostic()
    {
        StepClosedShellEntity closedShellEntity =
            CreateClosedShellEntity();

        Shell openShell =
            new(
                [
                    TopologyTestFactory.CreateFace()
                ]);

        Assert.False(
            openShell.IsClosed);

        var resolver =
            new StubReferenceResolver(
                closedShellEntity);

        var shellMapper =
            new StubClosedShellMapper(
                MappingResult<Shell>.Success(
                    openShell));

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

        Assert.Equal(
            "MAP-STEP-MANIFOLD-SOLID-BREP-001",
            resultDiagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            resultDiagnostic.Severity);

        Assert.Contains(
            "#100",
            resultDiagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "not topologically closed",
            resultDiagnostic.Message,
            StringComparison.Ordinal);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            resultDiagnostic,
            contextDiagnostic);

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
    }

    [Fact]
    public void Map_WithOpenOuterShellAndPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        StepClosedShellEntity closedShellEntity =
            CreateClosedShellEntity();

        Shell openShell =
            new(
                [
                    TopologyTestFactory.CreateFace()
                ]);

        var mapper =
            new StepManifoldSolidBrepMapper(
                new StubClosedShellMapper(
                    MappingResult<Shell>.Success(
                        openShell)));

        var context =
            new MappingContext(
                new StubReferenceResolver(
                    closedShellEntity));

        var preExistingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-001",
                "A previous mapping operation produced information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            preExistingDiagnostic);

        MappingResult<Solid> result =
            mapper.Map(
                CreateManifoldSolidBrepEntity(),
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-MANIFOLD-SOLID-BREP-001",
            resultDiagnostic.Code);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            preExistingDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            resultDiagnostic,
            context.Diagnostics[1]);
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
        private readonly StepClosedShellEntity
            _closedShell;

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

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                Assert.IsAssignableFrom<T>(
                    _closedShell));
        }
    }
}