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
int reverseAgreement=0, rightAngle=0, otherAngle=0;
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
            // A signed witness must be invariant under swapping the face order.
            if (!SignedLocalDihedralQuery.TryCalculate(faces,faces[j],faces[i],out double reverse) ||
                !double.IsFinite(reverse) || Math.Abs(angle-reverse)>1e-8)
                throw new InvalidOperationException("Signed dihedral face-order oracle mismatch.");
            reverseAgreement++;
            if (Math.Abs(Math.Abs(angle)-90.0)<=1e-8) rightAngle++;
            else otherAngle++;
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
if (reverseAgreement != angleValid || rightAngle + otherAngle != angleValid)
    throw new InvalidOperationException("Incomplete signed-angle oracle evidence.");
// Pinned corpus source hashes and measured golden distributions from the
// successful independent real-corpus validation run 39645588085.
// These are evidence regression counts, not a material-side correctness oracle.
int expectedFaces, expectedPairs, expectedOpposite, expectedIndeterminate,
    expectedValid, expectedRejected;
if (hash.Equals("E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD",
        StringComparison.OrdinalIgnoreCase))
{
    expectedFaces=11;expectedPairs=24;expectedOpposite=24;
    expectedIndeterminate=0;expectedValid=24;expectedRejected=0;
}
else if (hash.Equals("0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F",
        StringComparison.OrdinalIgnoreCase))
{
    expectedFaces=23;expectedPairs=44;expectedOpposite=40;
    expectedIndeterminate=4;expectedValid=24;expectedRejected=20;
}
else throw new InvalidOperationException("Unrecognised real STEP golden corpus.");
if (faces.Count!=expectedFaces || samples.Count!=expectedPairs ||
    opposite!=expectedOpposite || same!=0 ||
    indeterminate!=expectedIndeterminate ||
    angleValid!=expectedValid || angleRejected!=expectedRejected)
    throw new InvalidOperationException("Real STEP golden orientation/angle histogram drift.");
File.WriteAllText(args[2],JsonSerializer.Serialize(new {
    SourceSHA256=hash, Faces=faces.Count, AdjacentPairs=samples.Count,
    Opposite=opposite, SameDirection=same, Indeterminate=indeterminate,
    AngleValid=angleValid, AngleRejected=angleRejected,
    ReverseAgreement=reverseAgreement, RightAngleWitnesses=rightAngle,
    OtherAngleWitnesses=otherAngle,
    MaterialOrientationVerified=false, Samples=samples
},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS - actual STEP adjacency evidence: {faces.Count} faces, {samples.Count} pairs; F2 opposite {opposite}, same {same}, indeterminate {indeterminate}; F3 valid {angleValid}, rejected {angleRejected}. F4 material proof NOT supplied.");
