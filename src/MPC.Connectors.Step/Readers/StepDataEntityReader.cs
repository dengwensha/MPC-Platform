using System.Text;
using System.Text.RegularExpressions;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Contracts;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Readers;

public sealed class StepDataEntityReader
    : IStepDataEntityReader
{
    private static readonly Regex SimpleEntityRegex =
        new(
            @"^\s*(#\d+)\s*=\s*([A-Z0-9_]+)\s*\(",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

    private static readonly Regex ComplexEntityRegex =
        new(
            @"^\s*(#\d+)\s*=\s*\(",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

    private static readonly Regex ComplexComponentRegex =
        new(
            @"(?:^|\s)([A-Z][A-Z0-9_]*)\s*\(",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

    private readonly IStepDataInventoryValidator
        _inventoryValidator;

    public StepDataEntityReader(
        IStepDataInventoryValidator inventoryValidator)
    {
        _inventoryValidator =
            inventoryValidator ??
            throw new ArgumentNullException(
                nameof(inventoryValidator));
    }

    public CadConnectorResult<StepDataInventory> Read(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "STEP file path cannot be null, empty, or whitespace.",
                nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            return Failure(
                code:
                    "STEP_FILE_NOT_FOUND",
                message:
                    $"STEP file was not found: '{filePath}'.");
        }

        if (!HasSupportedExtension(filePath))
        {
            return Failure(
                code:
                    "STEP_FILE_EXTENSION_UNSUPPORTED",
                message:
                    "STEP DATA Reader accepts only .step and .stp files.");
        }

        string content;

        try
        {
            content =
                File.ReadAllText(
                    filePath,
                    Encoding.UTF8);
        }
        catch (Exception exception)
        {
            return Failure(
                code:
                    "STEP_FILE_READ_FAILED",
                message:
                    "STEP file could not be read.",
                exceptionType:
                    exception.GetType().Name);
        }

        var dataSection =
            ExtractDataSection(content);

        if (dataSection is null)
        {
            return Failure(
                code:
                    "STEP_DATA_SECTION_NOT_FOUND",
                message:
                    "A valid DATA...ENDSEC section could not be found.");
        }

        var statements =
            SplitStatements(dataSection);

        var entities =
            new List<StepDataEntityRecord>();

        var diagnostics =
            new List<CadDiagnostic>();

        foreach (var statement
                 in statements)
        {
            var record =
                CreateEntityRecord(
                    statement);

            entities.Add(record);

            diagnostics.AddRange(
                record.Diagnostics);
        }

        var inventoryDiagnostics =
            _inventoryValidator.Validate(
                entities);

        diagnostics.AddRange(
            inventoryDiagnostics);

        var inventory =
            new StepDataInventory(
                entities);

        if (diagnostics.Count > 0)
        {
            return CadConnectorResult<StepDataInventory>
                .SuccessWithDiagnostics(
                    inventory,
                    diagnostics);
        }

        return CadConnectorResult<StepDataInventory>
            .Success(inventory);
    }

    private static StepDataEntityRecord
        CreateEntityRecord(
            string statement)
    {
        var normalizedStatement =
            statement.Trim();

        var simpleMatch =
            SimpleEntityRegex.Match(
                normalizedStatement);

        if (simpleMatch.Success)
        {
            return new StepDataEntityRecord(
                sourceEntityId:
                    simpleMatch.Groups[1].Value,
                sourceEntityType:
                    simpleMatch.Groups[2]
                        .Value
                        .ToUpperInvariant(),
                rawStatement:
                    normalizedStatement,
                readStatus:
                    CadReadStatus.Complete);
        }

        var complexMatch =
            ComplexEntityRegex.Match(
                normalizedStatement);

        if (complexMatch.Success)
        {
            var componentTypes =
                ParseComplexComponentTypes(
                    normalizedStatement);

            if (componentTypes.Count > 0)
            {
                return new StepDataEntityRecord(
                    sourceEntityId:
                        complexMatch.Groups[1].Value,
                    sourceEntityType:
                        "COMPLEX_ENTITY",
                    rawStatement:
                        normalizedStatement,
                    readStatus:
                        CadReadStatus.Complete,
                    componentTypes:
                        componentTypes);
            }

            var complexDiagnostic =
                new CadDiagnostic(
                    code:
                        "STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND",
                    severity:
                        CadDiagnosticSeverity.Error,
                    message:
                        "STEP complex entity was preserved, but no component types could be identified.",
                    sourceEntityId:
                        complexMatch.Groups[1].Value,
                    sourceEntityType:
                        "COMPLEX_ENTITY");

            return new StepDataEntityRecord(
                sourceEntityId:
                    complexMatch.Groups[1].Value,
                sourceEntityType:
                    "UNKNOWN",
                rawStatement:
                    normalizedStatement,
                readStatus:
                    CadReadStatus.Partial,
                diagnostics:
                    new[]
                    {
                        complexDiagnostic
                    });
        }

        var sourceEntityId =
            TryReadSourceEntityId(
                normalizedStatement);

        var diagnostic =
            new CadDiagnostic(
                code:
                    "STEP_DATA_ENTITY_HEADER_UNRECOGNIZED",
                severity:
                    CadDiagnosticSeverity.Error,
                message:
                    "STEP DATA statement was preserved, but its entity header could not be recognized.",
                sourceEntityId:
                    sourceEntityId,
                sourceEntityType:
                    "UNKNOWN");

        return new StepDataEntityRecord(
            sourceEntityId:
                sourceEntityId,
            sourceEntityType:
                "UNKNOWN",
            rawStatement:
                normalizedStatement,
            readStatus:
                CadReadStatus.Partial,
            diagnostics:
                new[]
                {
                    diagnostic
                });
    }

    private static IReadOnlyList<string>
        ParseComplexComponentTypes(
            string statement)
    {
        var equalsIndex =
            statement.IndexOf('=');

        if (equalsIndex < 0)
        {
            return Array.Empty<string>();
        }

        var complexBody =
            statement[(equalsIndex + 1)..];

        var matches =
            ComplexComponentRegex.Matches(
                complexBody);

        return matches
            .Select(
                match =>
                    match.Groups[1]
                        .Value
                        .ToUpperInvariant())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }

    private static string? ExtractDataSection(
        string content)
    {
        var dataStart =
            FindKeywordStatement(
                content,
                "DATA",
                startIndex: 0);

        if (dataStart < 0)
        {
            return null;
        }

        var dataContentStart =
            content.IndexOf(
                ';',
                dataStart);

        if (dataContentStart < 0)
        {
            return null;
        }

        var endSectionStart =
            FindKeywordStatement(
                content,
                "ENDSEC",
                dataContentStart + 1);

        if (endSectionStart < 0)
        {
            return null;
        }

        return content.Substring(
            dataContentStart + 1,
            endSectionStart -
            dataContentStart -
            1);
    }

    private static int FindKeywordStatement(
        string content,
        string keyword,
        int startIndex)
    {
        var searchIndex =
            startIndex;

        while (searchIndex <
               content.Length)
        {
            var index =
                content.IndexOf(
                    keyword,
                    searchIndex,
                    StringComparison
                        .OrdinalIgnoreCase);

            if (index < 0)
            {
                return -1;
            }

            var beforeIsValid =
                index == 0 ||
                !IsIdentifierCharacter(
                    content[index - 1]);

            var afterIndex =
                index +
                keyword.Length;

            var afterIsValid =
                afterIndex >=
                content.Length ||
                !IsIdentifierCharacter(
                    content[afterIndex]);

            if (beforeIsValid &&
                afterIsValid)
            {
                return index;
            }

            searchIndex =
                index +
                keyword.Length;
        }

        return -1;
    }

    private static bool IsIdentifierCharacter(
        char character)
    {
        return char.IsLetterOrDigit(
                   character) ||
               character == '_';
    }

    private static IReadOnlyList<string>
        SplitStatements(
            string dataSection)
    {
        var statements =
            new List<string>();

        var current =
            new StringBuilder();

        var insideString =
            false;

        for (var index = 0;
             index < dataSection.Length;
             index++)
        {
            var character =
                dataSection[index];

            if (character == '\'')
            {
                current.Append(
                    character);

                if (insideString &&
                    index + 1 <
                    dataSection.Length &&
                    dataSection[index + 1] ==
                    '\'')
                {
                    current.Append(
                        dataSection[index + 1]);

                    index++;
                    continue;
                }

                insideString =
                    !insideString;

                continue;
            }

            if (!insideString &&
                character == ';')
            {
                var statement =
                    current
                        .ToString()
                        .Trim();

                if (!string.IsNullOrWhiteSpace(
                        statement))
                {
                    statements.Add(
                        statement + ";");
                }

                current.Clear();
                continue;
            }

            current.Append(
                character);
        }

        var remaining =
            current
                .ToString()
                .Trim();

        if (!string.IsNullOrWhiteSpace(
                remaining))
        {
            statements.Add(
                remaining);
        }

        return statements.AsReadOnly();
    }

    private static string?
        TryReadSourceEntityId(
            string statement)
    {
        var trimmed =
            statement.TrimStart();

        if (!trimmed.StartsWith(
                '#'))
        {
            return null;
        }

        var index = 1;

        while (index <
                   trimmed.Length &&
               char.IsDigit(
                   trimmed[index]))
        {
            index++;
        }

        return index > 1
            ? trimmed[..index]
            : null;
    }

    private static bool HasSupportedExtension(
        string filePath)
    {
        var extension =
            Path.GetExtension(
                filePath);

        return extension.Equals(
                   ".step",
                   StringComparison
                       .OrdinalIgnoreCase) ||
               extension.Equals(
                   ".stp",
                   StringComparison
                       .OrdinalIgnoreCase);
    }

    private static
        CadConnectorResult<StepDataInventory>
        Failure(
            string code,
            string message,
            string? exceptionType = null)
    {
        var diagnostic =
            new CadDiagnostic(
                code:
                    code,
                severity:
                    CadDiagnosticSeverity.Fatal,
                message:
                    message,
                exceptionType:
                    exceptionType);

        return CadConnectorResult<StepDataInventory>
            .Failure(
                new[]
                {
                    diagnostic
                });
    }
}