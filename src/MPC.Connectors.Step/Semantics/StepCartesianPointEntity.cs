namespace MPC.Connectors.Step.Semantics;

public sealed class StepCartesianPointEntity
    : StepSemanticEntity
{
    private readonly IReadOnlyList<double>
        _coordinates;

    public string Name { get; }

    public IReadOnlyList<double> Coordinates =>
        _coordinates;

    public StepCartesianPointEntity(
        string? sourceEntityId,
        string name,
        IEnumerable<double> coordinates)
        : base(
            sourceEntityId,
            "CARTESIAN_POINT")
    {
        Name =
            name ??
            throw new ArgumentNullException(
                nameof(name));

        if (coordinates is null)
        {
            throw new ArgumentNullException(
                nameof(coordinates));
        }

        var coordinateList =
            coordinates.ToList();

        if (coordinateList.Count == 0)
        {
            throw new ArgumentException(
                "STEP Cartesian point must contain at least one coordinate.",
                nameof(coordinates));
        }

        if (coordinateList.Any(
                coordinate =>
                    !double.IsFinite(
                        coordinate)))
        {
            throw new ArgumentException(
                "STEP Cartesian point coordinates must be finite.",
                nameof(coordinates));
        }

        _coordinates =
            coordinateList.AsReadOnly();
    }
}