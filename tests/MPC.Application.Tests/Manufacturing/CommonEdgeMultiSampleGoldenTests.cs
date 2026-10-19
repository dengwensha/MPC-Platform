using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping.Step;

namespace MPC.Application.Tests.Manufacturing;

public sealed class CommonEdgeMultiSampleGoldenTests
{
    [Fact]
    public void T001Prism_CanonicalLineEdgesYieldValidatedInteriorWitnesses()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        string? path = null;
        while (directory is not null && path is null)
        {
            string local = Path.Combine(directory.FullName,
                "TestData", "Step", "Golden", "T001_PRISM.step");
            string repository = Path.Combine(directory.FullName, "tests",
                "MPC.Tests", "TestData", "Step", "Golden", "T001_PRISM.step");
            if (File.Exists(local)) path = local;
            else if (File.Exists(repository)) path = repository;
            directory = directory.Parent;
        }
        Assert.NotNull(path);

        var data = new StepDataEntityReader(new StepDataInventoryValidator())
            .Read(path!);
        Assert.NotNull(data.Value);
        IStepSemanticEntityHandler[] handlers =
        [
            new StepCartesianPointEntityHandler(),
            new StepDirectionEntityHandler(),
            new StepVectorEntityHandler(),
            new StepLineEntityHandler(),
            new StepVertexPointEntityHandler(),
            new StepEdgeCurveEntityHandler(),
            new StepOrientedEdgeEntityHandler(),
            new StepEdgeLoopEntityHandler(),
            new StepFaceOuterBoundEntityHandler(),
            new StepAdvancedFaceEntityHandler(),
            new StepPlaneEntityHandler(),
            new StepAxis2Placement3DEntityHandler(),
            new StepClosedShellEntityHandler(),
            new StepManifoldSolidBrepEntityHandler()
        ];

        var reader = new StepSemanticEntityReader(new StepParameterParser(), handlers);
        StepSemanticEntity[] entities = data.Value!.Entities
            .Select(x => reader.Read(x))
            .OfType<StepSemanticEntity>()
            .ToArray();
        var root = Assert.Single(entities.OfType<StepManifoldSolidBrepEntity>());
        var operation = StepCanonicalMappingOperation.Create(
            new StepSemanticEntityResolver(entities));
        var mapped = operation.Map(root);
        Assert.True(mapped.IsSuccessful);
        Assert.NotNull(mapped.Value);
        var faces = mapped.Value!.OuterShell.Faces;
        Assert.Equal(6, faces.Count);

        var edges = faces
            .SelectMany(face => FaceAdjacencyQuery.Analyze(faces, face).Boundaries)
            .Select(boundary => boundary.Edge)
            .Distinct()
            .ToArray();
        Assert.Equal(12, edges.Length);
        int verified = 0;
        foreach (var edge in edges)
        {
            Assert.IsType<MPC.Geometry.Primitives.Line3D>(edge.Curve);
            Assert.True(CommonEdgeMultiSampleQuery.TrySample(edge, out var points));
            Assert.Equal(3, points.Count);

            var a = edge.StartVertex.Position;
            var b = edge.EndVertex.Position;
            for (int i = 0; i < points.Count; i++)
            {
                double t = (i + 1) / 4.0;
                Assert.InRange(Math.Abs(points[i].X - (a.X + t * (b.X - a.X))), 0, 1e-8);
                Assert.InRange(Math.Abs(points[i].Y - (a.Y + t * (b.Y - a.Y))), 0, 1e-8);
                Assert.InRange(Math.Abs(points[i].Z - (a.Z + t * (b.Z - a.Z))), 0, 1e-8);
            }
            verified++;
        }
        Assert.Equal(12, verified);
    }
}
