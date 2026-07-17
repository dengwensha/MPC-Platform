namespace MPC.Connectors.Step.Models;

public sealed class StepHeaderInfo
{
    private readonly IReadOnlyList<string> _descriptions;
    private readonly IReadOnlyList<string> _schemas;

    public IReadOnlyList<string> Descriptions => _descriptions;

    public string? ImplementationLevel { get; }

    public string? FileName { get; }

    public string? TimeStamp { get; }

    public string? Author { get; }

    public string? Organization { get; }

    public string? PreprocessorVersion { get; }

    public string? OriginatingSystem { get; }

    public string? Authorization { get; }

    public IReadOnlyList<string> Schemas => _schemas;

    public StepHeaderInfo(
        IEnumerable<string> descriptions,
        string? implementationLevel,
        string? fileName,
        string? timeStamp,
        string? author,
        string? organization,
        string? preprocessorVersion,
        string? originatingSystem,
        string? authorization,
        IEnumerable<string> schemas)
    {
        ArgumentNullException.ThrowIfNull(descriptions);
        ArgumentNullException.ThrowIfNull(schemas);

        var descriptionList = descriptions
            .Select(NormalizeRequired)
            .ToList();

        var schemaList = schemas
            .Select(NormalizeRequired)
            .ToList();

        if (descriptionList.Count == 0)
        {
            throw new ArgumentException(
                "STEP header must contain at least one description.",
                nameof(descriptions));
        }

        if (schemaList.Count == 0)
        {
            throw new ArgumentException(
                "STEP header must contain at least one schema.",
                nameof(schemas));
        }

        _descriptions = descriptionList.AsReadOnly();
        _schemas = schemaList.AsReadOnly();

        ImplementationLevel = NormalizeOptional(implementationLevel);
        FileName = NormalizeOptional(fileName);
        TimeStamp = NormalizeOptional(timeStamp);
        Author = NormalizeOptional(author);
        Organization = NormalizeOptional(organization);
        PreprocessorVersion = NormalizeOptional(preprocessorVersion);
        OriginatingSystem = NormalizeOptional(originatingSystem);
        Authorization = NormalizeOptional(authorization);
    }

    private static string NormalizeRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "STEP header collections cannot contain null, empty, or whitespace entries.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}