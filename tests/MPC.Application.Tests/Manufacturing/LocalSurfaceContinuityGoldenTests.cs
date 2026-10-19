using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping.Step;

namespace MPC.Application.Tests.Manufacturing;

public sealed class LocalSurfaceContinuityGoldenTests
{
    [Fact]
    public void T003Cylinder_RealPlanarCylinderJointIsLocallySharp()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        string? path = null;
        while (directory is not null && path is null)
        {
            string local = Path.Combine(directory.FullName,
                "TestData", "Step", "Golden", "T003_CYLINDER.step");
            string repository = Path.Combine(directory.FullName, "tests",
                "MPC.Tests", "TestData", "Step", "Golden", "T003_CYLINDER.step");
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
            new StepCircleEntityHandler(),
            new StepVertexPointEntityHandler(),
            new StepEdgeCurveEntityHandler(),
            new StepOrientedEdgeEntityHandler(),
            new StepEdgeLoopEntityHandler(),
            new StepFaceOuterBoundEntityHandler(),
            new StepAdvancedFaceEntityHandler(),
            new StepPlaneEntityHandler(),
            new StepCylindricalSurfaceEntityHandler(),
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
        Assert.Equal(4, faces.Count);

        int planarCurvedNeighbors = 0;
        int classified = 0;
        foreach (var face in faces.Where(f => f.Surface is MPC.Geometry.Primitives.Plane3D))
        {
            var adjacency = FaceAdjacencyQuery.Analyze(faces, face);
            foreach (var neighbor in adjacency.UniqueNeighbors
                .Where(f => f.Surface is MPC.Geometry.Primitives.Cylinder3D))
            {
                planarCurvedNeighbors++;
                var evidence = LocalSurfaceContinuityQuery.Evaluate(
                    faces, face, neighbor, 1e-6, 0.01);
                if (evidence.Kind != LocalSurfaceContinuityKind.Indeterminate)
                {
                    Assert.Equal(LocalG0Status.CoincidentAtSample, evidence.G0);
                    Assert.Equal(LocalSurfaceContinuityKind.SharpAtSample, evidence.Kind);
                    Assert.Equal(LocalTangentStatus.NotTangentAtSample, evidence.Tangency);
                    classified++;
                }
            }
        }
        Assert.True(planarCurvedNeighbors > 0,
            "Golden cylinder has no canonical planar-cylinder adjacency.");
        Assert.True(classified > 0,
            "No real planar-cylinder adjacency yielded local continuity evidence.");
    }
}
