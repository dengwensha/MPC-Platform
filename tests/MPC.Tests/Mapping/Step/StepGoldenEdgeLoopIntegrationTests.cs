using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class StepGoldenEdgeLoopIntegrationTests
{
    [Fact]
    public void Map_T001Prism_MapsAllEdgeLoopsThroughRealCanonicalPipeline()
    {
        string filePath =
            GetGoldenStepFilePath(
                "T001_PRISM.step");

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
            187,
            inventory.TotalEntityCount);

        Assert.Equal(
            6,
            inventory.GetCount(
                "EDGE_LOOP"));

        Assert.Equal(
            24,
            inventory.GetCount(
                "ORIENTED_EDGE"));

        Assert.Equal(
            12,
            inventory.GetCount(
                "EDGE_CURVE"));

        Assert.Equal(
            12,
            inventory.GetCount(
                "LINE"));

        Assert.Equal(
            8,
            inventory.GetCount(
                "VERTEX_POINT"));

        IStepSemanticEntityHandler[] handlers =
        [
            new StepCartesianPointEntityHandler(),
            new StepDirectionEntityHandler(),
            new StepVectorEntityHandler(),
            new StepLineEntityHandler(),
            new StepVertexPointEntityHandler(),
            new StepEdgeCurveEntityHandler(),
            new StepOrientedEdgeEntityHandler(),
            new StepEdgeLoopEntityHandler()
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
            127,
            semanticEntities.Count);

        Assert.Equal(
            27,
            semanticEntities
                .OfType<StepCartesianPointEntity>()
                .Count());

        Assert.Equal(
            26,
            semanticEntities
                .OfType<StepDirectionEntity>()
                .Count());

        Assert.Equal(
            12,
            semanticEntities
                .OfType<StepVectorEntity>()
                .Count());

        Assert.Equal(
            12,
            semanticEntities
                .OfType<StepLineEntity>()
                .Count());

        Assert.Equal(
            8,
            semanticEntities
                .OfType<StepVertexPointEntity>()
                .Count());

        Assert.Equal(
            12,
            semanticEntities
                .OfType<StepEdgeCurveEntity>()
                .Count());

        Assert.Equal(
            24,
            semanticEntities
                .OfType<StepOrientedEdgeEntity>()
                .Count());

        StepEdgeLoopEntity[] edgeLoops =
            semanticEntities
                .OfType<StepEdgeLoopEntity>()
                .ToArray();

        Assert.Equal(
            6,
            edgeLoops.Length);

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var canonicalResolver =
            new StepCanonicalReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver,
                canonicalResolver);

        var edgeLoopMapper =
            new StepEdgeLoopMapper(
                new StepOrientedEdgeMapper());

        var canonicalLoops =
            new List<Loop>(
                edgeLoops.Length);

        foreach (StepEdgeLoopEntity edgeLoop
                 in edgeLoops)
        {
            MappingResult<Loop> result =
                edgeLoopMapper.Map(
                    edgeLoop,
                    context);

            Assert.True(
                result.IsSuccessful,
                CreateFailureMessage(
                    edgeLoop,
                    result.Diagnostics));

            Assert.NotEqual(
                MappingStatus.Failed,
                result.Status);

            Loop loop =
                Assert.IsType<Loop>(
                    result.Value);

            Assert.True(
                loop.EdgeUses.Count >= 3);

            AssertLoopContinuity(
                loop);

            AssertLoopClosure(
                loop);

            canonicalLoops.Add(
                loop);
        }

        Assert.Equal(
            6,
            canonicalLoops.Count);

        int totalEdgeUseCount =
            canonicalLoops.Sum(
                loop =>
                    loop.EdgeUses.Count);

        Assert.Equal(
            24,
            totalEdgeUseCount);

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
            12,
            uniqueCanonicalEdges.Length);

        Vertex[] uniqueCanonicalVertices =
            uniqueCanonicalEdges
                .SelectMany(
                    edge =>
                        new Vertex[]
                        {
                            edge.StartVertex,
                            edge.EndVertex
                        })
                .Distinct<Vertex>(
                    ReferenceEqualityComparer.Instance)
                .ToArray();

        Assert.Equal(
            8,
            uniqueCanonicalVertices.Length);

        Assert.Empty(
            context.Diagnostics);
    }

    private static void AssertLoopContinuity(
        Loop loop)
    {
        for (int index = 0;
             index < loop.EdgeUses.Count - 1;
             index++)
        {
            EdgeUse current =
                loop.EdgeUses[index];

            EdgeUse next =
                loop.EdgeUses[index + 1];

            Assert.Same(
                current.EndVertex,
                next.StartVertex);
        }
    }

    private static void AssertLoopClosure(
        Loop loop)
    {
        EdgeUse first =
            loop.EdgeUses[0];

        EdgeUse last =
            loop.EdgeUses[^1];

        Assert.Same(
            last.EndVertex,
            first.StartVertex);
    }

    private static string CreateFailureMessage(
        StepEdgeLoopEntity source,
        IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "EDGE_LOOP"
                : source.SourceEntityId;

        if (diagnostics.Count == 0)
        {
            return
                $"STEP EDGE_LOOP '{sourceIdentity}' mapping failed without diagnostics.";
        }

        string diagnosticText =
            string.Join(
                Environment.NewLine,
                diagnostics.Select(
                    diagnostic =>
                        $"{diagnostic.Code}: {diagnostic.Message}"));

        return
            $"STEP EDGE_LOOP '{sourceIdentity}' mapping failed." +
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