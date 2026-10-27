namespace MPC.Contracts;

public static class ContractVersion
{
    // Legacy AnalyzeModel wire version remains unchanged.
    public const string Current = "1.0";

    public const string RegionSourceValidation = "1.1";

    public static bool IsSupported(string? version)
    {
        return string.Equals(
            version?.Trim(),
            Current,
            System.StringComparison.Ordinal);
    }
}
