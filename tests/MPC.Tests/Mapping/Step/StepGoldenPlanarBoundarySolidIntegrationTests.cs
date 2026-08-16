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

public sealed class StepGoldenPlanarBoundarySolidIntegrationTests
{
    [Fact]
    public void Map_T101BasicPocket_PreservesSingleInnerBoundary()
    {
        MapAndAssert(
            fileName: "T101_BASIC_POCKET.step",
            expectedTotalEntityCount: 329,
            expectedFaceCount: 11,
            expectedLoopCount: 12,
            expectedEdgeUseCount: 48,
            expectedEdgeCount: 24,
            expectedVertexCount: 16,
            expectedInnerLoopCountsBySourceFaceId:
            new Dictionary<string, int>(
                StringComparer.Ordinal)
            {
                ["#53"] = 1
            });
    }

    [Fact]
    public void Map_T103MultipleIslands_PreservesDistributedInnerBoundaries()
    {
        MapAndAssert(
            fileName: "T103_MULTIPLE_ISLANDS.step",
            expectedTotalEntityCount: 755,
            expectedFaceCount: 26,
            expectedLoopCount: 30,
            expectedEdgeUseCount: 120,
            expectedEdgeCount: 60,
            expectedVertexCount: 40,
            expectedInnerLoopCountsBySourceFaceId:
            new Dictionary<string, int>(
                StringComparer.Ordinal)
            {
                ["#407"] = 3,
                ["#451"] = 1
            });
    }

