namespace MPC.Contracts;

public static class ContractVersion
{
    public const string Current = "1.0";

    public static bool IsSupported(string? version)
    {
        return string.Equals(
            version?.Trim(),
            Current,
            System.StringComparison.Ordinal);
    }
}
