using System.Globalization;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Parsing;

public sealed class StepParameterParser
{
    public IReadOnlyList<StepParameter> Parse(
        string parameterText)
    {
        if (parameterText is null)
        {
            throw new ArgumentNullException(
                nameof(parameterText));
        }

        if (string.IsNullOrWhiteSpace(
                parameterText))
        {
            return Array.Empty<StepParameter>();
        }

        var rawParameters =
            SplitTopLevelParameters(
                parameterText);

        var parameters =
            rawParameters
                .Select(
                    ParseSingleParameter)
                .ToList();

        return parameters.AsReadOnly();
    }

    private StepParameter ParseSingleParameter(
        string rawText)
    {
        var normalized =
            rawText.Trim();

        if (normalized.Length == 0)
        {
            throw new FormatException(
                "STEP parameter cannot be empty.");
        }

        if (normalized == "$")
        {
            return new StepNullParameter(
                rawText:
                    normalized);
        }

        if (normalized == "*")
        {
            return new StepOmittedParameter(
                rawText:
                    normalized);
        }

        if (IsStringParameter(
                normalized))
        {
            return ParseStringParameter(
                normalized);
        }

        if (TryParseBinaryParameter(
                normalized,
                out var binaryParameter))
        {
            return binaryParameter;
        }

        if (IsEntityReference(
                normalized))
        {
            return new StepEntityReferenceParameter(
                rawText:
                    normalized,
                entityId:
                    normalized);
        }

        if (TryParseEnumerationParameter(
                normalized,
                out var enumerationParameter))
        {
            return enumerationParameter;
        }

        if (TryParseAggregateParameter(
                normalized,
                out var aggregateParameter))
        {
            return aggregateParameter;
        }

        if (TryParseTypedParameter(
                normalized,
                out var typedParameter))
        {
            return typedParameter;
        }

        if (TryParseIntegerParameter(
                normalized,
                out var integerParameter))
        {
            return integerParameter;
        }

        if (TryParseRealParameter(
                normalized,
                out var realParameter))
        {
            return realParameter;
        }

        return new StepUnknownParameter(
            rawText:
                normalized);
    }

    private static StepStringParameter ParseStringParameter(
        string rawText)
    {
        var encodedValue =
            rawText[
                1..^1];

        var decodedValue =
            encodedValue.Replace(
                "''",
                "'",
                StringComparison.Ordinal);

        return new StepStringParameter(
            rawText:
                rawText,
            value:
                decodedValue);
    }

    private static bool TryParseBinaryParameter(
        string rawText,
        out StepBinaryParameter binaryParameter)
    {
        binaryParameter =
            null!;

        if (rawText.Length < 3 ||
            rawText[0] != '"' ||
            rawText[^1] != '"')
        {
            return false;
        }

        var encodedValue =
            rawText[
                1..^1];

        if (!IsValidBinaryEncoding(
                encodedValue))
        {
            return false;
        }

        binaryParameter =
            new StepBinaryParameter(
                rawText:
                    rawText,
                encodedValue:
                    encodedValue);

        return true;
    }

    private bool TryParseAggregateParameter(
        string rawText,
        out StepAggregateParameter aggregateParameter)
    {
        aggregateParameter =
            null!;

        if (rawText.Length < 2 ||
            rawText[0] != '(')
        {
            return false;
        }

        var closingParenthesisIndex =
            FindMatchingClosingParenthesis(
                rawText,
                openParenthesisIndex:
                    0);

        if (closingParenthesisIndex < 0)
        {
            throw new FormatException(
                $"STEP aggregate has no matching closing parenthesis: '{rawText}'.");
        }

        var trailingIndex =
            SkipWhitespace(
                rawText,
                closingParenthesisIndex + 1);

        if (trailingIndex !=
            rawText.Length)
        {
            return false;
        }

        var nestedParameterText =
            rawText.Substring(
                1,
                closingParenthesisIndex - 1);

        var items =
            string.IsNullOrWhiteSpace(
                nestedParameterText)
                ? Array.Empty<StepParameter>()
                : Parse(
                    nestedParameterText);

        aggregateParameter =
            new StepAggregateParameter(
                rawText:
                    rawText,
                items:
                    items);

        return true;
    }

