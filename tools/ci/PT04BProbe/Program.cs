using System.Security.Cryptography;
using System.Text.Json;
using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping.Step;

if (args.Length != 3) throw new ArgumentException("Expected STEP path, SHA256 and evidence path.");
string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0])));
if (!hash.Equals(args[1],StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Real STEP corpus source hash mismatch.");
var data = new StepDataEntityReader(new StepDataInventoryValidator()).Read(args[0]).Value
    ?? throw new InvalidOperationException("No STEP inventory.");
IStepSemanticEntityHandler[] handlers =
[
    new StepAdvancedFaceEntityHandler(), new StepAxis2Placement3DEntityHandler(),
    new StepBSplineSurfaceWithKnotsEntityHandler(), new StepBSplineCurveWithKnotsEntityHandler(),
    new StepComplexBSplineEntityHandler(), new StepCartesianPointEntityHandler(),
    new StepCircleEntityHandler(), new StepClosedShellEntityHandler(),
    new StepConicalSurfaceEntityHandler(), new StepCylindricalSurfaceEntityHandler(),
    new StepDegenerateToroidalSurfaceEntityHandler(), new StepDirectionEntityHandler(),
    new StepEdgeCurveEntityHandler(), new StepEdgeLoopEntityHandler(),
    new StepFaceBoundEntityHandler(), new StepFaceOuterBoundEntityHandler(),
    new StepLineEntityHandler(), new StepManifoldSolidBrepEntityHandler(),
    new StepOrientedEdgeEntityHandler(), new StepPlaneEntityHandler(),
    new StepSphericalSurfaceEntityHandler(), new StepToroidalSurfaceEntityHandler(),
    new StepVectorEntityHandler(), new StepVertexPointEntityHandler()
];
var reader = new StepSemanticEntityReader(new StepParameterParser(),handlers);
var entities = data.Entities.Select(reader.Read).OfType<StepSemanticEntity>().ToArray();
var root = entities.OfType<StepManifoldSolidBrepEntity>().Single();
var op = StepCanonicalMappingOperation.Create(new StepSemanticEntityResolver(entities));
var mapped = op.Map(root);
if (!mapped.IsSuccessful || mapped.Value is null)
    throw new InvalidOperationException("Canonical real STEP mapping failed: " +
        string.Join("; ",mapped.Diagnostics.Select(x=>x.Message)));
var faces = mapped.Value.OuterShell.Faces;
var samples = new List<object>();
int opposite=0,same=0,indeterminate=0,angleValid=0,angleRejected=0;
for(int i=0;i<faces.Count;i++)
{
    var adjacency=FaceAdjacencyQuery.Analyze(faces,faces[i]);
    for(int j=i+1;j<faces.Count;j++)
    {
        if(!adjacency.UniqueNeighbors.Any(f=>ReferenceEquals(f,faces[j])))continue;
        var direction=SharedEdgeOrientationQuery.Evaluate(faces,faces[i],faces[j]);
        switch(direction)
        {
            case SharedEdgeOrientationStatus.Opposite: opposite++;break;
            case SharedEdgeOrientationStatus.SameDirection: same++;break;
            default: indeterminate++;break;
        }
        bool valid=SignedLocalDihedralQuery.TryCalculate(faces,faces[i],faces[j],out double angle);
        if(valid)
        {
            if(!double.IsFinite(angle)||Math.Abs(angle)>180)
                throw new InvalidOperationException("Invalid signed dihedral witness.");
            angleValid++;
        }
        else angleRejected++;
        samples.Add(new {First=i,Second=j,Orientation=direction.ToString(),
            AngleValid=valid,AngleDegrees=valid?(double?)angle:null});
    }
}
if(samples.Count==0 || opposite+same+indeterminate!=samples.Count ||
   angleValid+angleRejected!=samples.Count)
    throw new InvalidOperationException("Incomplete adjacency evidence.");
File.WriteAllText(args[2],JsonSerializer.Serialize(new {
    SourceSHA256=hash, Faces=faces.Count, AdjacentPairs=samples.Count,
    Opposite=opposite, SameDirection=same, Indeterminate=indeterminate,
    AngleValid=angleValid, AngleRejected=angleRejected,
    MaterialOrientationVerified=false, Samples=samples
},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS - actual STEP adjacency evidence: {faces.Count} faces, {samples.Count} pairs; F2 opposite {opposite}, same {same}, indeterminate {indeterminate}; F3 valid {angleValid}, rejected {angleRejected}. F4 material proof NOT supplied.");
