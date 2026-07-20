namespace MPC.Connectors.Step.Models;

public sealed class StepAggregateParameter
    : StepParameter
{
    private readonly IReadOnlyList<StepParameter>
        _items;

    public IReadOnlyList<StepParameter> Items =>
        _items;

    public StepAggregateParameter(
        string rawText,
        IEnumerable<StepParameter> items)
        : base(
            rawText)
    {
        if (items is null)
        {
            throw new ArgumentNullException(
                nameof(items));
        }

        var itemList =
            items.ToList();

        if (itemList.Any(
                item =>
                    item is null))
        {
            throw new ArgumentException(
                "STEP aggregate items cannot contain null entries.",
                nameof(items));
        }

        _items =
            itemList.AsReadOnly();
    }
}