using MPC.Application.Step;
using MPC.Connectors.Step.Models;

namespace MPC.Application.Inventory;

internal static partial class TargetProducer
{
    private sealed partial class Check
    {
        private (string Length, string Angle)? Context(string id)
        {
            if (_units.TryGetValue(id, out var cached)) return cached;
            string? length = null, angle = null; int lengths = 0, angles = 0, solids = 0;
            bool valid = !_bad.Contains(id) && P(id, "GLOBAL_UNIT_ASSIGNED_CONTEXT") is { Count: 1 };
            var assignments = P(id, "GLOBAL_UNIT_ASSIGNED_CONTEXT");
            foreach (string unit in assignments is null ? [] : Items(assignments[0]).Select(Ref).OfType<string>())
            {
                if (!_rows.TryGetValue(unit, out var row) || _bad.Contains(unit)) { valid = false; continue; }
                if (row.Parts.ContainsKey("LENGTH_UNIT")) { lengths++; length = Length(unit); valid &= length is not null; }
                else if (row.Parts.ContainsKey("PLANE_ANGLE_UNIT")) { angles++; angle = "RADIAN:1/1"; }
                else if (row.Parts.ContainsKey("SOLID_ANGLE_UNIT")) solids++;
                else valid = false;
            }
            valid &= lengths == 1 && angles == 1 && solids <= 1;
            if (valid && P(id, "GLOBAL_UNCERTAINTY_ASSIGNED_CONTEXT") is { } uncertainties)
                foreach (string uncertainty in Items(uncertainties[0]).Select(Ref).OfType<string>())
                {
                    var u = P(uncertainty, "UNCERTAINTY_MEASURE_WITH_UNIT");
                    if (u is null || _bad.Contains(uncertainty) || Ref(u[1]) is not { } unit || Length(unit) != length)
                    { valid = false; Add("Binding", "ContextUnitsUnsupported", id, related: [uncertainty]); }
                }
            (string Length, string Angle)? result = valid && length is not null && angle is not null ? (length, angle) : null;
            _units[id] = result;
            if (result is null) Add("Binding", "ContextUnitsUnsupported", id);
            return result;
        }
        private string? Length(string id)
        {
            if (_bad.Contains(id) || !_rows.TryGetValue(id, out var row) || !row.Parts.ContainsKey("LENGTH_UNIT")) return null;
            var si = P(id, "SI_UNIT");
            if (si is not { Count: 2 } || si[1].RawText != ".METRE.") return null;
            return si[0].RawText switch { "$" => "METRE:1/1", ".CENTI." => "METRE:1/100", ".MILLI." => "METRE:1/1000", _ => null };
        }
        private void Bind()
        {
            var roots = _rows.Where(p => p.Value.Role == "TargetRoot").Select(p => p.Key).ToArray();
            if (roots.Length == 1) _root = roots[0];
            else Add("Binding", "RootBindingInvalid", related: roots);

            // A1 is a direct call on the SAME retained object. No copied-policy substitute.
            if (_root is not null)
            {
                _guardCalled = true;
                try { ImportUnitPrecheck.EnsureCompatible(_inventory!, _root); _compatibility = TargetLayerStatus.Observed; }
                catch (InvalidDataException ex)
                {
                    _compatibility = TargetLayerStatus.Blocked;
                    Add("Compatibility", "CurrentImportIncompatible", _root, message: ex.Message,
                        causeType: ex.InnerException?.GetType().FullName, causeMessage: ex.InnerException?.Message);
                }
            }
            else Add("Compatibility", "DependencyUnavailable");
            if (_root is null) return;
            var shapes = _rows.Where(p => p.Value.Role == "ShapeRepresentation").Select(p => p.Key).ToArray();
            IReadOnlyList<StepParameter>? Shape(string id) => P(id, _rows[id].Record.SourceEntityType);
            var members = shapes.Where(id => Shape(id) is { } p && Items(p[1]).Any(v => Ref(v) == _root)).ToArray();
            if (members.Length != 1)
            { Add("Binding", "RootBindingInvalid", _root, related: members); return; }
            _geometry = members[0]; _main = _geometry;
            var geometry = Shape(_geometry)!;
            _context = Ref(geometry[2]);
            if (Items(geometry[1]).Count(v => Ref(v) == _root) != 1)
                Add("Binding", "RootBindingInvalid", _geometry);
            CheckPlacementItems(_geometry, geometry, _root);
            var bridges = _rows.Where(p => p.Value.Role == "NontransformingBridge"
                && References(p.Key).Contains(_geometry, StringComparer.Ordinal)).Select(p => p.Key).ToArray();
            string? bridge = null;
            if (bridges.Length > 1) Add("Binding", "RootBindingInvalid", _geometry, related: bridges);
            else if (bridges.Length == 1)
            {
                bridge = bridges[0]; var p = P(bridge, "SHAPE_REPRESENTATION_RELATIONSHIP")!;
                string? a = Ref(p[2]), b = Ref(p[3]);
                _main = a == _geometry ? b : a;
                if (a == b || a is null || b is null || (a != _geometry && b != _geometry)
                    || _main is null || P(_main, "SHAPE_REPRESENTATION") is not { } main)
                { Add("Binding", "RootBindingInvalid", bridge); _main = null; }
                else
                {
                    CheckPlacementItems(_main, main, null);
                    string? context = Ref(main[2]);
                    if (_context is null || context is null || Context(_context) is not { } u || Context(context) is not { } v)
                        Add("Binding", "ContextBindingUnavailable", bridge);
                    else if (u != v) Add("Binding", "BridgeUnitMismatch", bridge, related: [_context, context]);
                }
            }
            if (_context is null || Context(_context) is null) Add("Binding", "ContextBindingUnavailable", _geometry);
            var definitions = _rows.Where(p => p.Value.Role == "ShapeBinding"
                && P(p.Key, "SHAPE_DEFINITION_REPRESENTATION") is { } d
                && (Ref(d[1]) == _geometry || (_main is not null && Ref(d[1]) == _main))).Select(p => p.Key).ToArray();
            if (definitions.Length != 1) Add("Binding", "ProductBindingInvalid", _geometry, related: definitions);
            else
            {
                string sdr = definitions[0]; var d = P(sdr, "SHAPE_DEFINITION_REPRESENTATION")!;
                if (Ref(d[1]) != _main) Add("Binding", "ProductBindingInvalid", sdr);
                if (Ref(d[0]) is { } ps && P(ps, "PRODUCT_DEFINITION_SHAPE") is { } productShape)
                {
                    OnlyIncoming(ps, [sdr], "Binding", "ProductBindingInvalid");
                    if (Ref(productShape[2]) is { } pd && P(pd, "PRODUCT_DEFINITION") is not null)
                        OnlyIncoming(pd, [ps], "Binding", "ProductBindingInvalid");
                    else Add("Binding", "ProductBindingInvalid", ps);
                }
                else Add("Binding", "ProductBindingInvalid", sdr);
            }
            OnlyIncoming(_geometry, _main == _geometry ? definitions : bridge is null ? [] : [bridge], "Binding", "RootBindingInvalid");
            if (_main is not null && _main != _geometry)
                OnlyIncoming(_main, [..definitions, ..bridge is null ? Array.Empty<string>() : new[] { bridge }], "Binding", "RootBindingInvalid");
            foreach (string id in shapes.Where(s => s != _geometry && s != _main)) Add("Binding", "RootBindingInvalid", id);
            // A parsed reference alone cannot establish a clean product/context tail.
            // Field/type failures in a required binding dependency block that layer,
            // while the original precise Records finding remains intact.
            var pending = new Stack<string>(new[] { _root, _geometry, _main, _context, bridge }.OfType<string>().Concat(definitions));
            var seen = new HashSet<string>(StringComparer.Ordinal);
            while (pending.TryPop(out string? id))
            {
                if (!_rows.TryGetValue(id, out var row) || !seen.Add(id)) continue;
                if (_bad.Contains(id)) Add("Binding", "DependencyUnavailable", id);
                foreach (string target in References(id))
                    if (_rows.TryGetValue(target, out var child) && (child.Role is "ProductMetadata" or "Context" or "UnitMetadata"
                        or "ShapeBinding" or "NontransformingBridge" or "ShapeRepresentation" or "TargetRoot"
                        || child.Record.SourceEntityType is "AXIS2_PLACEMENT_3D" or "CARTESIAN_POINT" or "DIRECTION")) pending.Push(target);
            }
        }
        private void CheckPlacementItems(string id, IReadOnlyList<StepParameter> shape, string? root)
        {
            foreach (var item in Items(shape[1]))
            {
                string? target = Ref(item);
                if (target == root && root is not null) continue;
                if (target is null || P(target, "AXIS2_PLACEMENT_3D") is null)
                    Add("Binding", "RootBindingInvalid", id, related: target is null ? [] : [target]);
            }
        }
        private HashSet<string> GeometryClosure(IEnumerable<string> seeds)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal); var pending = new Stack<string>(seeds);
            while (pending.TryPop(out string? id))
            {
                if (!_rows.TryGetValue(id, out var row) || row.Role is not ("GeometryDependency" or "TargetRoot") || !visited.Add(id)) continue;
                foreach (string target in References(id)) pending.Push(target);
            }
            return visited;
        }
        private void PresentationAndOwnership()
        {
            var rootClosure = GeometryClosure(_root is null ? [] : [_root]);
            var seeds = new List<string>();
            if (_root is not null) seeds.Add(_root);
            foreach (string id in new[] { _geometry, _main }.OfType<string>().Distinct(StringComparer.Ordinal))
                if (_rows.TryGetValue(id, out var row) && P(id, row.Record.SourceEntityType) is { } p)
                    seeds.AddRange(Items(p[1]).Select(Ref).OfType<string>());
            var owned = GeometryClosure(seeds);
            bool ownershipAvailable = _root is not null && _geometry is not null && _main is not null;
            if (!ownershipAvailable) Add("Ownership", "DependencyUnavailable");
            if (_root is null && _rows.Any(r => r.Value.Role == "Presentation")) Add("Presentation", "DependencyUnavailable");
            foreach (var (id, row) in _rows)
            {
                if (ownershipAvailable && row.Role == "GeometryDependency" && !owned.Contains(id)) Add("Ownership", "UnownedGeometry", id);
                if (row.Role != "Presentation") continue;
                if (P(id, "STYLED_ITEM") is { } styled)
                {
                    if (_root is not null && (Ref(styled[2]) is not { } target || !rootClosure.Contains(target))) Add("Presentation", "PresentationInvalid", id);
                    OnlyIncoming(id, Incoming(id).Where(s => _rows.TryGetValue(s, out var incoming)
                        && incoming.Record.SourceEntityType is "PRESENTATION_LAYER_ASSIGNMENT" or "MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION"),
                        "Presentation", "PresentationInvalid");
                }
                if (P(id, "PRESENTATION_LAYER_ASSIGNMENT") is { } layer)
                    foreach (var item in Items(layer[2]))
                    {
                        string? target = Ref(item);
                        if (target is not null && P(target, "STYLED_ITEM") is { } s) target = Ref(s[2]);
                        if (_root is not null && (target is null || !rootClosure.Contains(target))) Add("Presentation", "PresentationInvalid", id, related: target is null ? [] : [target]);
                    }
            }
            if (_root is not null)
                OnlyIncoming(_root, Incoming(_root).Where(s => s == _geometry || (P(s, "STYLED_ITEM") is { } st && Ref(st[2]) == _root)
                    || (P(s, "PRESENTATION_LAYER_ASSIGNMENT") is { } l && Items(l[2]).Any(v => Ref(v) == _root))), "Binding", "RootBindingInvalid");
            // Kahn elimination avoids recursion/stack growth in malicious presentation graphs.
            var graph = _rows.Where(p => p.Value.Role == "Presentation").ToDictionary(p => p.Key,
                p => References(p.Key).Where(t => _rows.TryGetValue(t, out var child) && child.Role == "Presentation")
                    .Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
            var degrees = graph.ToDictionary(p => p.Key, _ => 0, StringComparer.Ordinal);
            foreach (var targets in graph.Values) foreach (string target in targets) degrees[target]++;
            var queue = new Queue<string>(degrees.Where(p => p.Value == 0).Select(p => p.Key));
            while (queue.TryDequeue(out string? id)) foreach (string target in graph[id]) if (--degrees[target] == 0) queue.Enqueue(target);
            foreach (string id in degrees.Where(p => p.Value > 0).Select(p => p.Key)) Add("Presentation", "PresentationInvalid", id);
        }
    }
}
