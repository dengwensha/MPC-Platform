using System.Security.Cryptography;
using System.Text.Json;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Geometry.Primitives;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

if (args.Length != 2) throw new ArgumentException("Expected R03 path and evidence path.");
string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0])));
if (hash != "319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12")
    throw new InvalidOperationException("R03 source hash mismatch.");
var inventory = new StepDataEntityReader(new StepDataInventoryValidator()).Read(args[0]).Value
    ?? throw new InvalidOperationException("Source inventory missing.");
var reader = new StepSemanticEntityReader(new StepParameterParser(),
    [new StepCartesianPointEntityHandler(), new StepVertexPointEntityHandler(),
     new StepEdgeCurveEntityHandler(), new StepComplexBSplineEntityHandler()]);
var entities = inventory.Entities.Select(reader.Read).OfType<StepSemanticEntity>().ToArray();
var curves = entities.OfType<StepRationalBSplineCurveEntity>().ToDictionary(x => x.SourceEntityId!);
if (!curves.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(["#72169", "#16491", "#54159", "#30473"]))
    throw new InvalidOperationException("Rational curve IDs differ from the verified B17 residuals.");
if (curves.Count != 4) throw new InvalidOperationException($"Expected 4 rational curves; got {curves.Count}.");
var operation = StepCanonicalMappingOperation.Create(new StepSemanticEntityResolver(entities));
var mapped = new HashSet<string>(StringComparer.Ordinal);
var evidence = new List<object>();
foreach (var edgeSource in entities.OfType<StepEdgeCurveEntity>())
{
    if (!curves.TryGetValue(edgeSource.EdgeGeometryReferenceId, out var source)) continue;
    var result = operation.Resolve<Edge>(edgeSource.SourceEntityId!);
    if (!result.IsSuccessful || result.Diagnostics.Count != 0 || result.Value?.Curve is not RationalBSplineCurve3D curve)
        throw new InvalidOperationException($"Edge {edgeSource.SourceEntityId}: " +
            string.Join("; ", result.Diagnostics.Select(x => x.Message)));
    if (!curve.Weights.SequenceEqual(source.Weights))
        throw new InvalidOperationException("Curve weights changed.");
    if (curve.Basis.Degree != source.Basis.Degree || !curve.Basis.Knots.SequenceEqual(source.Basis.Knots) ||
        !source.Basis.Multiplicities.SequenceEqual(curve.Basis.Multiplicities.Select(x => (long)x)) ||
        curve.Basis.ControlPoints.Count != source.Basis.ControlPointReferences.Count)
        throw new InvalidOperationException("Curve representation changed.");
    for (int i = 0; i < curve.Basis.ControlPoints.Count; i++)
    {
        var point = operation.Context.ReferenceResolver.Resolve<StepCartesianPointEntity>(source.Basis.ControlPointReferences[i]).Value!;
        if (curve.Basis.ControlPoints[i] != new Point3D(point.Coordinates[0], point.Coordinates[1], point.Coordinates[2]))
            throw new InvalidOperationException("Control-point order or values changed.");
    }
    if (!ReferenceEquals(curve, operation.Resolve<RationalBSplineCurve3D>(source.SourceEntityId!).Value))
        throw new InvalidOperationException("Curve cache identity changed.");
    mapped.Add(source.SourceEntityId!);
    evidence.Add(new { Curve = source.SourceEntityId, Edge = edgeSource.SourceEntityId,
        curve.Basis.Degree, Points = curve.Basis.ControlPoints.Count, curve.Weights, edgeSource.SameSense });
}
if (mapped.Count != 4 || operation.Context.Diagnostics.Count != 0)
    throw new InvalidOperationException("Not all rational curves mapped cleanly through owning edges.");
File.WriteAllText(args[1], JsonSerializer.Serialize(new { SourceSHA256 = hash, Mapped = mapped.Count, Curves = evidence },
    new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"PASS - {mapped.Count}/4 real rational B-spline curves and owning edges mapped with exact source values.");
