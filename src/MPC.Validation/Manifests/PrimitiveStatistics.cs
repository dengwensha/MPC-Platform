namespace MPC.Validation.Manifests;

public sealed class PrimitiveStatistics
{
    private readonly IReadOnlyDictionary<string, int> _counts;

    public IReadOnlyDictionary<string, int> Counts => _counts;

    public int TotalCount => _counts.Values.Sum();

    public PrimitiveStatistics(
        IEnumerable<KeyValuePair<string, int>> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);

        var normalizedCounts =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var entry in counts)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                throw new ArgumentException(
                    "Primitive type cannot be null, empty, or whitespace.",
                    nameof(counts));
            }

            if (entry.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(counts),
                    entry.Value,
                    "Primitive count cannot be negative.");
            }

            var primitiveType = entry.Key.Trim();

            if (!normalizedCounts.TryAdd(
                    primitiveType,
                    entry.Value))
            {
                throw new ArgumentException(
                    $"Duplicate primitive type detected: " +
                    $"'{primitiveType}'.",
                    nameof(counts));
            }
        }

        _counts = normalizedCounts;
    }

    public int GetCount(string primitiveType)
    {
        if (string.IsNullOrWhiteSpace(primitiveType))
        {
            throw new ArgumentException(
                "Primitive type cannot be null, empty, or whitespace.",
                nameof(primitiveType));
        }

        return _counts.TryGetValue(
            primitiveType.Trim(),
            out var count)
                ? count
                : 0;
    }
}