namespace MPC.Connectors.Step.Semantics;

public sealed class StepDirection
    : StepSemanticEntity
{
    private readonly IReadOnlyList<double>
        _directionRatios;

    public string Name { get; }

    public IReadOnlyList<double> DirectionRatios =>
        _directionRatios;

    public StepDirection(
        string? sourceEntityId,
        string name,
        IEnumerable<double> directionRatios)
        : base(
            sourceEntityId,
            "DIRECTION")
    {
        if (name is null)
        {
            throw new ArgumentNullException(
                nameof(name));
        }

        if (directionRatios is null)
        {
            throw new ArgumentNullException(
                nameof(directionRatios));
        }

        var ratioList =
            directionRatios.ToList();

        if (ratioList.Count == 0)
        {
            throw new ArgumentException(
                "STEP direction must contain at least one direction ratio.",
                nameof(directionRatios));
        }

        if (ratioList.Any(
                ratio =>
                    double.IsNaN(ratio) ||
                    double.IsInfinity(ratio)))
        {
            throw new ArgumentException(
                "STEP direction ratios must be finite.",
                nameof(directionRatios));
        }

        if (ratioList.All(
                ratio =>
                    ratio == 0.0))
        {
            throw new ArgumentException(
                "STEP direction ratios cannot all be zero.",
                nameof(directionRatios));
        }

        Name =
            name;

        _directionRatios =
            ratioList.AsReadOnly();
    }
}