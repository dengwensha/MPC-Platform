using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Connectors.Step.Semantics;

internal static class StepComplexComponentParameters
{
    // Split only component boundaries; all parameter grammar stays in StepParameterParser.
    internal static IReadOnlyList<StepParameter> Read(string text, StepParameterParser parser)
    {
        var components = new List<StepParameter>();
        int position = 0;
        while (position < text.Length)
        {
            while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
            if (position == text.Length) break;
            int start = position;
            int depth = 0;
            bool inString = false;
            bool inBinary = false;
            bool complete = false;
            for (; position < text.Length; position++)
            {
                char value = text[position];
                if (inBinary)
                {
                    if (value == '"') inBinary = false;
                    continue;
                }
                if (!inString && value == '"') { inBinary = true; continue; }
                if (value == '\'')
                {
                    if (inString && position + 1 < text.Length && text[position + 1] == '\'')
                    { position++; continue; }
                    inString = !inString;
                    continue;
                }
                if (inString) continue;
                if (value == '(') depth++;
                if (value != ')') continue;
                depth--;
                if (depth < 0) throw new FormatException("Unexpected component closing parenthesis.");
                if (depth == 0) { position++; complete = true; break; }
            }
            if (!complete || inString || inBinary)
                throw new FormatException("Incomplete complex component.");
            IReadOnlyList<StepParameter> parsed = parser.Parse(text[start..position]);
            if (parsed.Count != 1 || parsed[0] is not StepTypedParameter)
                throw new FormatException("Expected a named complex component.");
            components.Add(parsed[0]);
            while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
            // Preserve the connector's existing acceptance of comma-separated components.
            if (position < text.Length && text[position] == ',')
            {
                position++;
                while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
                if (position == text.Length) throw new FormatException("Trailing component separator.");
            }
        }
        return components.AsReadOnly();
    }
}
