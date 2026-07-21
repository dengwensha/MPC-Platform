namespace MPC.Connectors.Step.Semantics;

public sealed class StepCartesianPoint
    : StepSemanticEntity
{
    private readonly IReadOnlyList<double>
        _coordinates;

    public string Name { get; }

    public IReadOnlyList<double> Coordinates =>
        _coordinates;

    public StepCartesianPoint(
        string? sourceEntityId,
        string name,
        IEnumerable<double> coordinates)
        : base(
            sourceEntityId,
            "CARTESIAN_POINT")
    {
        if (name is null)
        {
            throw new ArgumentNullException(
                nameof(name));
        }

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
                    double.IsNaN(coordinate) ||
                    double.IsInfinity(coordinate)))
        {
            throw new ArgumentException(
                "STEP Cartesian point coordinates must be finite.",
                nameof(coordinates));
        }

        Name =
            name;

        _coordinates =
            coordinateList.AsReadOnly();
    }
}