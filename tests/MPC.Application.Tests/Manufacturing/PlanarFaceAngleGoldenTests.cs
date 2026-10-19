using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping.Step;

namespace MPC.Application.Tests.Manufacturing;

public sealed class PlanarFaceAngleGoldenTests
{
    [Fact]
    public void T001Prism_AllSharedPlanarEdgesHaveRightAngle()
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

        foreach (var face in faces)
        {
            var query = FaceAdjacencyQuery.Analyze(faces, face);
            Assert.Equal(4, query.UniqueNeighbors.Count);
            foreach (var neighbor in query.UniqueNeighbors)
            {
                Assert.True(PlanarFaceAngleQuery.TryCalculate(
                    faces, face, neighbor, out double angle));
                Assert.InRange(angle, 90.0 - 1e-8, 90.0 + 1e-8);
            }
        }
    }
}
