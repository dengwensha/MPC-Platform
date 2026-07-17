namespace MPC.Connectors.Snapshots.Source;

public sealed class CadSourceInfo
{
    public string SourceSystem { get; }

    public string SourceFormat { get; }

    public string SourceFileName { get; }

    public string? SourceVersion { get; }

    public string ConnectorVersion { get; }

    public CadSourceInfo(
        string sourceSystem,
        string sourceFormat,
        string sourceFileName,
        string? sourceVersion,
        string connectorVersion)
    {
        if (string.IsNullOrWhiteSpace(sourceSystem))
        {
            throw new ArgumentException(
                "Source system cannot be null, empty, or whitespace.",
                nameof(sourceSystem));
        }

        if (string.IsNullOrWhiteSpace(sourceFormat))
        {
            throw new ArgumentException(
                "Source format cannot be null, empty, or whitespace.",
                nameof(sourceFormat));
        }

        if (string.IsNullOrWhiteSpace(sourceFileName))
        {
            throw new ArgumentException(
                "Source file name cannot be null, empty, or whitespace.",
                nameof(sourceFileName));
        }

        if (string.IsNullOrWhiteSpace(connectorVersion))
        {
            throw new ArgumentException(
                "Connector version cannot be null, empty, or whitespace.",
                nameof(connectorVersion));
        }

        SourceSystem = sourceSystem.Trim();
        SourceFormat = sourceFormat.Trim();
        SourceFileName = sourceFileName.Trim();
        SourceVersion = string.IsNullOrWhiteSpace(sourceVersion)
            ? null
            : sourceVersion.Trim();
        ConnectorVersion = connectorVersion.Trim();
    }
}