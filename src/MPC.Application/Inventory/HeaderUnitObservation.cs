using System.Text;
using System.Text.RegularExpressions;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Application.Inventory;

// Neutral, immutable parser data. These types may be constructed by callers but
// cannot issue bound observations, evidence or decisions. No geometry normalization.
internal sealed record ExactUnitScale(long Numerator, long Denominator);
internal sealed record UnitDefinitionData(string SourceId, string Dimension,
    string Code, ExactUnitScale? Scale);
internal sealed class ContextUnitData
{
    internal string SourceId { get; }
    internal IReadOnlyList<UnitDefinitionData> Units { get; }
    internal IReadOnlyList<string> Reasons { get; }
    internal ContextUnitData(string id, IEnumerable<UnitDefinitionData> units, IEnumerable<string> reasons)
    {
        SourceId = id; Units = Array.AsReadOnly(units.ToArray());
        Reasons = Array.AsReadOnly(reasons.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }
}
internal sealed class HeaderUnitData
{
    internal string? Schema { get; }
    internal string? ImplementationLevel { get; }
    internal string? RawSchema { get; }
    internal string? RawImplementationLevel { get; }
    internal string? RecognizedSchema { get; }
    internal IReadOnlyList<string> SchemaOid { get; }
    internal IReadOnlyList<string> HeaderReasons { get; }
    internal IReadOnlyList<ContextUnitData> Contexts { get; }
    internal IReadOnlyList<string> ContextSetReasons { get; }
    internal HeaderUnitData(string? schema, string? level, IEnumerable<string> reasons,
        IEnumerable<ContextUnitData> contexts, IEnumerable<string> setReasons,
        string? rawSchema = null, string? rawImplementationLevel = null,
        string? recognizedSchema = null, IEnumerable<string>? schemaOid = null)
    {
        Schema = schema; ImplementationLevel = level;
        RawSchema = rawSchema; RawImplementationLevel = rawImplementationLevel;
        RecognizedSchema = recognizedSchema; SchemaOid = Array.AsReadOnly((schemaOid ?? []).ToArray());
        HeaderReasons = Array.AsReadOnly(reasons.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
        Contexts = Array.AsReadOnly(contexts.OrderBy(c => c.SourceId, StringComparer.Ordinal).ToArray());
        ContextSetReasons = Array.AsReadOnly(setReasons.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
    }
}

// Bound observation: neutral parser data cannot mint this type. The only binder
// accepts real immutable importer evidence AND its owner, never caller metadata.
internal sealed class HeaderUnitObservation
{
    internal const int HeaderPolicyRevision = 2;
    internal const int UnitPolicyRevision = 1;
    internal string? Schema { get; }
    internal string? ImplementationLevel { get; }
    internal string? RawSchema { get; }
    internal string? RawImplementationLevel { get; }
    internal string? RecognizedSchema { get; }
    internal IReadOnlyList<string> SchemaOid { get; }
    internal IReadOnlyList<string> HeaderReasons { get; }
    internal IReadOnlyList<ContextUnitData> Contexts { get; }
    internal IReadOnlyList<string> ContextSetReasons { get; }

    private HeaderUnitObservation(HeaderUnitData data)
    {
        Schema = data.Schema; ImplementationLevel = data.ImplementationLevel;
        RawSchema = data.RawSchema; RawImplementationLevel = data.RawImplementationLevel;
        RecognizedSchema = data.RecognizedSchema; SchemaOid = data.SchemaOid;
        HeaderReasons = data.HeaderReasons; Contexts = data.Contexts;
        ContextSetReasons = data.ContextSetReasons;
    }

    internal static class EvidenceBinding
    {
        internal static HeaderUnitObservation? Bind(InventoryEvidence? evidence,
            InventoryEvidence.ImportSession? expectedSession)
        {
            HeaderUnitData? data = expectedSession?.ReadHeaderUnitData(evidence);
            return data is null ? null : new HeaderUnitObservation(data);
        }
    }
}

internal static class HeaderUnitProducer
{
    // All codes are blocking/unavailable observation reasons with Error severity.
    // They do not modify connector diagnostics, the inventory manifest or policy A.
    internal static readonly IReadOnlyList<string> ReasonCodes = Array.AsReadOnly(new[] {
        "HeaderMalformed", "UnsupportedSchema", "UnsupportedSchemaForm", "UnsupportedImplementationLevel",
        "InventoryUnavailable", "NoGeometricContext", "ContextIdentityInvalid", "ContextMalformed",
        "UnitAssignmentsMissing", "UnitReferenceInvalid", "UnitDefinitionMalformed", "BlockedConversionUnit",
        "UnsupportedUnit", "MissingLengthUnit", "MissingAngleUnit", "AmbiguousLengthUnit", "AmbiguousAngleUnit"
    });
    internal static string ReasonSeverity => "Error";

    internal static HeaderUnitData ReadCaptured(Stream stream, StepDataInventory? inventory)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024, leaveOpen: true);
        string? schema = null, level = null, rawSchema = null, rawLevel = null, recognizedSchema = null;
        IReadOnlyList<string> oid = Array.Empty<string>();
        var reasons = new List<string>();
        // Decode/scan only through the header boundary. StreamReader may buffer
        // ahead, but DATA is neither split nor retained as a second string here.
        // IO failures propagate; never classify as a clean header.
        try { ReadHeader(StepEvidenceSyntax.HeaderStatements(reader), reasons, out schema, out level,
            out rawSchema, out rawLevel, out recognizedSchema, out oid); }
        catch (FormatException) { reasons.Add("HeaderMalformed"); }
        var contexts = new List<ContextUnitData>();
        var setReasons = new List<string>();
        if (inventory is null) setReasons.Add("InventoryUnavailable");
        else
        {
            var groups = inventory.Entities.Where(e => e.SourceEntityId is not null)
                .GroupBy(e => e.SourceEntityId!, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
            foreach (var entity in inventory.Entities.Where(e => e.ComponentTypes.Contains("GEOMETRIC_REPRESENTATION_CONTEXT", StringComparer.Ordinal)
                || e.SourceEntityType == "GEOMETRIC_REPRESENTATION_CONTEXT"))
            {
                string id = entity.SourceEntityId ?? "";
                if (id.Length == 0 || !groups.TryGetValue(id, out var same) || same.Length != 1)
                { setReasons.Add("ContextIdentityInvalid"); contexts.Add(new(id, [], ["ContextIdentityInvalid"])); continue; }
                contexts.Add(ReadContext(entity, groups));
            }
            if (contexts.Count == 0) setReasons.Add("NoGeometricContext");
        }
        return new(schema, level, reasons, contexts, setReasons, rawSchema, rawLevel, recognizedSchema, oid);
    }

    private static IReadOnlyList<StepParameter> Parameters(string text) => new StepParameterParser().Parse(text);
    private static StepTypedParameter Typed(string text)
    {
        var p = Parameters(text);
        if (p.Count != 1 || p[0] is not StepTypedParameter t) throw new FormatException("Expected one named record.");
        return t;
    }
    private static void ReadHeader(List<string> statements, List<string> reasons, out string? schema, out string? level,
        out string? rawSchema, out string? rawLevel, out string? recognizedSchema, out IReadOnlyList<string> oid)
    {
        schema = level = rawSchema = rawLevel = recognizedSchema = null;
        oid = Array.Empty<string>();
        // Scan structural statements, not name matches inside quoted strings/comments.
        if (statements.Count < 3 || statements[0] != "ISO-10303-21" || statements[1] != "HEADER")
            throw new FormatException("Missing exchange/header section.");
        int end = statements.IndexOf("ENDSEC", 2);
        if (end < 0) throw new FormatException("Unterminated header.");
        var records = statements.Skip(2).Take(end - 2).Select(Typed).ToArray();
        var required = new[] { "FILE_DESCRIPTION", "FILE_NAME", "FILE_SCHEMA" };
        if (records.Length != 3 || required.Any(n => records.Count(r => r.TypeName == n) != 1))
            throw new FormatException("Header records must occur exactly once.");
        var description = records.Single(r => r.TypeName == "FILE_DESCRIPTION").Parameters;
        var name = records.Single(r => r.TypeName == "FILE_NAME").Parameters;
        var schemas = records.Single(r => r.TypeName == "FILE_SCHEMA").Parameters;
        if (description.Count != 2 || description[0] is not StepAggregateParameter ds || ds.Items.Count == 0
            || ds.Items.Any(p => p is not StepStringParameter s || string.IsNullOrWhiteSpace(s.Value))
            || description[1] is not StepStringParameter impl || string.IsNullOrWhiteSpace(impl.Value)
            || name.Count != 7 || name[0] is not StepStringParameter || name[1] is not StepStringParameter
            || name[2] is not StepAggregateParameter authors || authors.Items.Any(p => p is not StepStringParameter)
            || name[3] is not StepAggregateParameter orgs || orgs.Items.Any(p => p is not StepStringParameter)
            || name.Skip(4).Any(p => p is not StepStringParameter)
            || schemas.Count != 1 || schemas[0] is not StepAggregateParameter ss || ss.Items.Count == 0
            || ss.Items.Any(p => p is not StepStringParameter s || string.IsNullOrWhiteSpace(s.Value)))
            throw new FormatException("Malformed required header field.");
        rawLevel = impl.RawText;
        level = impl.Value.Trim();
        if (level is not ("1" or "2;1")) reasons.Add("UnsupportedImplementationLevel");
        if (ss.Items.Count != 1) { reasons.Add("UnsupportedSchema"); return; }
        var item = (StepStringParameter)ss.Items[0];
        rawSchema = item.RawText;
        schema = item.Value;
        // Match the original literal, not a decoded/trimmed/case-folded alias.
        if (rawSchema.Length < 3 || rawSchema[0] != '\'' || rawSchema[^1] != '\'')
        { reasons.Add("UnsupportedSchemaForm"); return; }
        string declared = rawSchema[1..^1];
        if (declared.All(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_'))
        {
            if (declared == "AUTOMOTIVE_DESIGN") recognizedSchema = declared;
            else reasons.Add("UnsupportedSchema");
            return;
        }
        var match = QualifiedSchema.Match(declared);
        if (!match.Success) { reasons.Add("UnsupportedSchemaForm"); return; }
        string[] arcs = declared[(declared.IndexOf('{') + 1)..^1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        oid = Array.AsReadOnly(arcs);
        if (match.Groups["name"].Value == "AUTOMOTIVE_DESIGN"
            && arcs.SequenceEqual(new[] { "1", "0", "10303", "214", "3", "1", "1" }, StringComparer.Ordinal))
            recognizedSchema = "AUTOMOTIVE_DESIGN";
        else reasons.Add("UnsupportedSchema");
    }

    // Seven canonical decimal tokens, compared as strings: no integer overflow,
    // ignored suffix, broad whitespace or permissive ASN.1 normalization.
    private static readonly Regex QualifiedSchema = new(
        @"\A(?<name>[A-Z0-9_]+)[ ]+\{[ ]*(?<arc>0|[1-9][0-9]*)(?:[ ]+(?<arc>0|[1-9][0-9]*)){6}[ ]*\}\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static ContextUnitData ReadContext(StepDataEntityRecord entity,
        Dictionary<string, StepDataEntityRecord[]> entities)
    {
        var reasons = new List<string>(); var units = new List<UnitDefinitionData>();
        try
        {
            var components = StepEvidenceSyntax.Components(entity.RawStatement);
            if (!components.TryGetValue("GEOMETRIC_REPRESENTATION_CONTEXT", out var geometry)
                || geometry.Parameters.Count != 1 || geometry.Parameters[0] is not StepIntegerParameter dimension
                || dimension.RawText != "3" || !components.TryGetValue("REPRESENTATION_CONTEXT", out var representation)
                || representation.Parameters.Count != 2 || representation.Parameters.Any(p => p is not StepStringParameter))
                throw new FormatException("Unsupported/malformed geometric context.");
            if (!components.TryGetValue("GLOBAL_UNIT_ASSIGNED_CONTEXT", out var assignment))
                reasons.Add("UnitAssignmentsMissing");
            else
            {
                if (assignment.Parameters.Count != 1 || assignment.Parameters[0] is not StepAggregateParameter refs || refs.Items.Count == 0)
                    throw new FormatException("Invalid unit list.");
                foreach (var item in refs.Items)
                {
                    if (item is not StepEntityReferenceParameter reference || !entities.TryGetValue(reference.EntityId, out var candidates) || candidates.Length != 1)
                    { reasons.Add("UnitReferenceInvalid"); continue; }
                    UnitDefinitionData unit = ReadUnit(candidates[0]); units.Add(unit);
                    if (unit.Code != "SupportedUnit" && unit.Code != "OtherDimension") reasons.Add(unit.Code);
                }
            }
        }
        catch (FormatException) { reasons.Add("ContextMalformed"); }
        int length = units.Count(u => u.Dimension == "Length"), angle = units.Count(u => u.Dimension == "Angle");
        if (length != 1) reasons.Add(length == 0 ? "MissingLengthUnit" : "AmbiguousLengthUnit");
        if (angle != 1) reasons.Add(angle == 0 ? "MissingAngleUnit" : "AmbiguousAngleUnit");
        return new(entity.SourceEntityId!, units, reasons);
    }
    private static UnitDefinitionData ReadUnit(StepDataEntityRecord entity)
    {
        string id = entity.SourceEntityId!;
        try
        {
            var c = StepEvidenceSyntax.Components(entity.RawStatement);
            bool length = c.ContainsKey("LENGTH_UNIT"), angle = c.ContainsKey("PLANE_ANGLE_UNIT");
            string dim = length ? "Length" : angle ? "Angle" : "Other";
            if (length && angle) return new(id, "Unknown", "UnitDefinitionMalformed", null);
            if (c.ContainsKey("CONVERSION_BASED_UNIT")) return new(id, dim, "BlockedConversionUnit", null);
            // Only the reviewed direct SI forms. Other dimensions are preserved;
            // unclassified definitions do not silently disappear.
            if (!length && !angle)
                return new(id, dim, c.ContainsKey("SOLID_ANGLE_UNIT") && c.ContainsKey("SI_UNIT") ? "OtherDimension" : "UnsupportedUnit", null);
            var dimension = c[length ? "LENGTH_UNIT" : "PLANE_ANGLE_UNIT"];
            if (dimension.Parameters.Count != 0 || c.Keys.Any(k => k is not ("LENGTH_UNIT" or "PLANE_ANGLE_UNIT" or "NAMED_UNIT" or "SI_UNIT"))
                || !c.TryGetValue("NAMED_UNIT", out var named) || named.Parameters.Count != 1 || named.Parameters[0] is not StepOmittedParameter
                || !c.TryGetValue("SI_UNIT", out var si) || si.Parameters.Count != 2)
                return new(id, dim, "UnitDefinitionMalformed", null);
            string prefix = si.Parameters[0] is StepNullParameter ? "$" : si.Parameters[0].RawText;
            string unitName = si.Parameters[1].RawText;
            ExactUnitScale? scale = (dim, prefix, unitName) switch {
                ("Length", "$", ".METRE.") => new(1, 1),
                ("Length", ".CENTI.", ".METRE.") => new(1, 100),
                ("Length", ".MILLI.", ".METRE.") => new(1, 1000),
                ("Angle", "$", ".RADIAN.") => new(1, 1), _ => null };
            return new(id, dim, scale is null ? "UnsupportedUnit" : "SupportedUnit", scale);
        }
        catch (FormatException) { return new(id, "Unknown", "UnitDefinitionMalformed", null); }
    }
}

// Structural boundary scanner only; parameters use the existing connector parser.
// Quotes, doubled quotes, binary literals and comments protect separators.
internal static class StepEvidenceSyntax
{
    internal static List<string> HeaderStatements(TextReader reader) => Scan(reader, ';', false, stopAtEndsec: true);
    private static List<string> Split(string text, char separator, bool component)
    {
        using var reader = new StringReader(text);
        return Scan(reader, separator, component, stopAtEndsec: false);
    }
    private static List<string> Scan(TextReader reader, char separator, bool component, bool stopAtEndsec)
    {
        var output = new List<string>(); var current = new StringBuilder();
        bool quoted = false, binary = false, comment = false; int depth = 0;
        int next;
        while ((next = reader.Read()) != -1)
        {
            char c = (char)next;
            if (comment) { if (c == '*' && reader.Peek() == '/') { comment = false; reader.Read(); current.Append(' '); } continue; }
            if (!quoted && !binary && c == '/' && reader.Peek() == '*') { comment = true; reader.Read(); continue; }
            current.Append(c);
            if (!binary && c == '\'')
            {
                if (quoted && reader.Peek() == '\'') { current.Append((char)reader.Read()); continue; }
                quoted = !quoted; continue;
            }
            if (!quoted && c == '"') { binary = !binary; continue; }
            if (quoted || binary) continue;
            if (c == '(') depth++;
            if (c == ')' && --depth < 0) throw new FormatException("Unbalanced statement.");
            bool boundary = component ? c == ')' && depth == 0 : c == separator && depth == 0;
            if (boundary)
            {
                string value = current.ToString().Trim();
                if (!component) value = value[..^1].Trim();
                if (value.Length == 0) throw new FormatException("Empty statement.");
                output.Add(value); current.Clear();
                if (stopAtEndsec && value == "ENDSEC") return output;
            }
        }
        if (quoted || binary || comment || depth != 0 || current.ToString().Trim().Length != 0)
            throw new FormatException("Incomplete structural statement.");
        return output;
    }
    internal static Dictionary<string, StepTypedParameter> Components(string statement)
    {
        int equals = statement.IndexOf('=');
        if (equals < 0) throw new FormatException("Missing assignment.");
        string body = statement[(equals + 1)..].Trim().TrimEnd(';').Trim();
        if (body.StartsWith('(') && (body.Length < 2 || !body.EndsWith(')')))
            throw new FormatException("Incomplete complex body.");
        var result = new Dictionary<string, StepTypedParameter>(StringComparer.Ordinal);
        var parser = new StepParameterParser();
        foreach (string part in body.StartsWith('(') ? Split(body[1..^1], '\0', true) : new List<string> { body })
        {
            var parsed = parser.Parse(part);
            if (parsed.Count != 1 || parsed[0] is not StepTypedParameter typed || !result.TryAdd(typed.TypeName, typed))
                throw new FormatException("Malformed or duplicate component.");
        }
        return result;
    }
}
