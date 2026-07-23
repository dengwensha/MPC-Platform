namespace MPC.Connectors.Step.Semantics;

public sealed class StepClosedShellEntity
    : StepSemanticEntity
{
    private readonly IReadOnlyList<string>
        _faceReferenceIds;

    public string Name { get; }

    public IReadOnlyList<string> FaceReferenceIds =>
        _faceReferenceIds;

    public StepClosedShellEntity(
        string? sourceEntityId,
        string name,
        IEnumerable<string> faceReferenceIds)
        : base(
            sourceEntityId,
            "CLOSED_SHELL")
    {
        ArgumentNullException.ThrowIfNull(
            name);

        ArgumentNullException.ThrowIfNull(
            faceReferenceIds);

        var referenceIds =
            faceReferenceIds.ToList();

        if (referenceIds.Count == 0)
        {
            throw new ArgumentException(
                "CLOSED_SHELL must contain at least one face reference.",
                nameof(faceReferenceIds));
        }

        if (referenceIds.Any(
                referenceId =>
                    string.IsNullOrWhiteSpace(
                        referenceId)))
        {
            throw new ArgumentException(
                "CLOSED_SHELL face reference IDs cannot contain null, empty, or whitespace values.",
                nameof(faceReferenceIds));
        }

        Name =
            name;

        _faceReferenceIds =
            referenceIds.AsReadOnly();
    }
}