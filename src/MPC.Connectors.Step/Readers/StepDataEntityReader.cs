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

    /// <summary>Reads a STEP DATA inventory and preserves supported reader diagnostics.
    /// Selects one unparameterized structural DATA section, skipping quoted/comment/
    /// binary decoys. Ambiguous or unsupported section layout returns the existing
    /// fatal STEP_DATA_SECTION_NOT_FOUND result, not a partial first-section success.
    /// This API also supports DATA fragments; it is not full Part 21 conformance proof.
    /// </summary>
    /// <exception cref="System.FormatException">
    /// A simple declaration uses COMPLEX_ENTITY, the internal complex-record discriminator.
    /// The read is rejected before constructing an inconsistent record; this is not
    /// a guarantee that every malformed STEP expression throws this exception.
    /// </exception>
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

        return ReadContent(content);
    }

    /// <summary>
    /// Reads the caller-owned capture from its current position without a path or
    /// reopening a file. Leaves the stream open. Uses the same UTF-8 fallback and
    /// BOM detection as Read(path). Stream access failures propagate to the owner;
    /// they are not classified as STEP source diagnostics. This is not evidence
    /// issuance: Application still owns capture, hash and session binding.
    /// </summary>
    /// <exception cref="FormatException">Reserved simple COMPLEX_ENTITY declaration.</exception>
    public CadConnectorResult<StepDataInventory> ReadCaptured(Stream capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        using var text = new StreamReader(capture, Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        return ReadContent(text.ReadToEnd());
    }

    private CadConnectorResult<StepDataInventory> ReadContent(string content)
    {
        var dataSection =
            ExtractDataSection(
                content);

        if (dataSection is null)
        {
            return Failure(
                code:
                    "STEP_DATA_SECTION_NOT_FOUND",
                message:
                    "A valid DATA...ENDSEC section could not be found.");
        }

        var statements =
            SplitStatements(
                dataSection);

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

            entities.Add(
                record);

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
            .Success(
                inventory);
    }

    private static StepDataEntityRecord
        CreateEntityRecord(
            string statement)
    {
        var normalizedStatement =
            statement.Trim();

        var referencedEntityIds =
            ExtractReferencedEntityIds(
                normalizedStatement);

        var simpleMatch =
            SimpleEntityRegex.Match(
                normalizedStatement);

        if (simpleMatch.Success)
        {
            string entityType = simpleMatch.Groups[2].Value.ToUpperInvariant();
            // COMPLEX_ENTITY is the record model's internal discriminator, not
            // a simple STEP declaration. Reject this input before its invariant guard.
            if (entityType == "COMPLEX_ENTITY")
            {
                throw new FormatException(
                    $"STEP entity '{simpleMatch.Groups[1].Value}' cannot use the internal COMPLEX_ENTITY type as a simple declaration.");
            }

            return new StepDataEntityRecord(
                sourceEntityId:
                    simpleMatch.Groups[1].Value,
                sourceEntityType:
                    entityType,
                rawStatement:
                    normalizedStatement,
                readStatus:
                    CadReadStatus.Complete,
                referencedEntityIds:
                    referencedEntityIds);
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
                        componentTypes,
                    referencedEntityIds:
                        referencedEntityIds);
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
                    },
                referencedEntityIds:
                    referencedEntityIds);
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
                },
            referencedEntityIds:
                referencedEntityIds);
    }

    private static IReadOnlyList<string>
        ExtractReferencedEntityIds(
            string statement)
    {
        var equalsIndex =
            FindEqualsOutsideString(
                statement);

        if (equalsIndex < 0 ||
            equalsIndex + 1 >=
            statement.Length)
        {
            return Array.Empty<string>();
        }

        var referencedEntityIds =
            new List<string>();

        var insideString =
            false;

        for (var index =
                 equalsIndex + 1;
             index <
             statement.Length;
             index++)
        {
            var character =
                statement[index];

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 <
                    statement.Length &&
                    statement[index + 1] ==
                    '\'')
                {
                    index++;
                    continue;
                }

                insideString =
                    !insideString;

                continue;
            }

            if (insideString ||
                character != '#')
            {
                continue;
            }

            var digitStart =
                index + 1;

            var digitEnd =
                digitStart;

            while (digitEnd <
                       statement.Length &&
                   char.IsDigit(
                       statement[digitEnd]))
            {
                digitEnd++;
            }

            if (digitEnd ==
                digitStart)
            {
                continue;
            }

            referencedEntityIds.Add(
                statement[
                    index..digitEnd]);

            index =
                digitEnd - 1;
        }

        return referencedEntityIds
            .AsReadOnly();
    }

    private static int FindEqualsOutsideString(
        string statement)
    {
        var insideString =
            false;

        for (var index = 0;
             index <
             statement.Length;
             index++)
        {
            var character =
                statement[index];

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 <
                    statement.Length &&
                    statement[index + 1] ==
                    '\'')
                {
                    index++;
                    continue;
                }

                insideString =
                    !insideString;

                continue;
            }

            if (!insideString &&
                character == '=')
            {
                return index;
            }
        }

        return -1;
    }

    private static IReadOnlyList<string>
        ParseComplexComponentTypes(
            string statement)
    {
        var equalsIndex =
            FindEqualsOutsideString(
                statement);

        if (equalsIndex < 0 ||
            equalsIndex + 1 >=
            statement.Length)
        {
            return Array.Empty<string>();
        }

        var componentTypes =
            new List<string>();

        var insideString =
            false;

        var parenthesisDepth =
            0;

        for (var index =
                 equalsIndex + 1;
             index <
             statement.Length;
             index++)
        {
            var character =
                statement[index];

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 <
                    statement.Length &&
                    statement[index + 1] ==
                    '\'')
                {
                    index++;
                    continue;
                }

                insideString =
                    !insideString;

                continue;
            }

            if (insideString)
            {
                continue;
            }

            if (character == '(')
            {
                parenthesisDepth++;
                continue;
            }

            if (character == ')')
            {
                if (parenthesisDepth > 0)
                {
                    parenthesisDepth--;
                }

                continue;
            }

            if (parenthesisDepth != 1 ||
                !IsStepIdentifierStart(
                    character))
            {
                continue;
            }

            var identifierStart =
                index;

            var identifierEnd =
                index + 1;

            while (identifierEnd <
                       statement.Length &&
                   IsStepIdentifierCharacter(
                       statement[identifierEnd]))
            {
                identifierEnd++;
            }

            var nextIndex =
                identifierEnd;

            while (nextIndex <
                       statement.Length &&
                   char.IsWhiteSpace(
                       statement[nextIndex]))
            {
                nextIndex++;
            }

            if (nextIndex <
                    statement.Length &&
                statement[nextIndex] == '(')
            {
                componentTypes.Add(
                    statement[
                            identifierStart..identifierEnd]
                        .ToUpperInvariant());
            }

            index =
                identifierEnd - 1;
        }

        return componentTypes
            .AsReadOnly();
    }

    private static bool IsStepIdentifierStart(
        char character)
    {
        return char.IsLetter(
            character);
    }

    private static bool IsStepIdentifierCharacter(
        char character)
    {
        return char.IsLetterOrDigit(
                   character) ||
               character == '_';
    }

    // Reader API continues to support DATA fragments; full exchange/header policy
    // belongs to Application evidence. Selection itself must be unambiguous: one
    // bare DATA section, no additional/parameterized/unsupported sections or tail.
    // Statement boundaries skip strings, doubled quotes, binary literals and comments.
    // Parenthesis balance within entity payload remains the existing inventory/
    // semantic parser's responsibility, preserving partial-record diagnostics.
    private static string? ExtractDataSection(string content)
    {
        int dataStart = -1, dataEnd = -1, statementStart = 0;
        bool quoted = false, binary = false, comment = false, terminated = false;
        var prefix = new StringBuilder(128);
        bool truncated = false;
        void Append(char c, bool separator = false)
        {
            if (separator)
            {
                if (prefix.Length == 0 || prefix[^1] == ' ') return;
                c = ' ';
            }
            if (prefix.Length < 128) prefix.Append(c);
            else truncated = true;
        }
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (comment)
            {
                if (c == '*' && i + 1 < content.Length && content[i + 1] == '/')
                { comment = false; i++; Append(' ', separator: true); }
                continue;
            }
            if (!quoted && !binary && c == '/' && i + 1 < content.Length && content[i + 1] == '*')
            { comment = true; i++; Append(' ', separator: true); continue; }
            if (!quoted && !binary && char.IsWhiteSpace(c)) { Append(c, separator: true); continue; }
            if (!quoted && !binary && c == ';')
            {
                string statement = prefix.ToString().Trim();
                int nameEnd = 0;
                while (nameEnd < statement.Length && (char.IsLetterOrDigit(statement[nameEnd]) || statement[nameEnd] is '_' or '-')) nameEnd++;
                string name = statement[..nameEnd];
                string suffix = statement[nameEnd..].TrimStart();
                bool exact = !truncated && suffix.Length == 0;
                bool Is(string value) => name.Equals(value, StringComparison.OrdinalIgnoreCase);
                if (terminated || (dataEnd >= 0 && statement.Length == 0)) return null;
                if (Is("ANCHOR") || Is("REFERENCE") || Is("SIGNATURE")) return null;
                if (Is("DATA") && (exact || suffix.StartsWith('(')))
                {
                    if (!exact || dataStart >= 0) return null;
                    dataStart = i + 1;
                }
                else if (Is("ENDSEC") && exact && dataStart >= 0)
                {
                    if (dataEnd >= 0) return null;
                    dataEnd = statementStart;
                }
                else if (Is("END-ISO-10303-21") && exact)
                {
                    if (dataEnd < 0) return null;
                    terminated = true;
                }
                else if (dataEnd >= 0 && statement.Length != 0) return null;
                prefix.Clear(); truncated = false; statementStart = i + 1;
                continue;
            }
            Append(c);
            if (!binary && c == '\'')
            {
                if (quoted && i + 1 < content.Length && content[i + 1] == '\'')
                { Append(content[++i]); continue; }
                quoted = !quoted;
            }
            else if (!quoted && c == '"') binary = !binary;
        }
        if (quoted || binary || comment || prefix.ToString().Trim().Length != 0
            || dataStart < 0 || dataEnd < dataStart) return null;
        return content.Substring(dataStart, dataEnd - dataStart);
    }

    private static IReadOnlyList<string> SplitStatements(string dataSection)
    {
        var statements = new List<string>();
        var current = new StringBuilder();
        bool quoted = false, binary = false, comment = false;
        for (int i = 0; i < dataSection.Length; i++)
        {
            char c = dataSection[i];
            if (comment)
            {
                if (c == '*' && i + 1 < dataSection.Length && dataSection[i + 1] == '/')
                { comment = false; i++; current.Append(' '); }
                continue;
            }
            if (!quoted && !binary && c == '/' && i + 1 < dataSection.Length && dataSection[i + 1] == '*')
            { comment = true; i++; current.Append(' '); continue; }
            if (!quoted && !binary && c == ';')
            {
                string statement = current.ToString().Trim();
                if (statement.Length != 0) statements.Add(statement + ";");
                current.Clear(); continue;
            }
            current.Append(c);
            if (!binary && c == '\'')
            {
                if (quoted && i + 1 < dataSection.Length && dataSection[i + 1] == '\'')
                { current.Append(dataSection[++i]); continue; }
                quoted = !quoted;
            }
            else if (!quoted && c == '"') binary = !binary;
        }
        string remaining = current.ToString().Trim();
        if (remaining.Length != 0) statements.Add(remaining);
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

        var index =
            1;

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