    private static void MapAndAssert(
        string fileName,
        int expectedTotalEntityCount,
        int expectedFaceCount,
        int expectedLoopCount,
        int expectedEdgeUseCount,
        int expectedEdgeCount,
        int expectedVertexCount,
        IReadOnlyDictionary<string, int>
            expectedInnerLoopCountsBySourceFaceId)
    {
        string filePath =
            GetGoldenStepFilePath(
                fileName);

        var dataReader =
            new StepDataEntityReader(
                new StepDataInventoryValidator());

        var dataResult =
            dataReader.Read(
                filePath);

        Assert.Equal(
            CadConnectorResultStatus.Succeeded,
            dataResult.Status);

        Assert.Empty(
            dataResult.Diagnostics);

        StepDataInventory inventory =
            Assert.IsType<StepDataInventory>(
                dataResult.Value);

        Assert.Equal(
            expectedTotalEntityCount,
            inventory.TotalEntityCount);

        Assert.Equal(
            1,
            inventory.GetCount(
                "MANIFOLD_SOLID_BREP"));

        Assert.Equal(
            1,
            inventory.GetCount(
                "CLOSED_SHELL"));

        Assert.Equal(
            expectedFaceCount,
            inventory.GetCount(
                "ADVANCED_FACE"));

        Assert.Equal(
            expectedFaceCount,
            inventory.GetCount(
                "FACE_OUTER_BOUND"));

        Assert.Equal(
            expectedInnerLoopCountsBySourceFaceId.Values.Sum(),
            inventory.GetCount(
                "FACE_BOUND"));

        Assert.Equal(
            expectedLoopCount,
            inventory.GetCount(
                "EDGE_LOOP"));

        Assert.Equal(
            expectedEdgeUseCount,
            inventory.GetCount(
                "ORIENTED_EDGE"));

        Assert.Equal(
            expectedEdgeCount,
            inventory.GetCount(
                "EDGE_CURVE"));

        Assert.Equal(
            expectedEdgeCount,
            inventory.GetCount(
                "LINE"));

        Assert.Equal(
            expectedVertexCount,
            inventory.GetCount(
                "VERTEX_POINT"));

        Assert.Equal(
            expectedFaceCount,
            inventory.GetCount(
                "PLANE"));

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
            new StepFaceBoundEntityHandler(),
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

        foreach (StepDataEntityRecord record
                 in inventory.Entities)
        {
            StepSemanticEntity? semanticEntity =
                semanticReader.Read(
                    record);

            if (semanticEntity is not null)
            {
                semanticEntities.Add(
                    semanticEntity);
            }
        }

        Assert.Equal(
            expectedFaceCount,
            semanticEntities
                .OfType<StepAdvancedFaceEntity>()
                .Count());

        Assert.Equal(
            expectedFaceCount,
            semanticEntities
                .OfType<StepFaceOuterBoundEntity>()
                .Count());

        Assert.Equal(
            expectedInnerLoopCountsBySourceFaceId.Values.Sum(),
            semanticEntities
                .OfType<StepFaceBoundEntity>()
                .Count(
                    bound =>
                        bound is not
                            StepFaceOuterBoundEntity));

        StepManifoldSolidBrepEntity manifoldSolidBrep =
            Assert.Single(
                semanticEntities
                    .OfType<
                        StepManifoldSolidBrepEntity>());

        StepClosedShellEntity closedShell =
            Assert.Single(
                semanticEntities
                    .OfType<StepClosedShellEntity>());

        Assert.Equal(
            expectedFaceCount,
            closedShell.FaceReferenceIds.Count);

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Solid> result =
            operation.Map(
                manifoldSolidBrep);

        Assert.True(
            result.IsSuccessful,
            CreateFailureMessage(
                fileName,
                manifoldSolidBrep,
                result.Diagnostics));

        Assert.NotEqual(
            MappingStatus.Failed,
            result.Status);

        Solid solid =
            Assert.IsType<Solid>(
                result.Value);

        Assert.NotNull(
            solid.OuterShell);

        Assert.Empty(
            solid.InnerShells);

        Shell outerShell =
            solid.OuterShell;

        Assert.Equal(
            expectedFaceCount,
            outerShell.Count);

        Assert.Equal(
            expectedFaceCount,
            outerShell.Faces.Count);

        for (int index = 0;
             index < closedShell.FaceReferenceIds.Count;
             index++)
        {
            string faceReferenceId =
                closedShell.FaceReferenceIds[index];

            StepAdvancedFaceEntity sourceFace =
                semanticResolver
                    .Resolve<StepAdvancedFaceEntity>(
                        faceReferenceId);

            Face canonicalFace =
                outerShell.Faces[index];

            Assert.IsType<Plane3D>(
                canonicalFace.Surface);

            AssertLoop(
                canonicalFace.OuterLoop);

            int expectedInnerLoopCount =
                sourceFace.SourceEntityId is not null &&
                expectedInnerLoopCountsBySourceFaceId
                    .TryGetValue(
                        sourceFace.SourceEntityId,
                        out int configuredCount)
                    ? configuredCount
                    : 0;

            Assert.Equal(
                expectedInnerLoopCount,
                canonicalFace.InnerLoops.Count);

            Assert.All(
                canonicalFace.InnerLoops,
                AssertLoop);

            FaceOrientation expectedOrientation =
                sourceFace.SameSense
                    ? FaceOrientation.Forward
                    : FaceOrientation.Reversed;

            Assert.Equal(
                expectedOrientation,
                canonicalFace.Orientation);
        }

        Assert.Equal(
            expectedInnerLoopCountsBySourceFaceId.Count,
            outerShell.Faces.Count(
                face =>
                    face.InnerLoops.Count > 0));

        Assert.Equal(
            expectedInnerLoopCountsBySourceFaceId
                .Values
                .Sum(),
            outerShell.Faces.Sum(
                face =>
                    face.InnerLoops.Count));

        Loop[] canonicalLoops =
            outerShell.Faces
                .SelectMany(
                    face =>
                        new[]
                        {
                            face.OuterLoop
                        }
                        .Concat(
                            face.InnerLoops))
                .ToArray();

        Assert.Equal(
            expectedLoopCount,
            canonicalLoops.Length);

        Assert.Equal(
            expectedEdgeUseCount,
            canonicalLoops.Sum(
                loop =>
                    loop.EdgeUses.Count));

        Edge[] uniqueCanonicalEdges =
            canonicalLoops
                .SelectMany(
                    loop =>
                        loop.EdgeUses)
                .Select(
                    edgeUse =>
                        edgeUse.Edge)
                .Distinct<Edge>(
                    ReferenceEqualityComparer.Instance)
                .ToArray();

        Assert.Equal(
            expectedEdgeCount,
            uniqueCanonicalEdges.Length);

        Vertex[] uniqueCanonicalVertices =
            uniqueCanonicalEdges
                .SelectMany(
                    edge =>
                        new[]
                        {
                            edge.StartVertex,
                            edge.EndVertex
                        })
                .Distinct<Vertex>(
                    ReferenceEqualityComparer.Instance)
                .ToArray();

        Assert.Equal(
            expectedVertexCount,
            uniqueCanonicalVertices.Length);

        Assert.All(
            uniqueCanonicalVertices,
            vertex =>
                Assert.Empty(
                    vertex.IncidentEdges));

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult assemblyResult =
            assembler.Assemble(
                solid);

        Assert.Same(
            solid,
            assemblyResult.Solid);

        Assert.Equal(
            expectedFaceCount,
            assemblyResult.Topology.Faces.Count);

        Assert.Equal(
            expectedLoopCount,
            assemblyResult.Topology.Loops.Count);

        Assert.Equal(
            expectedEdgeCount,
            assemblyResult.Topology.Edges.Count);

        Assert.Equal(
            expectedVertexCount,
            assemblyResult.Topology.Vertices.Count);

        foreach (Face face in outerShell.Faces)
        {
            Assert.Contains(
                assemblyResult.Topology.Faces,
                publishedFace =>
                    ReferenceEquals(
                        publishedFace,
                        face));
        }

        foreach (Edge edge in uniqueCanonicalEdges)
        {
            Assert.Contains(
                assemblyResult.Topology.Edges,
                publishedEdge =>
                    ReferenceEquals(
                        publishedEdge,
                        edge));
        }

        foreach (Vertex vertex in uniqueCanonicalVertices)
        {
            Assert.Contains(
                assemblyResult.Topology.Vertices,
                publishedVertex =>
                    ReferenceEquals(
                        publishedVertex,
                        vertex));

            Assert.NotEmpty(
                vertex.IncidentEdges);

            Assert.All(
                vertex.IncidentEdges,
                incidentEdge =>
                    Assert.True(
                        ReferenceEquals(
                            incidentEdge.StartVertex,
                            vertex) ||
                        ReferenceEquals(
                            incidentEdge.EndVertex,
                            vertex)));
        }

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            operation.Context.Diagnostics);
    }

    private static void AssertLoop(
        Loop loop)
    {
        Assert.True(
            loop.EdgeUses.Count >= 3);

        for (int index = 0;
             index < loop.EdgeUses.Count - 1;
             index++)
        {
            Assert.Same(
                loop.EdgeUses[index].EndVertex,
                loop.EdgeUses[index + 1].StartVertex);
        }

        Assert.Same(
            loop.EdgeUses[^1].EndVertex,
            loop.EdgeUses[0].StartVertex);
    }

    private static string CreateFailureMessage(
        string fileName,
        StepManifoldSolidBrepEntity source,
        IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "MANIFOLD_SOLID_BREP"
                : source.SourceEntityId;

        string diagnosticText =
            diagnostics.Count == 0
                ? "Mapping failed without diagnostics."
                : string.Join(
                    Environment.NewLine,
                    diagnostics.Select(
                        diagnostic =>
                            $"{diagnostic.Code}: {diagnostic.Message}"));

        return
            $"Golden STEP '{fileName}', entity " +
            $"'{sourceIdentity}' could not be mapped." +
            Environment.NewLine +
            diagnosticText;
    }

    private static string GetGoldenStepFilePath(
        string fileName)
    {
        if (string.IsNullOrWhiteSpace(
                fileName))
        {
            throw new ArgumentException(
                "Golden STEP file name cannot be null, empty, or whitespace.",
                nameof(fileName));
        }

        var currentDirectory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidatePath =
                Path.Combine(
                    currentDirectory.FullName,
                    "TestData",
                    "Step",
                    "Golden",
                    fileName);

            if (File.Exists(
                    candidatePath))
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

            if (File.Exists(
                    candidatePath))
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