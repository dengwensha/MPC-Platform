namespace MPC.Connectors.Step.Semantics;

public sealed class StepManifoldSolidBrepEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string OuterShellReferenceId { get; }

    public StepManifoldSolidBrepEntity(
        string? sourceEntityId,
        string name,
        string outerShellReferenceId)
        : base(
            sourceEntityId,
            "MANIFOLD_SOLID_BREP")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        if (string.IsNullOrWhiteSpace(
                outerShellReferenceId))
        {
            throw new ArgumentException(
                "MANIFOLD_SOLID_BREP outer shell reference ID cannot be null, empty, or whitespace.",
                nameof(outerShellReferenceId));
        }

        Name =
            name;

        OuterShellReferenceId =
            outerShellReferenceId;
    }
}