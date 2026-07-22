namespace MPC.Connectors.Step.Semantics;

public sealed class StepVertexPointEntity
    : StepSemanticEntity
{
    public string Name { get; }

    public string VertexGeometryReferenceId { get; }

    public StepVertexPointEntity(
        string? sourceEntityId,
        string name,
        string vertexGeometryReferenceId)
        : base(
            sourceEntityId,
            "VERTEX_POINT")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        if (string.IsNullOrWhiteSpace(
                vertexGeometryReferenceId))
        {
            throw new ArgumentException(
                "Vertex geometry reference ID cannot be null, empty, or whitespace.",
                nameof(vertexGeometryReferenceId));
        }

        Name =
            name;

        VertexGeometryReferenceId =
            vertexGeometryReferenceId;
    }
}