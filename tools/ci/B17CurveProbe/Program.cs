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
     new StepEdgeCurveEntityHandler(), new StepBSplineCurveWithKnotsEntityHandler()]);
var entities = inventory.Entities.Select(reader.Read).OfType<StepSemanticEntity>().ToArray();
var curves = entities.OfType<StepBSplineCurveWithKnotsEntity>().ToDictionary(x => x.SourceEntityId!);
if (curves.Count != 1910) throw new InvalidOperationException($"Expected 1910 standard curves; got {curves.Count}.");
var operation = StepCanonicalMappingOperation.Create(new StepSemanticEntityResolver(entities));
var mapped = new HashSet<string>(StringComparer.Ordinal);
var evidence = new List<object>();
foreach (var edgeSource in entities.OfType<StepEdgeCurveEntity>())
{
    if (!curves.TryGetValue(edgeSource.EdgeGeometryReferenceId, out var source)) continue;
    var result = operation.Resolve<Edge>(edgeSource.SourceEntityId!);
    if (!result.IsSuccessful || result.Diagnostics.Count != 0 || result.Value?.Curve is not BSplineCurve3D curve)
        throw new InvalidOperationException($"Edge {edgeSource.SourceEntityId}: " +
            string.Join("; ", result.Diagnostics.Select(x => x.Message)));
    if (curve.Degree != source.Degree || !curve.Knots.SequenceEqual(source.Knots) ||
        !source.Multiplicities.SequenceEqual(curve.Multiplicities.Select(x => (long)x)) ||
        curve.ControlPoints.Count != source.ControlPointReferences.Count)
        throw new InvalidOperationException("Curve representation changed.");
    for (int i = 0; i < curve.ControlPoints.Count; i++)
    {
        var point = operation.Context.ReferenceResolver.Resolve<StepCartesianPointEntity>(source.ControlPointReferences[i]).Value!;
        if (curve.ControlPoints[i] != new Point3D(point.Coordinates[0], point.Coordinates[1], point.Coordinates[2]))
            throw new InvalidOperationException("Control-point order or values changed.");
    }
    if (!ReferenceEquals(curve, operation.Resolve<BSplineCurve3D>(source.SourceEntityId!).Value))
        throw new InvalidOperationException("Curve cache identity changed.");
    mapped.Add(source.SourceEntityId!);
    evidence.Add(new { Curve = source.SourceEntityId, Edge = edgeSource.SourceEntityId,
        curve.Degree, Points = curve.ControlPoints.Count, edgeSource.SameSense });
}
if (mapped.Count != 1910 || operation.Context.Diagnostics.Count != 0)
    throw new InvalidOperationException("Not all standard curves mapped cleanly through owning edges.");
File.WriteAllText(args[1], JsonSerializer.Serialize(new { SourceSHA256 = hash, Mapped = mapped.Count, Curves = evidence },
    new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"PASS - {mapped.Count}/1910 real standard B-spline curves and owning edges mapped with exact source values.");
