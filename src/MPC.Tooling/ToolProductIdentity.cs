namespace MPC.Tooling;

public sealed record ToolProductIdentity
{
    public string Manufacturer { get; }

    public string ManufacturerProductId { get; }

    public string OrderCode { get; }

    public string? Ean { get; }

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
        Ean = string.IsNullOrWhiteSpace(ean) ? null : ean.Trim();
    }
}
