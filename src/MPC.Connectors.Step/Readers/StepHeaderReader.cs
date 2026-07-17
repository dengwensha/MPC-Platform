using System.Text;
using System.Text.RegularExpressions;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Contracts;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Readers;

public sealed class StepHeaderReader : IStepHeaderReader
{
    private static readonly Regex HeaderSectionRegex =
        new(
            @"HEADER\s*;(.*?)ENDSEC\s*;",
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

    public CadConnectorResult<StepHeaderInfo> Read(
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
                code: "STEP_FILE_NOT_FOUND",
                message: $"STEP file was not found: '{filePath}'.");
        }

        if (!HasSupportedExtension(filePath))
        {
            return Failure(
                code: "STEP_FILE_EXTENSION_UNSUPPORTED",
                message:
                    "STEP Header Reader accepts only .step and .stp files.");
        }

        string content;

        try
        {
            content = File.ReadAllText(
                filePath,
                Encoding.UTF8);
        }
        catch (Exception exception)
        {
            return Failure(
                code: "STEP_FILE_READ_FAILED",
                message: "STEP file could not be read.",
                exceptionType: exception.GetType().Name);
        }

        var headerMatch =
            HeaderSectionRegex.Match(content);

        if (!headerMatch.Success)
        {
            return Failure(
                code: "STEP_HEADER_NOT_FOUND",
                message:
                    "A valid HEADER...ENDSEC section could not be found.");
        }

        var headerSection =
            headerMatch.Groups[1].Value;