    private bool TryParseTypedParameter(
        string rawText,
        out StepTypedParameter typedParameter)
    {
        typedParameter =
            null!;

        var identifierEnd =
            ReadIdentifierEnd(
                rawText);

        if (identifierEnd == 0)
        {
            return false;
        }

        var openParenthesisIndex =
            SkipWhitespace(
                rawText,
                identifierEnd);

        if (openParenthesisIndex >=
                rawText.Length ||
            rawText[openParenthesisIndex] != '(')
        {
            return false;
        }

        var closeParenthesisIndex =
            FindMatchingClosingParenthesis(
                rawText,
                openParenthesisIndex);

        if (closeParenthesisIndex < 0)
        {
            throw new FormatException(
                $"STEP typed parameter has no matching closing parenthesis: '{rawText}'.");
        }

        var trailingIndex =
            SkipWhitespace(
                rawText,
                closeParenthesisIndex + 1);

        if (trailingIndex !=
            rawText.Length)
        {
            return false;
        }

        var typeName =
            rawText[
                0..identifierEnd];

        var nestedParameterText =
            rawText.Substring(
                openParenthesisIndex + 1,
                closeParenthesisIndex -
                openParenthesisIndex -
                1);

        var nestedParameters =
            string.IsNullOrWhiteSpace(
                nestedParameterText)
                ? Array.Empty<StepParameter>()
                : Parse(
                    nestedParameterText);

        typedParameter =
            new StepTypedParameter(
                rawText:
                    rawText,
                typeName:
                    typeName,
                parameters:
                    nestedParameters);

        return true;
    }

    private static bool TryParseEnumerationParameter(
        string rawText,
        out StepEnumerationParameter enumerationParameter)
    {
        enumerationParameter =
            null!;

        if (rawText.Length < 3 ||
            rawText[0] != '.' ||
            rawText[^1] != '.')
        {
            return false;
        }

        var name =
            rawText[
                1..^1];

        if (!IsValidIdentifier(
                name))
        {
            return false;
        }

        enumerationParameter =
            new StepEnumerationParameter(
                rawText:
                    rawText,
                name:
                    name);

        return true;
    }

    private static bool TryParseIntegerParameter(
        string rawText,
        out StepIntegerParameter integerParameter)
    {
        integerParameter =
            null!;

        if (!IsIntegerText(
                rawText))
        {
            return false;
        }

        if (!long.TryParse(
                rawText,
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var value))
        {
            throw new FormatException(
                $"STEP integer parameter is outside Int64 range: '{rawText}'.");
        }

        integerParameter =
            new StepIntegerParameter(
                rawText:
                    rawText,
                value:
                    value);

        return true;
    }

    private static bool TryParseRealParameter(
        string rawText,
        out StepRealParameter realParameter)
    {
        realParameter =
            null!;

        if (!LooksLikeRealNumber(
                rawText))
        {
            return false;
        }

        if (!double.TryParse(
                rawText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return false;
        }

        if (!double.IsFinite(
                value))
        {
            throw new FormatException(
                $"STEP numeric parameter must be finite: '{rawText}'.");
        }

        realParameter =
            new StepRealParameter(
                rawText:
                    rawText,
                value:
                    value);

        return true;
    }

    private static IReadOnlyList<string>
        SplitTopLevelParameters(
            string parameterText)
    {
        var parameters =
            new List<string>();

        var parameterStart =
            0;

        var parenthesisDepth =
            0;

        var insideString =
            false;

        var insideBinary =
            false;

        for (var index = 0;
             index < parameterText.Length;
             index++)
        {
            var character =
                parameterText[index];

            if (insideBinary)
            {
                if (character == '"')
                {
                    insideBinary =
                        false;
                }

                continue;
            }

            if (character == '"' &&
                !insideString)
            {
                insideBinary =
                    true;

                continue;
            }

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 <
                    parameterText.Length &&
                    parameterText[index + 1] ==
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
                parenthesisDepth--;

                if (parenthesisDepth < 0)
                {
                    throw new FormatException(
                        "STEP parameter text contains an unmatched closing parenthesis.");
                }

                continue;
            }

            if (character == ',' &&
                parenthesisDepth == 0)
            {
                AddParameterSegment(
                    parameterText,
                    parameterStart,
                    index,
                    parameters);

                parameterStart =
                    index + 1;
            }
        }

        if (insideString)
        {
            throw new FormatException(
                "STEP parameter text contains an unterminated string.");
        }

        if (insideBinary)
        {
            throw new FormatException(
                "STEP parameter text contains an unterminated binary literal.");
        }

        if (parenthesisDepth != 0)
        {
            throw new FormatException(
                "STEP parameter text contains unmatched parentheses.");
        }

        AddParameterSegment(
            parameterText,
            parameterStart,
            parameterText.Length,
            parameters);

        return parameters.AsReadOnly();
    }

