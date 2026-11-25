using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MPC.Application.Step;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;

namespace MPC.Application.Inventory;

internal static partial class TargetProducer
{
    internal const string ContractSha256 = "F6C55BD1B7E84CE00683BBB2BC018DC3871EDD6FF2AD0BAFD1DCABFE7227BBB1";
    private static readonly JsonDocument Contract = LoadContract();
    private static readonly IReadOnlyDictionary<string, JsonElement> Rules = Contract.RootElement.GetProperty("rules")
        .EnumerateArray().ToDictionary(r => r.GetProperty("signature").GetString()!, StringComparer.Ordinal);
    // Bounded, immutable metadata from the hash-checked embedded contract only.
    // Never stores source-file values or a session/observation across captures.
    private static readonly IReadOnlyDictionary<string, JsonElement> Profiles = Contract.RootElement.GetProperty("profiles")
        .EnumerateObject().ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<string, (string Component, int Index, string Profile)[]> Fields = Rules
        .ToDictionary(r => r.Key, r => r.Value.GetProperty("components").EnumerateObject()
            .SelectMany(c => c.Value.GetProperty("fields").EnumerateArray()
                .Select(f => (c.Name, f.GetProperty("index").GetInt32(), f.GetProperty("constraint").GetString()!)))
            .ToArray(), StringComparer.Ordinal);
    internal static IReadOnlyList<string> ReasonCodes { get; } = Array.AsReadOnly(Contract.RootElement
        .GetProperty("proposed_reason_codes").EnumerateObject().Select(p => p.Name).ToArray());
    internal static string ReasonSeverity => "Error";
    private static JsonDocument LoadContract()
    {
        using var stream = typeof(TargetProducer).Assembly.GetManifestResourceStream("MPC.TargetContract.v1.json")
            ?? throw new InvalidOperationException("Missing approved target contract.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string text = reader.ReadToEnd().TrimStart('\uFEFF').Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != ContractSha256)
            throw new InvalidOperationException("Target contract changed without review.");
        return JsonDocument.Parse(text);
    }

    internal static TargetData Read(StepDataInventory? inventory) => new Check(inventory).Run();

    private sealed partial class Check
    {
        private sealed record Row(StepDataEntityRecord Record, Dictionary<string, StepTypedParameter> Parts,
            string Signature, string Role, JsonElement Rule, string[] References);
        private readonly StepDataInventory? _inventory;
        private readonly Dictionary<string, Row> _rows = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _incoming = new(StringComparer.Ordinal);
        private readonly HashSet<string> _bad = new(StringComparer.Ordinal);
        private readonly List<TargetReason> _reasons = new();
        private readonly Dictionary<string, (string Length, string Angle)?> _units = new(StringComparer.Ordinal);
        private string? _root, _geometry, _main, _context;
        private bool _guardCalled;
        private TargetLayerStatus _compatibility = TargetLayerStatus.Unavailable;
        internal Check(StepDataInventory? inventory) { _inventory = inventory; }
        private void Add(string layer, string code, string source = "", string component = "",
            int[]? indices = null, string[]? related = null, string? message = null,
            string? causeType = null, string? causeMessage = null) =>
            _reasons.Add(new(layer, code, source, component, indices, related, _reasons.Count, message, causeType, causeMessage));
        private void Invalid(string id, string code, string component = "", int[]? indices = null, string[]? related = null)
        { _bad.Add(id); Add("Records", code, id, component, indices, related); }
        private static bool Identity(string? id)
        {
            if (id is not { Length: > 1 } || id[0] != '#' || id[1] is < '1' or > '9') return false;
            for (int i = 2; i < id.Length; i++) if (id[i] is < '0' or > '9') return false;
            return true;
        }
        private static string Signature(Dictionary<string, StepTypedParameter> parts) => string.Join("|",
            parts.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Key + ":" + p.Value.Parameters.Count.ToString(CultureInfo.InvariantCulture)));
        private IReadOnlyList<StepParameter>? P(string? id, string component) => id is not null
            && _rows.TryGetValue(id, out var row) && row.Parts.TryGetValue(component, out var p) ? p.Parameters : null;
        private static string? Ref(StepParameter? p) => (p as StepEntityReferenceParameter)?.EntityId;
        private static string[] ReadReferences(Dictionary<string, StepTypedParameter> parts)
        {
            var references = new List<string>();
            void Append(StepParameter parameter)
            {
                if (parameter is StepEntityReferenceParameter reference) references.Add(reference.EntityId);
                else if (parameter is StepAggregateParameter aggregate) foreach (var child in aggregate.Items) Append(child);
                else if (parameter is StepTypedParameter typed) foreach (var child in typed.Parameters) Append(child);
            }
            foreach (var part in parts.Values) foreach (var parameter in part.Parameters) Append(parameter);
            return references.ToArray();
        }
        private IEnumerable<string> References(string id) => _rows[id].References;
        private static IReadOnlyList<StepParameter> Items(StepParameter? p) => (p as StepAggregateParameter)?.Items ?? [];
        private IEnumerable<string> Incoming(string id) => _incoming.TryGetValue(id, out var rows) ? rows : [];
        private void OnlyIncoming(string id, IEnumerable<string> allowed, string layer, string code)
        {
            var admitted = allowed.ToHashSet(StringComparer.Ordinal);
            foreach (string source in Incoming(id).Where(s => !admitted.Contains(s)).Order(StringComparer.Ordinal))
                Add(layer, code, id, related: [source]);
        }

        internal TargetData Run()
        {
            if (_inventory is null)
                return new(null, null, null, null, 0, false, TargetLayerStatus.Unavailable,
                    TargetLayerStatus.Unavailable, [new("Inventory", "TargetEvidenceUnavailable")]);
            var groups = _inventory.Entities.Select((r, i) => (Record: r, Index: i))
                .GroupBy(x => x.Record.SourceEntityId ?? "", StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal);
            foreach (var group in groups)
            {
                string id = group.Key;
                if (!Identity(id) || group.Count() != 1)
                { Invalid(id, "RecordIdentityInvalid"); continue; }
                var record = group.Single().Record;
                if (record.ReadStatus != CadReadStatus.Complete) Invalid(id, "RecordPartial");
                Dictionary<string, StepTypedParameter> parts;
                try { parts = StepEvidenceSyntax.Components(record.RawStatement); }
                catch (FormatException) { Invalid(id, "RecordPartial"); continue; }
                string signature = Signature(parts);
                bool known = Rules.TryGetValue(signature, out var rule);
                string role = known ? rule.GetProperty("role").GetString()! : "Unclassified";
                _rows.Add(id, new(record, parts, signature, role, rule, ReadReferences(parts)));
                if (!known || role == "Unclassified") Invalid(id, "UnclassifiedSignature");
                else if (!rule.GetProperty("admission").ValueEquals("conditional")) Invalid(id, "UnsupportedScope");
                if (!known && parts.Keys.Any(k => k is "BREP_WITH_VOIDS" or "FACETED_BREP" or "MAPPED_ITEM"
                    or "REPRESENTATION_MAP" or "SHELL_BASED_SURFACE_MODEL" or "FACE_BASED_SURFACE_MODEL"
                    or "GEOMETRIC_CURVE_SET" or "GEOMETRIC_SET")) Invalid(id, "UnsupportedScope");
                CompactParameters(parts);
            }
            // Include incomplete/unknown records in the reverse index as negative path evidence.
            foreach (var record in _inventory.Entities)
                foreach (string target in record.ReferencedEntityIds.Distinct(StringComparer.Ordinal))
                {
                    if (!_incoming.TryGetValue(target, out var list)) _incoming[target] = list = new();
                    list.Add(record.SourceEntityId ?? "");
                }
            foreach (var (id, row) in _rows)
            {
                foreach (string target in References(id).Distinct(StringComparer.Ordinal))
                    if (!Identity(target) || !_rows.ContainsKey(target)) Invalid(id, "ReferenceUnresolved", related: [target]);
                if (row.Role is "Unclassified" or "UnsupportedPath" or "UnsupportedGeometry") continue;
                ValidateTransientRow(id, row);
            }
            foreach (var (id, row) in _rows.Where(p => p.Value.Role == "Context")) Context(id);
            Bind();
            PresentationAndOwnership();
            var binding = _reasons.Any(r => r.Layer == "Binding") ? TargetLayerStatus.Blocked
                : _root is null || _geometry is null ? TargetLayerStatus.Unavailable : TargetLayerStatus.Observed;
            return new(_root, _geometry, _main, _context, _inventory.TotalEntityCount, _guardCalled, binding, _compatibility, _reasons);
        }

        // EXPERIMENT ONLY, Senior §44. Pass one keeps every name and the complete
        // signature/reference index, but only later-read component payloads. This
        // list is component-based even for unclassified/partial incoming sources.
        private static bool RetainParameters(string component) => component is
            "GLOBAL_UNIT_ASSIGNED_CONTEXT" or "GLOBAL_UNCERTAINTY_ASSIGNED_CONTEXT" or
            "SI_UNIT" or "UNCERTAINTY_MEASURE_WITH_UNIT" or
            "SHAPE_REPRESENTATION" or "ADVANCED_BREP_SHAPE_REPRESENTATION" or
            "SHAPE_REPRESENTATION_RELATIONSHIP" or "SHAPE_DEFINITION_REPRESENTATION" or
            "PRODUCT_DEFINITION_SHAPE" or "PRODUCT_DEFINITION" or
            "AXIS2_PLACEMENT_3D" or "STYLED_ITEM" or "PRESENTATION_LAYER_ASSIGNMENT" or
            "MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION";

        // Contract-name-only immutable placeholders; no file/session data cached.
        private static readonly IReadOnlyDictionary<string, StepTypedParameter> EmptyPayloads = Rules.Keys
            .SelectMany(s => s.Split('|')).Select(s => s[..s.IndexOf(':')]).Distinct(StringComparer.Ordinal)
            .ToDictionary(s => s, s => new StepTypedParameter(s + "()", s, []), StringComparer.Ordinal);

        private static void CompactParameters(Dictionary<string, StepTypedParameter> parts)
        {
            foreach (var (component, value) in parts)
                if (!RetainParameters(component) && value.Parameters.Count != 0)
                    parts[component] = EmptyPayloads.TryGetValue(component, out var empty) ? empty
                        : new StepTypedParameter(component + "()", component, []);
        }

        private void ValidateTransientRow(string id, Row compact)
        {
            // Same immutable RawStatement; no file reread. All forward signatures
            // exist before validation. Install just this row for existing P()/spline
            // reads, then restore even on failure. No transient tree escapes Check.
            var active = compact with { Parts = StepEvidenceSyntax.Components(compact.Record.RawStatement) };
            _rows[id] = active;
            try
            {
                foreach (var field in Fields[active.Signature])
                    Validate(id, field.Component, [field.Index], active.Parts[field.Component].Parameters[field.Index], field.Profile);
                Splines(id, active);
            }
            finally { _rows[id] = compact; }
        }

        // Executes every named profile and every nested member, accumulating paths.
        private void Validate(string id, string component, int[] path, StepParameter value, string profile)
        {
            var spec = Profiles[profile];
            bool valid = MatchesProfile(value, profile);
            if (!valid) Invalid(id, "FieldConstraintFailed", component, path);
            ValidateReferences(id, component, path, value, spec);
        }
        private static bool MatchesProfile(StepParameter value, string name)
        {
            if (!Matches(value, Profiles[name])) return false;
            if (name != "Direction3") return true;
            foreach (var item in Items(value)) if (Number(item, out double n) && n != 0) return true;
            return false;
        }
        private static bool Matches(StepParameter value, JsonElement spec)
        {
            if (spec.ValueKind == JsonValueKind.String)
                return MatchesProfile(value, spec.GetString()!);
            if (spec.TryGetProperty("one_of", out var choices))
            {
                foreach (var choice in choices.EnumerateArray()) if (Matches(value, choice)) return true;
                return false;
            }
            if (spec.TryGetProperty("base", out var basis)
                && !MatchesProfile(value, basis.GetString()!)) return false;
            if (spec.TryGetProperty("kind", out var kinds))
            {
                string kind = value is StepAggregateParameter ? "aggregate" : value.GetType().Name;
                bool found = false;
                if (kinds.ValueKind == JsonValueKind.Array)
                { foreach (var candidate in kinds.EnumerateArray()) if (candidate.ValueEquals(kind)) { found = true; break; } }
                else found = kinds.ValueEquals(kind);
                if (!found) return false;
            }
            if (spec.TryGetProperty("raw_literal", out var literal) && !literal.ValueEquals(value.RawText)) return false;
            if (spec.TryGetProperty("literals", out var literals))
            {
                bool found = false;
                foreach (var literalValue in literals.EnumerateArray()) if (literalValue.ValueEquals(value.RawText)) { found = true; break; }
                if (!found) return false;
            }
            if (spec.TryGetProperty("conversion", out _) && !Number(value, out _)) return false;
            if (spec.TryGetProperty("minimum_exclusive", out var min) && (!Number(value, out double n) || n <= min.GetDouble())) return false;
            if (spec.TryGetProperty("minimum_inclusive", out min) && (!Number(value, out double lower) || lower < min.GetDouble())) return false;
            if (spec.TryGetProperty("maximum_inclusive", out var max) && (!Number(value, out double upper) || upper > max.GetDouble())) return false;
            if (spec.TryGetProperty("range", out _) && value is not StepIntegerParameter { Value: > 0 }) return false;
            if (spec.TryGetProperty("type_name", out var type)
                && (value is not StepTypedParameter named || !type.ValueEquals(named.TypeName))) return false;
            if (spec.TryGetProperty("parameters", out var ps))
            {
                if (value is not StepTypedParameter typed || typed.Parameters.Count != ps.GetArrayLength()) return false;
                for (int i = 0; i < typed.Parameters.Count; i++) if (!Matches(typed.Parameters[i], ps[i])) return false;
            }
            if (spec.TryGetProperty("member", out var member))
            {
                if (value is not StepAggregateParameter a || a.Items.Count < spec.GetProperty("minimum_items").GetInt32()) return false;
                if (spec.TryGetProperty("maximum_items", out max) && max.ValueKind == JsonValueKind.Number && a.Items.Count > max.GetInt32()) return false;
                foreach (var item in a.Items) if (!Matches(item, member)) return false;
                if (spec.TryGetProperty("duplicates", out var duplicates) && duplicates.ValueEquals("forbidden")
                    && a.Items.Select(v => v is StepEntityReferenceParameter r ? r.EntityId : v.RawText).Distinct(StringComparer.Ordinal).Count() != a.Items.Count) return false;
                if (spec.GetProperty("ordering").ValueEquals("strictly increasing"))
                    for (int i = 1; i < a.Items.Count; i++)
                        if (!Number(a.Items[i - 1], out double previous) || !Number(a.Items[i], out double next) || next <= previous) return false;
            }
            return true;
        }
        private void ValidateReferences(string id, string component, int[] path, StepParameter value, JsonElement spec)
        {
            if (spec.ValueKind == JsonValueKind.String)
            { ValidateReferences(id, component, path, value, Profiles[spec.GetString()!]); return; }
            if (spec.TryGetProperty("base", out var basis))
                ValidateReferences(id, component, path, value, Profiles[basis.GetString()!]);
            if (spec.TryGetProperty("one_of", out var choices))
                foreach (var choice in choices.EnumerateArray())
                    if (Matches(value, choice)) ValidateReferences(id, component, path, value, choice);
            if (spec.TryGetProperty("target_exact_signatures", out var expected) && value is StepEntityReferenceParameter r)
            {
                if (!_rows.TryGetValue(r.EntityId, out var target)) Invalid(id, "ReferenceUnresolved", component, path, [r.EntityId]);
                else
                {
                    bool found = false;
                    foreach (var signature in expected.EnumerateArray()) if (signature.ValueEquals(target.Signature)) { found = true; break; }
                    if (!found) Invalid(id, "ReferenceRoleMismatch", component, path, [r.EntityId]);
                }
            }
            if (spec.TryGetProperty("member", out var member) && value is StepAggregateParameter a)
                for (int i = 0; i < a.Items.Count; i++)
                {
                    var child = Profiles[member.GetString()!];
                    if (!Matches(a.Items[i], child)) Invalid(id, "FieldConstraintFailed", component, [..path, i]);
                    ValidateReferences(id, component, [..path, i], a.Items[i], child);
                }
        }
        private static bool Number(StepParameter value, out double n)
        {
            n = value switch { StepIntegerParameter i => i.Value, StepRealParameter r => r.Value, _ => double.NaN };
            return double.IsFinite(n);
        }
        private void Splines(string id, Row row)
        {
            bool complexCurve = row.Parts.ContainsKey("B_SPLINE_CURVE"), complexSurface = row.Parts.ContainsKey("B_SPLINE_SURFACE");
            if (P(id, "B_SPLINE_CURVE_WITH_KNOTS") is { } knots)
            {
                var b = complexCurve ? P(id, "B_SPLINE_CURVE")! : knots.Skip(1).Take(5).ToArray();
                var k = complexCurve ? knots : knots.Skip(6).ToArray();
                if (!KnotCheck(b[0], Items(b[1]).Count, k[0], k[1])
                    || (P(id, "RATIONAL_B_SPLINE_CURVE") is { } w && Items(w[0]).Count != Items(b[1]).Count))
                    Invalid(id, "FieldConstraintFailed", "B_SPLINE_CURVE_WITH_KNOTS");
            }
            if (P(id, "B_SPLINE_SURFACE_WITH_KNOTS") is { } surface)
            {
                var b = complexSurface ? P(id, "B_SPLINE_SURFACE")! : surface.Skip(1).Take(7).ToArray();
                var k = complexSurface ? surface : surface.Skip(8).ToArray();
                var grid = Items(b[2]); int columns = grid.Count > 0 ? Items(grid[0]).Count : 0;
                bool valid = grid.Count >= 2 && columns >= 2 && grid.All(r => Items(r).Count == columns)
                    && KnotCheck(b[0], grid.Count, k[0], k[2]) && KnotCheck(b[1], columns, k[1], k[3]);
                if (P(id, "RATIONAL_B_SPLINE_SURFACE") is { } w)
                    valid &= Items(w[0]).Count == grid.Count && Items(w[0]).All(r => Items(r).Count == columns);
                if (!valid) Invalid(id, "FieldConstraintFailed", "B_SPLINE_SURFACE_WITH_KNOTS");
            }
        }
        private static bool KnotCheck(StepParameter degree, int count, StepParameter multiplicities, StepParameter knots)
        {
            if (degree is not StepIntegerParameter { Value: > 0 } d || d.Value >= count) return false;
            var m = Items(multiplicities); var k = Items(knots);
            if (k.Count < 2 || k.Count != m.Count) return false;
            decimal sum = 0;
            for (int i = 0; i < m.Count; i++)
            {
                if (m[i] is not StepIntegerParameter { Value: > 0 } n || n.Value > d.Value + (i == 0 || i == m.Count - 1 ? 1 : 0)) return false;
                sum += n.Value;
                if (!Number(k[i], out double current) || (i > 0 && (!Number(k[i - 1], out double previous) || current <= previous))) return false;
            }
            return sum == count + (decimal)d.Value + 1;
        }
    }
}
