using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepGoldenManifoldSolidBrepIntegrationTests
{
    [Fact]
    public void Map_T001Prism_MapsManifoldSolidBrepThroughRealCanonicalPipeline()
    {
        string filePath = GetGoldenStepFilePath("T001_PRISM.step");

        var dataReader = new StepDataEntityReader(new StepDataInventoryValidator());
        var dataResult = dataReader.Read(filePath);

        Assert.Equal(CadConnectorResultStatus.Succeeded, dataResult.Status);
        Assert.Empty(dataResult.Diagnostics);

        StepDataInventory inventory =
            Assert.IsType<StepDataInventory>(dataResult.Value);

        Assert.Equal(187, inventory.TotalEntityCount);
        Assert.Equal(1, inventory.GetCount("MANIFOLD_SOLID_BREP"));
        Assert.Equal(1, inventory.GetCount("CLOSED_SHELL"));
        Assert.Equal(6, inventory.GetCount("ADVANCED_FACE"));
        Assert.Equal(6, inventory.GetCount("FACE_OUTER_BOUND"));
        Assert.Equal(6, inventory.GetCount("EDGE_LOOP"));
        Assert.Equal(24, inventory.GetCount("ORIENTED_EDGE"));
        Assert.Equal(12, inventory.GetCount("EDGE_CURVE"));
        Assert.Equal(12, inventory.GetCount("LINE"));
        Assert.Equal(8, inventory.GetCount("VERTEX_POINT"));
        Assert.Equal(6, inventory.GetCount("PLANE"));
        Assert.Equal(7, inventory.GetCount("AXIS2_PLACEMENT_3D"));

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

        var semanticReader =
            new StepSemanticEntityReader(
                new StepParameterParser(),
                handlers);

        var semanticEntities =
            new List<StepSemanticEntity>();

        foreach (StepDataEntityRecord record in inventory.Entities)
        {
            StepSemanticEntity? semanticEntity = semanticReader.Read(record);

            if (semanticEntity is not null)
            {
                semanticEntities.Add(semanticEntity);
            }
        }

        Assert.Equal(154, semanticEntities.Count);

        StepManifoldSolidBrepEntity manifoldSolidBrep =
            Assert.Single(
                semanticEntities.OfType<StepManifoldSolidBrepEntity>());

        Assert.Equal("#63", manifoldSolidBrep.SourceEntityId);
        Assert.Equal("#79", manifoldSolidBrep.OuterShellReferenceId);

        StepClosedShellEntity closedShell =
            Assert.Single(
                semanticEntities.OfType<StepClosedShellEntity>());

        Assert.Equal("#79", closedShell.SourceEntityId);

        Assert.Equal(
            ["#8", "#30", "#62", "#76", "#143", "#174"],
            closedShell.FaceReferenceIds);

        var semanticResolver =
            new StepSemanticEntityResolver(semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(semanticResolver);

        var canonicalResolver =
            new StepCanonicalReferenceResolver(semanticResolver);

        var context =
            new MappingContext(
                referenceResolver,
                canonicalResolver);

        var mapper =
            new StepManifoldSolidBrepMapper();

        MappingResult<Solid> result =
            mapper.Map(
                manifoldSolidBrep,
                context);

        Assert.True(
            result.IsSuccessful,
            CreateFailureMessage(
                manifoldSolidBrep,
                result.Diagnostics));

        Assert.NotEqual(MappingStatus.Failed, result.Status);

        Solid solid =
            Assert.IsType<Solid>(result.Value);

        Assert.NotNull(solid.OuterShell);
        Assert.Empty(solid.InnerShells);

        Shell outerShell = solid.OuterShell;

        Assert.Equal(6, outerShell.Count);
        Assert.Equal(6, outerShell.Faces.Count);

        for (int index = 0;
             index < closedShell.FaceReferenceIds.Count;
             index++)
        {
            string faceReferenceId =
                closedShell.FaceReferenceIds[index];

            StepAdvancedFaceEntity sourceFace =
                semanticResolver.Resolve<StepAdvancedFaceEntity>(
                    faceReferenceId);

            Face canonicalFace =
                outerShell.Faces[index];

            Assert.IsType<Plane3D>(
                canonicalFace.Surface);

            Assert.Empty(
                canonicalFace.InnerLoops);

            Assert.True(
                canonicalFace.OuterLoop.EdgeUses.Count >= 3);

            AssertLoopContinuity(
                canonicalFace.OuterLoop);

            AssertLoopClosure(
                canonicalFace.OuterLoop);

            FaceOrientation expectedOrientation =
                sourceFace.SameSense
                    ? FaceOrientation.Forward
                    : FaceOrientation.Reversed;

            Assert.Equal(
                expectedOrientation,
                canonicalFace.Orientation);
        }

        Assert.Equal(
            5,
            outerShell.Faces.Count(
                face =>
                    face.Orientation ==
                    FaceOrientation.Reversed));

        Assert.Equal(
            1,
            outerShell.Faces.Count(
                face =>
                    face.Orientation ==
                    FaceOrientation.Forward));

        int totalEdgeUseCount =
            outerShell.Faces.Sum(
                face =>
                    face.OuterLoop.EdgeUses.Count);

        Assert.Equal(24, totalEdgeUseCount);

        Edge[] uniqueCanonicalEdges =
            outerShell.Faces
                .SelectMany(face => face.OuterLoop.EdgeUses)
                .Select(edgeUse => edgeUse.Edge)
                .Distinct<Edge>(ReferenceEqualityComparer.Instance)
                .ToArray();

        Assert.Equal(12, uniqueCanonicalEdges.Length);

        Vertex[] uniqueCanonicalVertices =
            uniqueCanonicalEdges
                .SelectMany(
                    edge =>
                        new Vertex[]
                        {
                            edge.StartVertex,
                            edge.EndVertex
                        })
                .Distinct<Vertex>(ReferenceEqualityComparer.Instance)
                .ToArray();

        Assert.Equal(8, uniqueCanonicalVertices.Length);

        Assert.Empty(result.Diagnostics);
        Assert.Empty(context.Diagnostics);
    }

    private static void AssertLoopContinuity(Loop loop)
    {
        for (int index = 0;
             index < loop.EdgeUses.Count - 1;
             index++)
        {
            Assert.Same(
                loop.EdgeUses[index].EndVertex,
                loop.EdgeUses[index + 1].StartVertex);
        }
    }

    private static void AssertLoopClosure(Loop loop)
    {
        Assert.Same(
            loop.EdgeUses[^1].EndVertex,
            loop.EdgeUses[0].StartVertex);
    }

    private static string CreateFailureMessage(
        StepManifoldSolidBrepEntity source,
        IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(source.SourceEntityId)
                ? "MANIFOLD_SOLID_BREP"
                : source.SourceEntityId;

        if (diagnostics.Count == 0)
        {
            return $"STEP MANIFOLD_SOLID_BREP '{sourceIdentity}' mapping failed without diagnostics.";
        }

        string diagnosticText =
            string.Join(
                Environment.NewLine,
                diagnostics.Select(
                    diagnostic =>
                        $"{diagnostic.Code}: {diagnostic.Message}"));

        return
            $"STEP MANIFOLD_SOLID_BREP '{sourceIdentity}' mapping failed." +
            Environment.NewLine +
            diagnosticText;
    }

    private static string GetGoldenStepFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "Golden STEP file name cannot be null, empty, or whitespace.",
                nameof(fileName));
        }

        var currentDirectory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidatePath =
                Path.Combine(
                    currentDirectory.FullName,
                    "TestData",
                    "Step",
                    "Golden",
                    fileName);

            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            candidatePath =
                Path.Combine(
                    currentDirectory.FullName,
                    "tests",
                    "MPC.Tests",
                    "TestData",
                    "Step",
                    "Golden",
                    fileName);

            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            currentDirectory =
                currentDirectory.Parent;
        }

        throw new FileNotFoundException(
            $"Golden STEP test file was not found: '{fileName}'.",
            fileName);
    }
}