    private static void AddParameterSegment(
        string source,
        int startIndex,
        int endIndex,
        ICollection<string> destination)
    {
        var segment =
            source[
                    startIndex..endIndex]
                .Trim();

        if (segment.Length == 0)
        {
            throw new FormatException(
                "STEP parameter list contains an empty parameter.");
        }

        destination.Add(
            segment);
    }

    private static bool IsStringParameter(
        string rawText)
    {
        if (rawText.Length < 2 ||
            rawText[0] != '\'' ||
            rawText[^1] != '\'')
        {
            return false;
        }

        var insideString =
            false;

        for (var index = 0;
             index < rawText.Length;
             index++)
        {
            if (rawText[index] != '\'')
            {
                continue;
            }

            if (insideString &&
                index + 1 <
                rawText.Length &&
                rawText[index + 1] ==
                '\'')
            {
                index++;
                continue;
            }

            insideString =
                !insideString;

            if (!insideString &&
                index != rawText.Length - 1)
            {
                return false;
            }
        }

        return !insideString;
    }

    private static bool IsEntityReference(
        string rawText)
    {
        if (rawText.Length < 2 ||
            rawText[0] != '#')
        {
            return false;
        }

        for (var index = 1;
             index < rawText.Length;
             index++)
        {
            if (!char.IsDigit(
                    rawText[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsIntegerText(
        string rawText)
    {
        if (rawText.Length == 0)
        {
            return false;
        }

        var index =
            0;

        if (rawText[0] is '+' or '-')
        {
            index++;

            if (index ==
                rawText.Length)
            {
                return false;
            }
        }

        for (;
             index < rawText.Length;
             index++)
        {
            if (!char.IsDigit(
                    rawText[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool LooksLikeRealNumber(
        string rawText)
    {
        return rawText.Contains(
                   '.',
                   StringComparison.Ordinal) ||
               rawText.Contains(
                   'E',
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidBinaryEncoding(
        string encodedValue)
    {
        if (encodedValue.Length == 0)
        {
            return false;
        }

        if (encodedValue[0] is < '0' or > '3')
        {
            return false;
        }

        for (var index = 1;
             index < encodedValue.Length;
             index++)
        {
            if (!IsHexadecimalCharacter(
                    encodedValue[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsHexadecimalCharacter(
        char value)
    {
        return value is >= '0' and <= '9' or
               >= 'A' and <= 'F' or
               >= 'a' and <= 'f';
    }

    private static int ReadIdentifierEnd(
        string rawText)
    {
        if (rawText.Length == 0 ||
            !char.IsLetter(
                rawText[0]))
        {
            return 0;
        }

        var index =
            1;

        while (index <
               rawText.Length)
        {
            var character =
                rawText[index];

            if (!char.IsLetterOrDigit(
                    character) &&
                character != '_')
            {
                break;
            }

            index++;
        }

        return index;
    }

    private static bool IsValidIdentifier(
        string value)
    {
        if (value.Length == 0 ||
            !char.IsLetter(
                value[0]))
        {
            return false;
        }

        for (var index = 1;
             index < value.Length;
             index++)
        {
            var character =
                value[index];

            if (!char.IsLetterOrDigit(
                    character) &&
                character != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static int SkipWhitespace(
        string value,
        int startIndex)
    {
        var index =
            startIndex;

        while (index <
                   value.Length &&
               char.IsWhiteSpace(
                   value[index]))
        {
            index++;
        }

        return index;
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
             index <
             value.Length;
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

            if (character == '"' &&
                !insideString)
            {
                insideBinary =
                    true;

                continue;
            }

            if (character == '\'')
            {
                if (insideString &&
                    index + 1 <
                    value.Length &&
                    value[index + 1] ==
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
}