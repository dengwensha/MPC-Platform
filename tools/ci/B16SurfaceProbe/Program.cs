using System.Security.Cryptography;
using MPC.Application.Manufacturing;
using System.Text.Json;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

if (args.Length != 2) throw new ArgumentException("Expected R03 path and evidence output path.");
string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0])));
if (hash != "319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12")
    throw new InvalidOperationException("R03 source hash mismatch.");
var inventory = new StepDataEntityReader(new StepDataInventoryValidator()).Read(args[0]).Value
    ?? throw new InvalidOperationException("Source inventory missing.");
var reader = new StepSemanticEntityReader(new StepParameterParser(),
    [new StepCartesianPointEntityHandler(), new StepAdvancedFaceEntityHandler(),
     new StepRationalBSplineSurfaceEntityHandler()]);
var entities = inventory.Entities.Select(reader.Read).OfType<StepSemanticEntity>().ToArray();
var rational = entities.OfType<StepRationalBSplineSurfaceEntity>().ToDictionary(x => x.SourceEntityId!);
if (rational.Count != 276) throw new InvalidOperationException($"Expected 276 rational surfaces; got {rational.Count}.");
var context = new MappingContext(new StepReferenceResolver(new StepSemanticEntityResolver(entities)));
var mapper = new StepAdvancedFaceGeometryMapper(new StepPlaneGeometryMapper(),
    new StepCylindricalSurfaceGeometryMapper(), new StepConicalSurfaceGeometryMapper(),
    new StepSphericalSurfaceGeometryMapper());
var mapped = new HashSet<string>(StringComparer.Ordinal);
var evidence = new List<object>();
int normalSuccess = 0;
int normalRejected = 0;
int noEligibleSpan = 0;
int invalidAtSample = 0;
foreach (var face in entities.OfType<StepAdvancedFaceEntity>())
{
    if (!rational.TryGetValue(face.SurfaceReferenceId, out var source)) continue;
    var result = mapper.Map(face, context);
    if (!result.IsSuccessful || result.Diagnostics.Count != 0 ||
        result.Value is not RationalBSplineSurface3D surface)
        throw new InvalidOperationException($"Surface {source.SourceEntityId}: " +
            string.Join("; ", result.Diagnostics.Select(x => x.Message)));
    if (!source.Weights.SelectMany(x => x).SequenceEqual(surface.Weights.SelectMany(x => x)) ||
        !source.Basis.UKnots.SequenceEqual(surface.Basis.UKnots) ||
        !source.Basis.VKnots.SequenceEqual(surface.Basis.VKnots) ||
        !source.Basis.UMultiplicities.SequenceEqual(surface.Basis.UMultiplicities.Select(x => (long)x)) ||
        !source.Basis.VMultiplicities.SequenceEqual(surface.Basis.VMultiplicities.Select(x => (long)x)) ||
        source.Basis.UDegree != surface.Basis.UDegree || source.Basis.VDegree != surface.Basis.VDegree)
        throw new InvalidOperationException($"Surface {source.SourceEntityId}: representation changed.");
    for (int u = 0; u < source.Basis.ControlPointReferenceGrid.Count; u++)
        for (int v = 0; v < source.Basis.ControlPointReferenceGrid[u].Count; v++)
        {
            var point = context.ReferenceResolver.Resolve<StepCartesianPointEntity>(
                source.Basis.ControlPointReferenceGrid[u][v]).Value!;
            if (surface.Basis.ControlPointGrid[u][v] != new Point3D(point.Coordinates[0], point.Coordinates[1], point.Coordinates[2]))
                throw new InvalidOperationException("Control-point order or value changed.");
        }
    // E5: evaluate a real, mapped rational STEP surface at an explicit
    // interior UV witness, never inferring trimmed-face XYZ coordinates.
    double u0 = surface.Basis.UKnots[0];
    double u1 = surface.Basis.UKnots[^1];
    double v0 = surface.Basis.VKnots[0];
    double v1 = surface.Basis.VKnots[^1];
    // Select an interior point from an actual positive knot span, avoiding
    // the midpoint coinciding with an interior knot (review GE03-R04).
    double? uSample = InteriorSpanSample(surface.Basis.UKnots);
    double? vSample = InteriorSpanSample(surface.Basis.VKnots);
    bool eligible = uSample.HasValue && vSample.HasValue;
    bool normalValid = eligible && RationalBSplineNormalQuery.TryEvaluate(
        surface, uSample!.Value, vSample!.Value, out _);
    if (normalValid)
    {
        if (!RationalBSplineNormalQuery.TryEvaluate(
                surface, uSample!.Value, vSample!.Value, out var normal))
            throw new InvalidOperationException("Non-deterministic normal query.");
        double length = Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        if (!double.IsFinite(length) || Math.Abs(length - 1.0) > 1e-8)
            throw new InvalidOperationException($"Surface {source.SourceEntityId}: invalid unit normal.");
        normalSuccess++;
    }
    else
    {
        normalRejected++;
        if (!eligible) noEligibleSpan++;
        else invalidAtSample++;
    }
    mapped.Add(source.SourceEntityId!);
    evidence.Add(new { Surface = source.SourceEntityId, Face = face.SourceEntityId,
        surface.Basis.UDegree, surface.Basis.VDegree,
        Rows = surface.Weights.Count, Columns = surface.Weights[0].Count,
        U = uSample, V = vSample, NormalValid = normalValid,
        RejectionReason = normalValid ? null : eligible ? "NormalUndefinedAtInteriorSample" : "NoPositiveInteriorKnotSpan" });
}
if (mapped.Count != 276 || context.Diagnostics.Count != 0)
    throw new InvalidOperationException("Not all rational surfaces mapped cleanly through advanced-face dispatch.");
if (normalSuccess == 0 || normalSuccess + normalRejected != mapped.Count)
    throw new InvalidOperationException("No valid real STEP B-spline normal evidence was produced.");
if (noEligibleSpan + invalidAtSample != normalRejected)
    throw new InvalidOperationException("R03 rejection histogram inconsistent.");
File.WriteAllText(args[1], JsonSerializer.Serialize(new { SourceSHA256 = hash, Mapped = mapped.Count,
    NormalSuccess = normalSuccess, NormalRejected = normalRejected,
    NoEligibleSpan = noEligibleSpan, InvalidAtSample = invalidAtSample,
    Surfaces = evidence },
    new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"PASS - {mapped.Count}/276 real rational surfaces mapped; " +
    $"E5 regular unit normals: {normalSuccess}, controlled rejections: {normalRejected}.");

static double? InteriorSpanSample(IReadOnlyList<double> knots)
{
    for (int i = 0; i + 1 < knots.Count; i++)
    {
        double start = knots[i], end = knots[i+1];
        if (double.IsFinite(start) && double.IsFinite(end) && end > start)
        {
            double sample = start + (end - start) * 0.37;
            if (sample > start && sample < end) return sample;
        }
    }
    return null;
}
