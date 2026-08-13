using MPC.Connectors.Step.Semantics;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepManifoldSolidBrepMapper :
    IEntityMapper<
        StepManifoldSolidBrepEntity,
        Solid>
{
    private const string OpenOuterShellDiagnosticCode =
        "MAP-STEP-MANIFOLD-SOLID-BREP-001";

    private readonly IEntityMapper<
        StepClosedShellEntity,
        Shell> _closedShellMapper;

    public StepManifoldSolidBrepMapper(
        IEntityMapper<
            StepClosedShellEntity,
            Shell> closedShellMapper)
    {
        ArgumentNullException.ThrowIfNull(
            closedShellMapper);

        _closedShellMapper =
            closedShellMapper;
    }

    public MappingResult<Solid> Map(
        StepManifoldSolidBrepEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        int diagnosticCheckpoint =
            context.CaptureDiagnosticCheckpoint();

        ReferenceResolutionResult<
            StepClosedShellEntity> shellResolution =
                context.ReferenceResolver
                    .Resolve<StepClosedShellEntity>(
                        source.OuterShellReferenceId);

        if (!shellResolution.IsResolved)
        {
            context.AddDiagnostics(
                shellResolution.Diagnostics);

            return MappingResult<Solid>.Failure(
                shellResolution.Diagnostics);
        }

        StepClosedShellEntity closedShell =
            shellResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved CLOSED_SHELL reference must contain a semantic entity.");

        MappingResult<Shell> shellResult =
            _closedShellMapper.Map(
                closedShell,
                context);

        if (!shellResult.IsSuccessful ||
            shellResult.Value is null)
        {
            return MappingResult<Solid>.Failure(
                shellResult.Diagnostics);
        }

        Shell canonicalShell =
            shellResult.Value;

        if (!canonicalShell.IsClosed)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    source,
                    "The canonical outer shell is not topologically closed. " +
                    "Every canonical edge must be used exactly twice across the shell boundaries.");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Solid>.Failure(
                [
                    diagnostic
                ]);
        }

        var solid =
            new Solid(
                canonicalShell);

        IReadOnlyList<MappingDiagnostic> operationDiagnostics =
            context.GetDiagnosticsSince(
                diagnosticCheckpoint);

        if (operationDiagnostics.Count > 0)
        {
            return MappingResult<Solid>
                .SuccessWithDiagnostics(
                    solid,
                    operationDiagnostics);
        }

        return MappingResult<Solid>.Success(
            solid);
    }

    private static MappingDiagnostic CreateDiagnostic(
        StepManifoldSolidBrepEntity source,
        string message)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "MANIFOLD_SOLID_BREP"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            OpenOuterShellDiagnosticCode,
            $"STEP MANIFOLD_SOLID_BREP '{sourceIdentity}' could not be mapped. {message}",
            MappingDiagnosticSeverity.Error);
    }
}