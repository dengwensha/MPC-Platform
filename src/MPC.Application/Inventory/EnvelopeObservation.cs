using System.Text;

namespace MPC.Application.Inventory;

// Neutral structural data cannot issue bound envelope evidence or an eligibility decision.
internal sealed class EnvelopeData
{
    internal int DataSectionCount { get; }
    internal IReadOnlyList<string> Reasons { get; }
    internal EnvelopeData(int dataSectionCount, IEnumerable<string> reasons)
    {
        DataSectionCount = dataSectionCount;
        Reasons = Array.AsReadOnly(reasons.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }
}

internal sealed class EnvelopeObservation
{
    internal const int CurrentPolicyRevision = 1;
    internal int PolicyRevision => CurrentPolicyRevision;
    internal int DataSectionCount { get; }
    internal IReadOnlyList<string> Reasons { get; }
    private EnvelopeObservation(EnvelopeData data)
    {
        DataSectionCount = data.DataSectionCount; Reasons = data.Reasons;
    }

    internal static class EvidenceBinding
    {
        internal static EnvelopeObservation? Bind(InventoryEvidence? evidence,
            InventoryEvidence.ImportSession? expectedSession)
        {
            EnvelopeData? data = expectedSession?.ReadEnvelopeData(evidence);
            return data is null ? null : new EnvelopeObservation(data);
        }
    }
}

internal static class EnvelopeProducer
{
    internal static readonly IReadOnlyList<string> ReasonCodes = Array.AsReadOnly(new[] {
        "EnvelopeMalformed", "MultipleDataSections", "ParameterizedDataSection",
        "UnsupportedEnvelopeSection", "TrailingEnvelopeContent"
    });
    internal static string ReasonSeverity => "Error";

    internal static EnvelopeData ReadCaptured(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024, leaveOpen: true);
        return Read(reader);
    }

    // Full-file structural observation only. No entity strings/inventory are retained;
    // bounded prefixes identify statements while all lexical boundaries are visited.
    internal static EnvelopeData Read(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var reasons = new List<string>();
        int state = 0, dataCount = 0; // ISO -> HEADER -> records -> DATA -> entities -> terminator -> EOF
        void Statement(string prefix, bool truncated)
        {
            int end = 0;
            while (end < prefix.Length && (prefix[end] is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-')) end++;
            string keyword = prefix[..end];
            string suffix = prefix[end..].TrimStart();
            bool exact = !truncated && suffix.Length == 0;
            bool data = keyword == "DATA" && (exact || suffix.StartsWith('('));
            if (data)
            {
                dataCount++;
                if (dataCount > 1) reasons.Add("MultipleDataSections");
                if (!exact) reasons.Add("ParameterizedDataSection");
            }
            if (keyword is "ANCHOR" or "REFERENCE" or "SIGNATURE")
                reasons.Add("UnsupportedEnvelopeSection");
            if (state == 6) { reasons.Add("TrailingEnvelopeContent"); return; }
            switch (state)
            {
                case 0:
                    if (keyword == "ISO-10303-21" && exact) state = 1;
                    else reasons.Add("EnvelopeMalformed");
                    break;
                case 1:
                    if (keyword == "HEADER" && exact) state = 2;
                    else reasons.Add("EnvelopeMalformed");
                    break;
                case 2:
                    if (keyword == "ENDSEC" && exact) state = 3;
                    else if (keyword.Length == 0 || !suffix.StartsWith('(')) reasons.Add("EnvelopeMalformed");
                    break;
                case 3:
                    if (data) state = 4;
                    else reasons.Add("EnvelopeMalformed");
                    break;
                case 4:
                    if (keyword == "ENDSEC" && exact) state = 5;
                    else if (data) { /* Counted and rejected above; continue observing. */ }
                    else if (!prefix.StartsWith('#')) reasons.Add("EnvelopeMalformed");
                    break;
                case 5:
                    if (keyword == "END-ISO-10303-21" && exact) state = 6;
                    else if (data) state = 4; // Preserve observation of rejected later DATA sections.
                    else reasons.Add("EnvelopeMalformed");
                    break;
            }
        }
        try { Scan(reader, Statement); }
        catch (FormatException) { reasons.Add("EnvelopeMalformed"); }
        if (state != 6 || dataCount != 1) reasons.Add("EnvelopeMalformed");
        return new EnvelopeData(dataCount, reasons);
    }

    private static void Scan(TextReader reader, Action<string, bool> statement)
    {
        const int prefixLimit = 128;
        var prefix = new StringBuilder(prefixLimit);
        bool quoted = false, binary = false, comment = false, seen = false, truncated = false;
        int depth = 0;
        void Append(char c, bool separator = false)
        {
            if (separator)
            {
                if (prefix.Length == 0 || prefix[^1] == ' ') return;
                c = ' ';
            }
            if (prefix.Length < prefixLimit) prefix.Append(c);
            else truncated = true;
        }
        int next;
        while ((next = reader.Read()) != -1)
        {
            char c = (char)next;
            if (comment)
            {
                if (c == '*' && reader.Peek() == '/') { comment = false; reader.Read(); Append(' ', separator: true); }
                continue;
            }
            if (!quoted && !binary && c == '/' && reader.Peek() == '*')
            { comment = true; reader.Read(); Append(' ', separator: true); continue; }
            if (!quoted && !binary && char.IsWhiteSpace(c)) { Append(c, separator: true); continue; }
            if (!quoted && !binary && c == ';' && depth == 0)
            {
                if (!seen) throw new FormatException("Empty envelope statement.");
                statement(prefix.ToString().Trim(), truncated);
                prefix.Clear(); seen = truncated = false; continue;
            }
            seen = true; Append(c);
            if (!binary && c == '\'')
            {
                if (quoted && reader.Peek() == '\'') { Append((char)reader.Read()); continue; }
                quoted = !quoted; continue;
            }
            if (!quoted && c == '"') { binary = !binary; continue; }
            if (quoted || binary) continue;
            if (c == '(') depth++;
            else if (c == ')' && --depth < 0) throw new FormatException("Unbalanced envelope statement.");
        }
        if (seen || quoted || binary || comment || depth != 0)
            throw new FormatException("Incomplete envelope statement.");
    }
}