        try
        {
            var descriptions =
                ParseRequiredStringList(
                    headerSection,
                    "FILE_DESCRIPTION",
                    argumentIndex: 0);

            var implementationLevel =
                ParseOptionalScalar(
                    headerSection,
                    "FILE_DESCRIPTION",
                    argumentIndex: 1);

            var fileNameArguments =
                ParseEntityArguments(
                    headerSection,
                    "FILE_NAME");

            EnsureMinimumArgumentCount(
                fileNameArguments,
                expectedMinimum: 7,
                entityName: "FILE_NAME");

            var schemas =
                ParseRequiredStringList(
                    headerSection,
                    "FILE_SCHEMA",
                    argumentIndex: 0);

            var header = new StepHeaderInfo(
                descriptions: descriptions,
                implementationLevel: implementationLevel,
                fileName: ParseOptionalString(
                    fileNameArguments[0]),
                timeStamp: ParseOptionalString(
                    fileNameArguments[1]),
                author: ParseFirstListValue(
                    fileNameArguments[2]),
                organization: ParseFirstListValue(
                    fileNameArguments[3]),
                preprocessorVersion: ParseOptionalString(
                    fileNameArguments[4]),
                originatingSystem: ParseOptionalString(
                    fileNameArguments[5]),
                authorization: ParseOptionalString(
                    fileNameArguments[6]),
                schemas: schemas);

            return CadConnectorResult<StepHeaderInfo>
                .Success(header);
        }
        catch (Exception exception)
        {
            return Failure(
                code: "STEP_HEADER_PARSE_FAILED",
                message: exception.Message,
                exceptionType: exception.GetType().Name);
        }
    }

    private static IReadOnlyList<string> ParseRequiredStringList(
        string headerSection,
        string entityName,
        int argumentIndex)
    {
        var arguments =
            ParseEntityArguments(
                headerSection,
                entityName);

        if (argumentIndex >= arguments.Count)
        {
            throw new FormatException(
                $"{entityName} does not contain argument " +
                $"{argumentIndex}.");
        }

        var values =
            ParseStringList(arguments[argumentIndex]);

        if (values.Count == 0)
        {
            throw new FormatException(
                $"{entityName} must contain at least one value.");
        }

        return values;
    }

    private static string? ParseOptionalScalar(
        string headerSection,
        string entityName,
        int argumentIndex)
    {
        var arguments =
            ParseEntityArguments(
                headerSection,
                entityName);

        if (argumentIndex >= arguments.Count)
        {
            return null;
        }

        return ParseOptionalString(
            arguments[argumentIndex]);
    }

    private static IReadOnlyList<string> ParseEntityArguments(
        string headerSection,
        string entityName)
    {
        var entityText =
            FindEntityText(
                headerSection,
                entityName);

        var openingParenthesis =
            entityText.IndexOf('(');

        var closingParenthesis =
            entityText.LastIndexOf(')');

        if (openingParenthesis < 0 ||
            closingParenthesis <= openingParenthesis)
        {
            throw new FormatException(
                $"{entityName} has an invalid argument list.");
        }

        var argumentText =
            entityText.Substring(
                openingParenthesis + 1,
                closingParenthesis -
                openingParenthesis -
                1);

        return SplitTopLevelArguments(
            argumentText);
    }

    private static string FindEntityText(
        string headerSection,
        string entityName)
    {
        var startIndex =
            headerSection.IndexOf(
                entityName,
                StringComparison.OrdinalIgnoreCase);

        if (startIndex < 0)
        {
            throw new FormatException(
                $"{entityName} was not found.");
        }

        var endIndex =
            FindStatementTerminator(
                headerSection,
                startIndex);

        if (endIndex < 0)
        {
            throw new FormatException(
                $"{entityName} is not terminated by ';'.");
        }

        return headerSection.Substring(
            startIndex,
            endIndex - startIndex);
    }

    private static int FindStatementTerminator(
        string text,
        int startIndex)
    {
        var insideString = false;

        for (var index = startIndex;
             index < text.Length;
             index++)
        {
            var character = text[index];

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 < text.Length &&
                    text[index + 1] == '\'')
                {
                    index++;
                    continue;
                }

                insideString = !insideString;
                continue;
            }

            if (!insideString &&
                character == ';')
            {
                return index;
            }
        }

        return -1;
    }

    private static IReadOnlyList<string> SplitTopLevelArguments(
        string text)
    {
        var arguments = new List<string>();
        var current = new StringBuilder();

        var nestingLevel = 0;
        var insideString = false;

        for (var index = 0;
             index < text.Length;
             index++)
        {
            var character = text[index];

            if (character == '\'')
            {
                current.Append(character);

                if (insideString &&
                    index + 1 < text.Length &&
                    text[index + 1] == '\'')
                {
                    current.Append(text[index + 1]);
                    index++;
                    continue;
                }

                insideString = !insideString;
                continue;
            }

            if (!insideString)
            {
                if (character == '(')
                {
                    nestingLevel++;
                }
                else if (character == ')')
                {
                    nestingLevel--;
                }
                else if (character == ',' &&
                         nestingLevel == 0)
                {
                    arguments.Add(
                        current.ToString().Trim());

                    current.Clear();
                    continue;
                }
            }

            current.Append(character);
        }

        arguments.Add(
            current.ToString().Trim());

        return arguments;
    }

    private static IReadOnlyList<string> ParseStringList(
        string value)
    {
        var trimmed = value.Trim();

        if (!trimmed.StartsWith('(') ||
            !trimmed.EndsWith(')'))
        {
            throw new FormatException(
                $"Expected STEP string list but received '{value}'.");
        }

        var inner =
            trimmed[1..^1];

        return SplitTopLevelArguments(inner)
            .Select(ParseOptionalString)
            .Where(item => item is not null)
            .Cast<string>()
            .ToList()
            .AsReadOnly();
    }

    private static string? ParseFirstListValue(
        string value)
    {
        var values =
            ParseStringList(value);

        return values.FirstOrDefault();
    }

    private static string? ParseOptionalString(
        string value)
    {
        var trimmed = value.Trim();

        if (trimmed is "$" or "*")
        {
            return null;
        }

        if (trimmed.Length >= 2 &&
            trimmed[0] == '\'' &&
            trimmed[^1] == '\'')
        {
            return trimmed[1..^1]
                .Replace("''", "'");
        }

        return string.IsNullOrWhiteSpace(trimmed)
            ? null
            : trimmed;
    }

    private static void EnsureMinimumArgumentCount(
        IReadOnlyCollection<string> arguments,
        int expectedMinimum,
        string entityName)
    {
        if (arguments.Count < expectedMinimum)
        {
            throw new FormatException(
                $"{entityName} must contain at least " +
                $"{expectedMinimum} arguments.");
        }
    }

    private static bool HasSupportedExtension(
        string filePath)
    {
        var extension =
            Path.GetExtension(filePath);

        return extension.Equals(
                   ".step",
                   StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(
                   ".stp",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static CadConnectorResult<StepHeaderInfo> Failure(
        string code,
        string message,
        string? exceptionType = null)
    {
        var diagnostic = new CadDiagnostic(
            code: code,
            severity: CadDiagnosticSeverity.Fatal,
            message: message,
            exceptionType: exceptionType);

        return CadConnectorResult<StepHeaderInfo>
            .Failure(
                new[]
                {
                    diagnostic
                });
    }
}