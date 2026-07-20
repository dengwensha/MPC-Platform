namespace MPC.Connectors.Step.Semantics;

public sealed class StepDirectionEntity
    : StepSemanticEntity
{
    private readonly IReadOnlyList<double>
        _ratios;

    public string Name { get; }

    public IReadOnlyList<double> Ratios =>
        _ratios;

    public StepDirectionEntity(
        string? sourceEntityId,
        string name,
        IEnumerable<double> ratios)
        : base(
            sourceEntityId,
            "DIRECTION")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (ratios is null)
        {
            throw new ArgumentNullException(
                nameof(ratios));
        }

        var ratioList =
            ratios.ToList();

        if (ratioList.Count == 0)
        {
            throw new ArgumentException(
                "STEP direction must contain at least one direction ratio.",
                nameof(ratios));
        }

        if (ratioList.Any(
                ratio =>
                    !double.IsFinite(
                        ratio)))
        {
            throw new ArgumentException(
                "STEP direction ratios must be finite.",
                nameof(ratios));
        }

        _ratios =
            ratioList.AsReadOnly();
    }
}