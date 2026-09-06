using MPC.Connectors.Step.Semantics;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Tests.Mapping.Step.TestInfrastructure;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepClosedShellClosureOwnershipTests
{
    private const string OpenShellDiagnosticCode =
        "MAP-STEP-CLOSED-SHELL-004";

    [Fact]
    public void Map_WithOpenCanonicalShell_ReturnsClosedShellDiagnostic()
    {
        (
            StepClosedShellMapper mapper,
            MappingContext context,
            StepClosedShellEntity source) =
                CreateSystemUnderTest();

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

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            OpenShellDiagnosticCode,
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
    }

    [Fact]
    public void Map_WithOpenCanonicalShellAndPreExistingDiagnostic_ReturnsOnlyCurrentFailure()
    {
        (
            StepClosedShellMapper mapper,
            MappingContext context,
            StepClosedShellEntity source) =
                CreateSystemUnderTest();

        var preExistingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-001",
                "A previous mapping operation produced information.",
                MappingDiagnosticSeverity.Information);

        context.AddDiagnostic(
            preExistingDiagnostic);

        MappingResult<Shell> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            OpenShellDiagnosticCode,
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

    private static (
        StepClosedShellMapper Mapper,
        MappingContext Context,
        StepClosedShellEntity Source)
        CreateSystemUnderTest()
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

        Face openCanonicalFace =
            TopologyTestFactory.CreateFace();

        var openShell =
            new Shell(
                new[]
                {
                    openCanonicalFace
                });

        Assert.False(
            openShell.IsClosed);

        var mapper =
            new StepClosedShellMapper(
                new StubAdvancedFaceMapper(
                    sourceFace,
                    openCanonicalFace));

        var context =
            new MappingContext(
                new StubReferenceResolver(
                    sourceFace));

        var source =
            new StepClosedShellEntity(
                "#100",
                "Closed shell",
                new[]
                {
                    "#20"
                });

        return (
            mapper,
            context,
            source);
    }

    private sealed class StubAdvancedFaceMapper :
        IEntityMapper<
            StepAdvancedFaceEntity,
            Face>
    {
        private readonly StepAdvancedFaceEntity
            _expectedSource;

        private readonly Face
            _canonicalFace;

        public StubAdvancedFaceMapper(
            StepAdvancedFaceEntity expectedSource,
            Face canonicalFace)
        {
            _expectedSource =
                expectedSource
                ?? throw new ArgumentNullException(
                    nameof(expectedSource));

            _canonicalFace =
                canonicalFace
                ?? throw new ArgumentNullException(
                    nameof(canonicalFace));
        }

        public MappingResult<Face> Map(
            StepAdvancedFaceEntity source,
            MappingContext context)
        {
            ArgumentNullException.ThrowIfNull(
                source);

            ArgumentNullException.ThrowIfNull(
                context);

            Assert.Same(
                _expectedSource,
                source);

            return MappingResult<Face>.Success(
                _canonicalFace);
        }
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly StepAdvancedFaceEntity
            _sourceFace;

        public StubReferenceResolver(
            StepAdvancedFaceEntity sourceFace)
        {
            _sourceFace =
                sourceFace
                ?? throw new ArgumentNullException(
                    nameof(sourceFace));
        }

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            Assert.Equal(
                "#20",
                referenceId);

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                Assert.IsAssignableFrom<T>(
                    _sourceFace));
        }
    }
}