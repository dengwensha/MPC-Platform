using MPC.Connectors.Step.Semantics;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepManifoldSolidBrepMapper :
    IEntityMapper<
        StepManifoldSolidBrepEntity,
        Solid>
{
    private readonly IEntityMapper<
        StepClosedShellEntity,
        Shell> _closedShellMapper;

    public StepManifoldSolidBrepMapper()
        : this(
            new StepClosedShellMapper())
    {
    }

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

        var solid =
            new Solid(
                shellResult.Value);

        if (context.HasDiagnostics)
        {
            return MappingResult<Solid>
                .SuccessWithDiagnostics(
                    solid,
                    context.Diagnostics);
        }

        return MappingResult<Solid>.Success(
            solid);
    }
}