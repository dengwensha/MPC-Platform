using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Connectors.Step.Semantics;

public sealed class StepSemanticEntityReader
{
    private readonly StepParameterParser
        _parameterParser;

    private readonly IReadOnlyDictionary<
        string,
        IStepSemanticEntityHandler>
        _handlers;

    public StepSemanticEntityReader(
        StepParameterParser parameterParser,
        IEnumerable<IStepSemanticEntityHandler> handlers)
    {
        _parameterParser =
            parameterParser ??
            throw new ArgumentNullException(
                nameof(parameterParser));

        if (handlers is null)
        {
            throw new ArgumentNullException(
                nameof(handlers));
        }

        var handlerList =
            handlers.ToList();

        if (handlerList.Any(
                handler =>
                    handler is null))
        {
            throw new ArgumentException(
                "Semantic entity handlers cannot contain null entries.",
                nameof(handlers));
        }

        var duplicateHandlerType =
            handlerList
                .GroupBy(
                    handler =>
                        NormalizeEntityType(
                            handler.SupportedEntityType),
                    StringComparer.Ordinal)
                .FirstOrDefault(
                    group =>
                        group.Count() > 1);

        if (duplicateHandlerType is not null)
        {
            throw new InvalidOperationException(
                $"Multiple semantic handlers are registered for STEP entity type '{duplicateHandlerType.Key}'.");
        }

        _handlers =
            handlerList.ToDictionary(
                handler =>
                    NormalizeEntityType(
                        handler.SupportedEntityType),
                handler =>
                    handler,
                StringComparer.Ordinal);
    }

    public StepSemanticEntity? Read(
        StepDataEntityRecord record)
    {
        if (record is null)
        {
            throw new ArgumentNullException(
                nameof(record));
        }

        var normalizedEntityType =
            NormalizeEntityType(
                record.SourceEntityType);

        if (!_handlers.TryGetValue(
                normalizedEntityType,
                out var handler))
        {
            return null;
        }

        var parameterText =
            ExtractParameterText(
                record.RawStatement);

        var parameters =
            _parameterParser.Parse(
                parameterText);

        return handler.Read(
            record,
            parameters);
    }

    private static string ExtractParameterText(
        string rawStatement)
    {
        if (string.IsNullOrWhiteSpace(
                rawStatement))
        {
            throw new ArgumentException(
                "Raw STEP statement cannot be null, empty, or whitespace.",
                nameof(rawStatement));
        }

        var equalsIndex =
            FindEqualsOutsideString(
                rawStatement);

        if (equalsIndex < 0)
        {
            throw new FormatException(
                "STEP entity statement does not contain an assignment operator.");
        }

        var openParenthesisIndex =
            FindOpeningParenthesis(
                rawStatement,
                equalsIndex + 1);

        if (openParenthesisIndex < 0)
        {
            throw new FormatException(
                "STEP entity statement does not contain an opening parenthesis.");
        }

        var closeParenthesisIndex =
            FindMatchingClosingParenthesis(
                rawStatement,
                openParenthesisIndex);

        if (closeParenthesisIndex < 0)
        {
            throw new FormatException(
                "STEP entity statement does not contain a matching closing parenthesis.");
        }

        EnsureOnlyStatementTerminatorFollows(
            rawStatement,
            closeParenthesisIndex + 1);

        return rawStatement.Substring(
            openParenthesisIndex + 1,
            closeParenthesisIndex -
            openParenthesisIndex -
            1);
    }

    private static int FindEqualsOutsideString(
        string value)
    {
        var insideString =
            false;

        for (var index = 0;
             index < value.Length;
             index++)
        {
            var character =
                value[index];

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 < value.Length &&
                    value[index + 1] == '\'')
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

    private static int FindOpeningParenthesis(
        string value,
        int startIndex)
    {
        var insideString =
            false;

        for (var index = startIndex;
             index < value.Length;
             index++)
        {
            var character =
                value[index];

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 < value.Length &&
                    value[index + 1] == '\'')
                {
                    index++;
                    continue;
                }

                insideString =
                    !insideString;

                continue;
            }

            if (!insideString &&
                character == '(')
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindMatchingClosingParenthesis(
        string value,
        int openParenthesisIndex)
    {
        var depth =
            0;

        var insideString =
            false;

        var insideBinary =
            false;

        for (var index =
                 openParenthesisIndex;
             index < value.Length;
             index++)
        {
            var character =
                value[index];

            if (insideBinary)
            {
                if (character == '"')
                {
                    insideBinary =
                        false;
                }

                continue;
            }

            if (!insideString &&
                character == '"')
            {
                insideBinary =
                    true;

                continue;
            }

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 < value.Length &&
                    value[index + 1] == '\'')
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
                depth++;
                continue;
            }

            if (character != ')')
            {
                continue;
            }

            depth--;

            if (depth == 0)
            {
                return index;
            }

            if (depth < 0)
            {
                return -1;
            }
        }

        return -1;
    }

    private static void EnsureOnlyStatementTerminatorFollows(
        string value,
        int startIndex)
    {
        for (var index = startIndex;
             index < value.Length;
             index++)
        {
            var character =
                value[index];

            if (char.IsWhiteSpace(
                    character) ||
                character == ';')
            {
                continue;
            }

            throw new FormatException(
                "STEP entity statement contains unexpected content after its parameter list.");
        }
    }

    private static string NormalizeEntityType(
        string entityType)
    {
        if (string.IsNullOrWhiteSpace(
                entityType))
        {
            throw new ArgumentException(
                "STEP entity type cannot be null, empty, or whitespace.",
                nameof(entityType));
        }

        return entityType
            .Trim()
            .ToUpperInvariant();
    }
}