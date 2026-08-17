using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepGoldenCircleMappingIntegrationTests
{
    [Theory]
    [InlineData("T002_SPHERE.step", 2)]
    [InlineData("T003_CYLINDER.step", 4)]
    public void Map_GoldenFile_MapsAllCirclesThroughRealCanonicalPipeline(
        string fileName,
        int expectedCircleCount)
    {
        StepDataInventory inventory =
            ReadGoldenInventory(
                fileName);

        Assert.Equal(
            expectedCircleCount,
            inventory.GetCount(
                "CIRCLE"));

        List<StepSemanticEntity> semanticEntities =
            ReadSemanticEntities(
                inventory);

        StepCircleEntity[] sourceCircles =
            semanticEntities
                .OfType<StepCircleEntity>()
                .ToArray();

        Assert.Equal(
            expectedCircleCount,
            sourceCircles.Length);

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver);

        var mapper =
            new StepCircleGeometryMapper();

        var canonicalCircles =
            new List<Circle3D>(
                sourceCircles.Length);

        foreach (StepCircleEntity sourceCircle
                 in sourceCircles)
        {
            MappingResult<Circle3D> result =
                mapper.Map(
                    sourceCircle,
                    context);

            Assert.True(
                result.IsSuccessful,
                CreateFailureMessage(
                    fileName,
                    sourceCircle,
                    result.Diagnostics));

            Assert.Equal(
                MappingStatus.Succeeded,
                result.Status);

            Assert.Empty(
                result.Diagnostics);

            canonicalCircles.Add(
                Assert.IsType<Circle3D>(
                    result.Value));
        }

        Assert.Equal(
            expectedCircleCount,
            canonicalCircles.Count);

        Assert.Empty(
            context.Diagnostics);

        Assert.All(
            canonicalCircles,
            circle =>
            {
                Assert.Equal(
                    50.0,
                    circle.Radius,
                    precision:
                        10);

                double normalLengthSquared =
                    circle.Normal.X * circle.Normal.X +
                    circle.Normal.Y * circle.Normal.Y +
                    circle.Normal.Z * circle.Normal.Z;

                Assert.Equal(
                    1.0,
                    normalLengthSquared,
                    precision:
                        10);
            });
    }

    [Fact]
    public void Map_T002Sphere_PreservesGoldenCircleCentersAndNormals()
    {
        Circle3D[] circles =
            MapGoldenCircles(
                "T002_SPHERE.step");

        Assert.Equal(
            2,
            circles.Length);

        Assert.All(
            circles,
            circle =>
                Assert.Equal(
                    new Point3D(
                        0.0,
                        0.0,
                        0.0),
                    circle.Center));

        Assert.Contains(
            circles,
            circle =>
                Math.Abs(
                    circle.Normal.Z - 1.0) <
                1e-10);

        Assert.Contains(
            circles,
            circle =>
                Math.Abs(
                    circle.Normal.Z + 1.0) <
                1e-10);
    }

    [Fact]
    public void Map_T003Cylinder_PreservesGoldenCircleCentersAndNormals()
    {
        Circle3D[] circles =
            MapGoldenCircles(
                "T003_CYLINDER.step");

        Assert.Equal(
            4,
            circles.Length);

        Assert.Equal(
            2,
            circles.Count(
                circle =>
                    Math.Abs(
                        circle.Center.Z) <
                    1e-10));

        Assert.Equal(
            2,
            circles.Count(
                circle =>
                    Math.Abs(
                        circle.Center.Z - 50.0) <
                    1e-10));

        Assert.All(
            circles,
            circle =>
            {
                Assert.Equal(
                    0.0,
                    circle.Center.X,
                    precision:
                        10);

                Assert.Equal(
                    0.0,
                    circle.Center.Y,
                    precision:
                        10);

                Assert.Equal(
                    new Vector3D(
                        0.0,
                        0.0,
                        1.0),
                    circle.Normal);
            });
    }

    private static Circle3D[] MapGoldenCircles(
        string fileName)
    {
        StepDataInventory inventory =
            ReadGoldenInventory(
                fileName);

        List<StepSemanticEntity> semanticEntities =
            ReadSemanticEntities(
                inventory);

        StepCircleEntity[] sourceCircles =
            semanticEntities
                .OfType<StepCircleEntity>()
                .ToArray();

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver);

        var mapper =
            new StepCircleGeometryMapper();

        return sourceCircles
            .Select(
                sourceCircle =>
                {
                    MappingResult<Circle3D> result =
                        mapper.Map(
                            sourceCircle,
                            context);

                    Assert.True(
                        result.IsSuccessful,
                        CreateFailureMessage(
                            fileName,
                            sourceCircle,
                            result.Diagnostics));

                    Assert.Empty(
                        result.Diagnostics);

                    return Assert.IsType<Circle3D>(
                        result.Value);
                })
            .ToArray();
    }

    private static StepDataInventory ReadGoldenInventory(
        string fileName)
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

        return Assert.IsType<StepDataInventory>(
            dataResult.Value);
    }

    private static List<StepSemanticEntity> ReadSemanticEntities(
        StepDataInventory inventory)
    {
        IStepSemanticEntityHandler[] handlers =
        [
            new StepCartesianPointEntityHandler(),
            new StepDirectionEntityHandler(),
            new StepAxis2Placement3DEntityHandler(),
            new StepCircleEntityHandler()
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

        return semanticEntities;
    }

    private static string CreateFailureMessage(
        string fileName,
        StepCircleEntity source,
        IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "CIRCLE"
                : source.SourceEntityId;

        if (diagnostics.Count == 0)
        {
            return
                $"STEP CIRCLE '{sourceIdentity}' from '{fileName}' " +
                "mapping failed without diagnostics.";
        }

        string diagnosticText =
            string.Join(
                Environment.NewLine,
                diagnostics.Select(
                    diagnostic =>
                        $"{diagnostic.Code}: {diagnostic.Message}"));

        return
            $"STEP CIRCLE '{sourceIdentity}' from '{fileName}' mapping failed." +
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