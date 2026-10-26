namespace MPC.Tooling;

public enum GtinValidationStatus
{
    NotProvided = 0,
    NeedsReview = 1,
    ValidFormat = 2
}

public sealed record ToolProductIdentity
{
    public string Manufacturer { get; }

    public string ManufacturerProductId { get; }

    public string OrderCode { get; }

    // Exact source text, including whitespace. Never used as a global product key.
    public string? Ean { get; }

    public GtinValidationStatus GtinStatus { get; }

    // Only length, ASCII digits and check digit are validated, not product/source identity.
    public string? FormatValidatedGtin { get; }

    public ToolProductIdentity(
        string manufacturer,
        string manufacturerProductId,
        string orderCode,
        string? ean = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manufacturer);
        ArgumentException.ThrowIfNullOrWhiteSpace(manufacturerProductId);
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);

        Manufacturer = manufacturer.Trim();
        ManufacturerProductId = manufacturerProductId.Trim();
        OrderCode = orderCode.Trim();
        Ean = ean;
        if (ean is null)
        {
            GtinStatus = GtinValidationStatus.NotProvided;
        }
        else
        {
            string candidate = ean.Trim();
            bool valid = HasValidGtinFormat(candidate);
            GtinStatus = valid ? GtinValidationStatus.ValidFormat : GtinValidationStatus.NeedsReview;
            FormatValidatedGtin = valid ? candidate : null;
        }
    }
    private static bool HasValidGtinFormat(string value)
    {
        if (value.Length is not (8 or 12 or 13 or 14) ||
            value.Any(character => character < '0' || character > '9'))
        {
            return false;
        }

        int sum = 0;
        int weight = 3;
        for (int index = value.Length - 2; index >= 0; index--)
        {
            sum += (value[index] - '0') * weight;
            weight = weight == 3 ? 1 : 3;
        }

        return (10 - sum % 10) % 10 == value[^1] - '0';
    }
}
