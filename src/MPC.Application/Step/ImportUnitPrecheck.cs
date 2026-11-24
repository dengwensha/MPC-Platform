using System.Globalization;
using MPC.Application.Inventory;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Diagnostics;

namespace MPC.Application.Step;

// Local rejection only. No evidence, normalization or evaluation permission is issued.
internal static class ImportUnitPrecheck
{
    internal const string MessagePrefix = "STEP unit/context precheck: ";
    internal static void EnsureCompatible(StepDataInventory inventory, string rootId)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootId);
        var check = new Check(inventory, rootId);
        try { check.Run(); }
        catch (FormatException ex)
        {
            throw new InvalidDataException(MessagePrefix + $"MalformedBinding; root {rootId}; source {check.CurrentSourceId}.", ex);
        }
    }

    private sealed class Check
    {
        private readonly string _root;
        private readonly Dictionary<string, StepDataEntityRecord[]> _records;
        private readonly Dictionary<string, List<StepDataEntityRecord>> _incoming = new(StringComparer.Ordinal);
        private readonly HashSet<string> _styleActive = new(StringComparer.Ordinal);
        private readonly HashSet<string> _styleDone = new(StringComparer.Ordinal);
        private HashSet<string> _rootDependencies = new(StringComparer.Ordinal);
        internal string CurrentSourceId { get; private set; }
        internal Check(StepDataInventory inventory, string root)
        {
            _root = root; CurrentSourceId = root;
            _records = inventory.Entities.Where(r => r.SourceEntityId is not null)
                .GroupBy(r => r.SourceEntityId!, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
            foreach (var record in inventory.Entities)
                foreach (string reference in record.ReferencedEntityIds.Distinct(StringComparer.Ordinal))
                {
                    if (!_incoming.TryGetValue(reference, out var list)) _incoming[reference] = list = new();
                    list.Add(record);
                }
        }
        private void Reject(string reason, params string[] ids) => throw new InvalidDataException(
            MessagePrefix + reason + "; root " + _root + "; source " + string.Join(",", ids) + ".");
        private StepDataEntityRecord Record(string id)
        {
            CurrentSourceId = id;
            if (!_records.TryGetValue(id, out var rows) || rows.Length != 1)
                Reject("UnresolvedOrDuplicateIdentity", id);
            var row = _records[id][0];
            if (row.ReadStatus != CadReadStatus.Complete) Reject("PartialBindingRecord", id);
            return row;
        }
        private IReadOnlyList<StepParameter> Simple(string id, string type, int count)
        {
            var row = Record(id);
            var parts = StepEvidenceSyntax.Components(row.RawStatement);
            if (row.IsComplexEntity || row.SourceEntityType != type || parts.Count != 1
                || !parts.TryGetValue(type, out var part) || part.Parameters.Count != count)
                Reject("UnsupportedRecordSignature", id);
            return parts[type].Parameters;
        }
        private string Ref(StepParameter p)
        {
            if (p is not StepEntityReferenceParameter reference) Reject("ExpectedReference", CurrentSourceId);
            string id = ((StepEntityReferenceParameter)p).EntityId;
            Record(id); return id;
        }
        private List<string> Refs(StepParameter p)
        {
            if (p is not StepAggregateParameter aggregate || aggregate.Items.Count == 0)
                Reject("ExpectedNonemptyReferenceList", CurrentSourceId);
            return ((StepAggregateParameter)p).Items.Select(Ref).ToList();
        }
        private void Text(StepParameter p, bool optional = false)
        {
            if (p is not StepStringParameter && !(optional && p is StepNullParameter))
                Reject("ExpectedText", CurrentSourceId);
        }
        private IEnumerable<StepDataEntityRecord> Incoming(string id) =>
            _incoming.TryGetValue(id, out var list) ? list : [];
        private void OnlyIncoming(string id, IEnumerable<string> admitted)
        {
            var allowed = admitted.ToHashSet(StringComparer.Ordinal);
            foreach (var r in Incoming(id))
                if (r.SourceEntityId is null || !allowed.Contains(r.SourceEntityId))
                    Reject(r.ComponentTypes.Contains("REPRESENTATION_RELATIONSHIP_WITH_TRANSFORMATION", StringComparer.Ordinal)
                        ? "TransformedRepresentationPath" : "UnreviewedIncomingPath", id, r.SourceEntityId ?? "<unknown>");
        }
        private static bool Shape(StepDataEntityRecord r) =>
            r.SourceEntityType is "SHAPE_REPRESENTATION" or "ADVANCED_BREP_SHAPE_REPRESENTATION";
        private (List<string> Items, string Context) Representation(string id)
        {
            var r = Record(id);
            if (!Shape(r)) Reject("UnsupportedRepresentation", id);
            var p = Simple(id, r.SourceEntityType, 3); Text(p[0]);
            return (Refs(p[1]), Ref(p[2]));
        }
        internal void Run()
        {
            Simple(_root, "MANIFOLD_SOLID_BREP", 2);
            _rootDependencies = Dependencies(_root);
            var shapes = Incoming(_root).Where(Shape).ToArray();
            if (shapes.Length != 1) Reject("AmbiguousDirectRepresentation", _root);
            string geometry = shapes[0].SourceEntityId!;
            var g = Representation(geometry);
            if (g.Items.Count(id => id == _root) != 1) Reject("DuplicateRootMembership", geometry);
            foreach (string item in g.Items.Where(id => id != _root)) Placement(item);
            Units(g.Context);
            var admittedRoot = new List<string> { geometry };
            foreach (var r in Incoming(_root).Where(r => r.SourceEntityType == "STYLED_ITEM"))
            { Styled(r.SourceEntityId!); admittedRoot.Add(r.SourceEntityId!); }
            foreach (var r in Incoming(_root).Where(r => r.SourceEntityType == "PRESENTATION_LAYER_ASSIGNMENT"))
            { Layer(r.SourceEntityId!); admittedRoot.Add(r.SourceEntityId!); }
            OnlyIncoming(_root, admittedRoot);

            var bridges = Incoming(geometry).Where(r => r.SourceEntityType == "SHAPE_REPRESENTATION_RELATIONSHIP").ToArray();
            if (bridges.Length > 1) Reject("MultipleBridges", geometry);
            string main = geometry; string? bridge = null;
            if (bridges.Length == 1)
            {
                bridge = bridges[0].SourceEntityId!;
                var p = Simple(bridge, "SHAPE_REPRESENTATION_RELATIONSHIP", 4);
                Text(p[0]); Text(p[1], true); string a = Ref(p[2]), b = Ref(p[3]);
                if (a == b || (a != geometry && b != geometry)) Reject("InvalidBridgeEndpoints", bridge);
                main = a == geometry ? b : a;
                if (Record(main).SourceEntityType != "SHAPE_REPRESENTATION") Reject("UnsupportedMainRepresentation", main);
                var m = Representation(main);
                foreach (string item in m.Items) Placement(item);
                Units(m.Context); // Both contexts must independently be mm + radian; IDs may differ.
            }
            var definitions = Incoming(geometry).Concat(main == geometry ? [] : Incoming(main))
                .Where(r => r.SourceEntityType == "SHAPE_DEFINITION_REPRESENTATION").Distinct().ToArray();
            if (definitions.Length != 1) Reject("AmbiguousProductBinding", geometry, main);
            string sdr = definitions[0].SourceEntityId!;
            var binding = Simple(sdr, "SHAPE_DEFINITION_REPRESENTATION", 2);
            string productShape = Ref(binding[0]), represented = Ref(binding[1]);
            if (represented != main) Reject("ProductBindingEndpoint", sdr, main);
            OnlyIncoming(geometry, main == geometry ? new[] { sdr } : new[] { bridge! });
            if (main != geometry) OnlyIncoming(main, new[] { sdr, bridge! });
            var ps = Simple(productShape, "PRODUCT_DEFINITION_SHAPE", 3);
            Text(ps[0]); Text(ps[1], true); string product = Ref(ps[2]);
            OnlyIncoming(productShape, new[] { sdr });
            var pd = Simple(product, "PRODUCT_DEFINITION", 4);
            Text(pd[0]); Text(pd[1], true); string formation = Ref(pd[2]), context = Ref(pd[3]);
            string formationType = Record(formation).SourceEntityType;
            if (formationType is not ("PRODUCT_DEFINITION_FORMATION" or "PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE"))
                Reject("UnsupportedProductFormation", formation);
            var fp = Simple(formation, formationType, formationType == "PRODUCT_DEFINITION_FORMATION" ? 3 : 4);
            Text(fp[0]); Text(fp[1], true); Ref(fp[2]);
            var pc = Simple(context, "PRODUCT_DEFINITION_CONTEXT", 3);
            Text(pc[0]); Ref(pc[1]); Text(pc[2]);
            OnlyIncoming(product, new[] { productShape }); // Includes assembly/occurrence and unknown product paths.
        }
        private HashSet<string> Dependencies(string root)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal); var pending = new Stack<string>(); pending.Push(root);
            while (pending.TryPop(out string? id))
                if (seen.Add(id) && _records.TryGetValue(id, out var rows) && rows.Length == 1)
                    foreach (string dependency in rows[0].ReferencedEntityIds) pending.Push(dependency);
            // Geometric dangling references remain the mapper's existing diagnostic.
            // This set only restricts T1 style targets; Ref() still resolves any styled item.
            return seen;
        }
        private void Placement(string id)
        {
            var p = Simple(id, "AXIS2_PLACEMENT_3D", 4); Text(p[0]);
            var point = Simple(Ref(p[1]), "CARTESIAN_POINT", 2); Text(point[0]);
            for (int i = 2; i < 4; i++)
                if (p[i] is not StepNullParameter) Simple(Ref(p[i]), "DIRECTION", 2);
        }
        private void Units(string id)
        {
            var parts = StepEvidenceSyntax.Components(Record(id).RawStatement);
            if (parts.Keys.Any(k => k is not ("GEOMETRIC_REPRESENTATION_CONTEXT" or "GLOBAL_UNIT_ASSIGNED_CONTEXT"
                or "GLOBAL_UNCERTAINTY_ASSIGNED_CONTEXT" or "REPRESENTATION_CONTEXT"))
                || !parts.TryGetValue("GEOMETRIC_REPRESENTATION_CONTEXT", out var geo) || geo.Parameters.Count != 1
                || geo.Parameters[0] is not StepIntegerParameter dimension || dimension.RawText != "3"
                || !parts.TryGetValue("REPRESENTATION_CONTEXT", out var rep) || rep.Parameters.Count != 2
                || rep.Parameters.Any(p => p is not StepStringParameter)
                || !parts.TryGetValue("GLOBAL_UNIT_ASSIGNED_CONTEXT", out var assignment) || assignment.Parameters.Count != 1)
                Reject("UnsupportedContext", id);
            int length = 0, angle = 0;
            foreach (string unitId in Refs(parts["GLOBAL_UNIT_ASSIGNED_CONTEXT"].Parameters[0]))
            {
                var unit = StepEvidenceSyntax.Components(Record(unitId).RawStatement);
                string[] dimensions = unit.Keys.Where(k => k is "LENGTH_UNIT" or "PLANE_ANGLE_UNIT" or "SOLID_ANGLE_UNIT").ToArray();
                if (dimensions.Length != 1 || unit.Count != 3 || !unit.TryGetValue("NAMED_UNIT", out var named)
                    || named.Parameters.Count != 1 || named.Parameters[0] is not StepOmittedParameter
                    || !unit.TryGetValue("SI_UNIT", out var si) || si.Parameters.Count != 2
                    || unit[dimensions[0]].Parameters.Count != 0)
                    Reject("UnsupportedUnitDefinition", id, unitId);
                string d = dimensions[0]; var values = unit["SI_UNIT"].Parameters;
                bool accepted = d switch {
                    "LENGTH_UNIT" => values[0] is StepEnumerationParameter && values[0].RawText == ".MILLI."
                        && values[1] is StepEnumerationParameter && values[1].RawText == ".METRE.",
                    "PLANE_ANGLE_UNIT" => values[0] is StepNullParameter && values[1] is StepEnumerationParameter && values[1].RawText == ".RADIAN.",
                    _ => values[0] is StepNullParameter && values[1] is StepEnumerationParameter && values[1].RawText == ".STERADIAN." };
                if (!accepted) Reject("ImportRequiresMillimetreRadian", id, unitId);
                if (d == "LENGTH_UNIT") length++;
                if (d == "PLANE_ANGLE_UNIT") angle++;
            }
            if (length != 1 || angle != 1) Reject("MissingOrDuplicateDimension", id);
        }
        private void Styled(string id)
        {
            var p = Simple(id, "STYLED_ITEM", 3); Text(p[0]);
            foreach (string assignment in Refs(p[1])) Style(assignment);
            string item = Ref(p[2]);
            if (!_rootDependencies.Contains(item)) Reject("ForeignPresentationGeometry", id, item);
            var admitted = new List<string>();
            foreach (var incoming in Incoming(id))
            {
                string source = incoming.SourceEntityId!;
                if (incoming.SourceEntityType == "PRESENTATION_LAYER_ASSIGNMENT") Layer(source);
                else if (incoming.SourceEntityType == "MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION")
                {
                    var r = Simple(source, incoming.SourceEntityType, 3); Text(r[0]); Ref(r[2]);
                    // Check every item, including styles for other faces of this root.
                    foreach (string styled in Refs(r[1]))
                    {
                        var other = Simple(styled, "STYLED_ITEM", 3); Text(other[0]);
                        if (!_rootDependencies.Contains(Ref(other[2]))) Reject("ForeignPresentationGeometry", source, styled);
                        foreach (string assignment in Refs(other[1])) Style(assignment);
                    }
                }
                else Reject("UnreviewedPresentationPath", id, source);
                admitted.Add(source);
            }
            OnlyIncoming(id, admitted);
        }
        private void Layer(string id)
        {
            var p = Simple(id, "PRESENTATION_LAYER_ASSIGNMENT", 3); Text(p[0]); Text(p[1]);
            foreach (string item in Refs(p[2]))
            {
                var r = Record(item);
                if (r.SourceEntityType == "STYLED_ITEM")
                {
                    var styled = Simple(item, "STYLED_ITEM", 3); Text(styled[0]);
                    if (!_rootDependencies.Contains(Ref(styled[2]))) Reject("ForeignLayerGeometry", id, item);
                    foreach (string assignment in Refs(styled[1])) Style(assignment);
                }
                else if (!_rootDependencies.Contains(item)) Reject("ForeignLayerGeometry", id, item);
            }
        }
        private void Style(string id)
        {
            if (_styleDone.Contains(id)) return;
            if (!_styleActive.Add(id)) Reject("PresentationCycle", id);
            string type = Record(id).SourceEntityType;
            var count = type switch {
                "PRESENTATION_STYLE_ASSIGNMENT" or "SURFACE_STYLE_FILL_AREA" => 1,
                "SURFACE_STYLE_USAGE" or "SURFACE_SIDE_STYLE" or "FILL_AREA_STYLE" or "FILL_AREA_STYLE_COLOUR" => 2,
                "COLOUR_RGB" => 4, _ => 0 };
            if (count == 0) Reject("UnsupportedPresentationStyle", id);
            var p = Simple(id, type, count);
            void Child(string child, string expected)
            { if (Record(child).SourceEntityType != expected) Reject("PresentationRoleMismatch", id, child); Style(child); }
            switch (type)
            {
                case "PRESENTATION_STYLE_ASSIGNMENT":
                    foreach (string child in Refs(p[0])) Child(child, "SURFACE_STYLE_USAGE"); break;
                case "SURFACE_STYLE_USAGE":
                    if (p[0] is not StepEnumerationParameter || p[0].RawText is not (".BOTH." or ".POSITIVE." or ".NEGATIVE.")) Reject("InvalidStyleSide", id);
                    Child(Ref(p[1]), "SURFACE_SIDE_STYLE"); break;
                case "SURFACE_SIDE_STYLE":
                    Text(p[0]); foreach (string child in Refs(p[1])) Child(child, "SURFACE_STYLE_FILL_AREA"); break;
                case "SURFACE_STYLE_FILL_AREA": Child(Ref(p[0]), "FILL_AREA_STYLE"); break;
                case "FILL_AREA_STYLE":
                    Text(p[0]); foreach (string child in Refs(p[1])) Child(child, "FILL_AREA_STYLE_COLOUR"); break;
                case "FILL_AREA_STYLE_COLOUR": Text(p[0]); Child(Ref(p[1]), "COLOUR_RGB"); break;
                case "COLOUR_RGB":
                    Text(p[0]);
                    foreach (var value in p.Skip(1))
                        if (!double.TryParse(value.RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                            || !double.IsFinite(number) || number < 0 || number > 1) Reject("InvalidColour", id);
                    break;
            }
            _styleActive.Remove(id); _styleDone.Add(id);
        }
    }
}
