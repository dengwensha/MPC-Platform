using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Tests.Connectors;

public sealed class GoldenStepDataInventoryTests
{
    [Theory]
    [InlineData("T001_PRISM.step")]
    [InlineData("T002_SPHERE.step")]
    [InlineData("T003_CYLINDER.step")]
    [InlineData("T101_BASIC_POCKET.step")]
    [InlineData("T103_MULTIPLE_ISLANDS.step")]
    public void Read_GoldenStepFile_ShouldProduceCompleteInventory(
        string fileName)
    {
        var filePath =
            GetGoldenStepFilePath(
                fileName);

        var reader =
            new StepDataEntityReader(
                new StepDataInventoryValidator());

        var result =
            reader.Read(
                filePath);

        Assert.NotEqual(
            CadConnectorResultStatus.Failed,
            result.Status);

        var inventory =
            Assert.IsType<StepDataInventory>(
                result.Value);

        Assert.NotEmpty(
            inventory.Entities);

        Assert.Equal(
            inventory.Entities.Count,
            inventory.TotalEntityCount);

        Assert.All(
            inventory.Entities,
            entity =>
            {
                Assert.False(
                    string.IsNullOrWhiteSpace(
                        entity.RawStatement));

                Assert.False(
                    string.IsNullOrWhiteSpace(
                        entity.SourceEntityType));
            });
    }

    [Theory]
    [InlineData("T001_PRISM.step")]
    [InlineData("T002_SPHERE.step")]
    [InlineData("T003_CYLINDER.step")]
    [InlineData("T101_BASIC_POCKET.step")]
    [InlineData("T103_MULTIPLE_ISLANDS.step")]
    public void Read_GoldenStepFile_ShouldMatchGoldenInventoryBaseline(
        string fileName)
    {
        var expected =
            GetExpectedBaseline(
                fileName);

        var filePath =
            GetGoldenStepFilePath(
                fileName);

        var reader =
            new StepDataEntityReader(
                new StepDataInventoryValidator());

        var result =
            reader.Read(
                filePath);

        Assert.Equal(
            CadConnectorResultStatus.Succeeded,
            result.Status);

        Assert.Equal(
            expected.ResultDiagnosticCount,
            result.Diagnostics.Count);

        var inventory =
            Assert.IsType<StepDataInventory>(
                result.Value);

        Assert.Equal(
            expected.TotalEntityCount,
            inventory.TotalEntityCount);

        Assert.Equal(
            expected.TotalEntityCount,
            inventory.Entities.Count);

        var entityDiagnosticCount =
            inventory.Entities.Sum(
                entity => entity.Diagnostics.Count);

        Assert.Equal(
            expected.EntityDiagnosticCount,
            entityDiagnosticCount);

        Assert.Equal(
            expected.TypeCounts.Count,
            inventory.TypeCounts.Count);

        foreach (var expectedTypeCount in expected.TypeCounts)
        {
            Assert.True(
                inventory.TypeCounts.ContainsKey(
                    expectedTypeCount.Key),
                $"Expected STEP entity type was not found. " +
                $"File: '{fileName}', " +
                $"Type: '{expectedTypeCount.Key}'.");

            Assert.Equal(
                expectedTypeCount.Value,
                inventory.GetCount(
                    expectedTypeCount.Key));
        }

        var actualReadStatusDistribution =
            inventory.Entities
                .GroupBy(
                    entity => entity.ReadStatus.ToString())
                .ToDictionary(
                    group => group.Key,
                    group => group.Count(),
                    StringComparer.Ordinal);

        Assert.Equal(
            expected.ReadStatusDistribution.Count,
            actualReadStatusDistribution.Count);

        foreach (var expectedStatusCount in expected.ReadStatusDistribution)
        {
            Assert.True(
                actualReadStatusDistribution.ContainsKey(
                    expectedStatusCount.Key),
                $"Expected read status was not found. " +
                $"File: '{fileName}', " +
                $"Status: '{expectedStatusCount.Key}'.");

            Assert.Equal(
                expectedStatusCount.Value,
                actualReadStatusDistribution[
                    expectedStatusCount.Key]);
        }
    }

    private static GoldenInventoryBaseline GetExpectedBaseline(
        string fileName)
    {
        return fileName switch
        {
            "T001_PRISM.step" =>
                new GoldenInventoryBaseline(
                    totalEntityCount: 187,
                    resultDiagnosticCount: 0,
                    entityDiagnosticCount: 0,
                    typeCounts:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["ADVANCED_BREP_SHAPE_REPRESENTATION"] = 1,
                        ["ADVANCED_FACE"] = 6,
                        ["APPLICATION_CONTEXT"] = 2,
                        ["APPLICATION_PROTOCOL_DEFINITION"] = 2,
                        ["AXIS2_PLACEMENT_3D"] = 7,
                        ["CARTESIAN_POINT"] = 27,
                        ["CLOSED_SHELL"] = 1,
                        ["COLOUR_RGB"] = 1,
                        ["COMPLEX_ENTITY"] = 8,
                        ["DIRECTION"] = 26,
                        ["EDGE_CURVE"] = 12,
                        ["EDGE_LOOP"] = 6,
                        ["FACE_OUTER_BOUND"] = 6,
                        ["FILL_AREA_STYLE"] = 1,
                        ["FILL_AREA_STYLE_COLOUR"] = 1,
                        ["LINE"] = 12,
                        ["MANIFOLD_SOLID_BREP"] = 1,
                        ["MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION"] = 1,
                        ["ORIENTED_EDGE"] = 24,
                        ["PLANE"] = 6,
                        ["PRESENTATION_LAYER_ASSIGNMENT"] = 1,
                        ["PRESENTATION_STYLE_ASSIGNMENT"] = 1,
                        ["PRODUCT"] = 1,
                        ["PRODUCT_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION"] = 1,
                        ["PRODUCT_DEFINITION_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE"] = 1,
                        ["PRODUCT_DEFINITION_SHAPE"] = 1,
                        ["PRODUCT_RELATED_PRODUCT_CATEGORY"] = 1,
                        ["SHAPE_DEFINITION_REPRESENTATION"] = 1,
                        ["STYLED_ITEM"] = 1,
                        ["SURFACE_SIDE_STYLE"] = 1,
                        ["SURFACE_STYLE_FILL_AREA"] = 1,
                        ["SURFACE_STYLE_USAGE"] = 1,
                        ["UNCERTAINTY_MEASURE_WITH_UNIT"] = 2,
                        ["VECTOR"] = 12,
                        ["VERTEX_POINT"] = 8
                    },
                    readStatusDistribution:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["Complete"] = 187
                    }),

            "T002_SPHERE.step" =>
                new GoldenInventoryBaseline(
                    totalEntityCount: 75,
                    resultDiagnosticCount: 0,
                    entityDiagnosticCount: 0,
                    typeCounts:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["ADVANCED_BREP_SHAPE_REPRESENTATION"] = 1,
                        ["ADVANCED_FACE"] = 2,
                        ["APPLICATION_CONTEXT"] = 2,
                        ["APPLICATION_PROTOCOL_DEFINITION"] = 2,
                        ["AXIS2_PLACEMENT_3D"] = 5,
                        ["CARTESIAN_POINT"] = 7,
                        ["CIRCLE"] = 2,
                        ["CLOSED_SHELL"] = 1,
                        ["COLOUR_RGB"] = 1,
                        ["COMPLEX_ENTITY"] = 8,
                        ["DIRECTION"] = 10,
                        ["EDGE_CURVE"] = 2,
                        ["EDGE_LOOP"] = 2,
                        ["FACE_OUTER_BOUND"] = 2,
                        ["FILL_AREA_STYLE"] = 1,
                        ["FILL_AREA_STYLE_COLOUR"] = 1,
                        ["MANIFOLD_SOLID_BREP"] = 1,
                        ["MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION"] = 1,
                        ["ORIENTED_EDGE"] = 4,
                        ["PRESENTATION_LAYER_ASSIGNMENT"] = 1,
                        ["PRESENTATION_STYLE_ASSIGNMENT"] = 1,
                        ["PRODUCT"] = 1,
                        ["PRODUCT_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION"] = 1,
                        ["PRODUCT_DEFINITION_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE"] = 1,
                        ["PRODUCT_DEFINITION_SHAPE"] = 1,
                        ["PRODUCT_RELATED_PRODUCT_CATEGORY"] = 1,
                        ["SHAPE_DEFINITION_REPRESENTATION"] = 1,
                        ["SPHERICAL_SURFACE"] = 2,
                        ["STYLED_ITEM"] = 1,
                        ["SURFACE_SIDE_STYLE"] = 1,
                        ["SURFACE_STYLE_FILL_AREA"] = 1,
                        ["SURFACE_STYLE_USAGE"] = 1,
                        ["UNCERTAINTY_MEASURE_WITH_UNIT"] = 2,
                        ["VERTEX_POINT"] = 2
                    },
                    readStatusDistribution:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["Complete"] = 75
                    }),

            "T003_CYLINDER.step" =>
                new GoldenInventoryBaseline(
                    totalEntityCount: 125,
                    resultDiagnosticCount: 0,
                    entityDiagnosticCount: 0,
                    typeCounts:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["ADVANCED_BREP_SHAPE_REPRESENTATION"] = 1,
                        ["ADVANCED_FACE"] = 4,
                        ["APPLICATION_CONTEXT"] = 2,
                        ["APPLICATION_PROTOCOL_DEFINITION"] = 2,
                        ["AXIS2_PLACEMENT_3D"] = 9,
                        ["CARTESIAN_POINT"] = 15,
                        ["CIRCLE"] = 4,
                        ["CLOSED_SHELL"] = 1,
                        ["COLOUR_RGB"] = 1,
                        ["COMPLEX_ENTITY"] = 8,
                        ["CYLINDRICAL_SURFACE"] = 2,
                        ["DIRECTION"] = 20,
                        ["EDGE_CURVE"] = 6,
                        ["EDGE_LOOP"] = 4,
                        ["FACE_OUTER_BOUND"] = 4,
                        ["FILL_AREA_STYLE"] = 1,
                        ["FILL_AREA_STYLE_COLOUR"] = 1,
                        ["LINE"] = 2,
                        ["MANIFOLD_SOLID_BREP"] = 1,
                        ["MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION"] = 1,
                        ["ORIENTED_EDGE"] = 12,
                        ["PLANE"] = 2,
                        ["PRESENTATION_LAYER_ASSIGNMENT"] = 1,
                        ["PRESENTATION_STYLE_ASSIGNMENT"] = 1,
                        ["PRODUCT"] = 1,
                        ["PRODUCT_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION"] = 1,
                        ["PRODUCT_DEFINITION_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE"] = 1,
                        ["PRODUCT_DEFINITION_SHAPE"] = 1,
                        ["PRODUCT_RELATED_PRODUCT_CATEGORY"] = 1,
                        ["SHAPE_DEFINITION_REPRESENTATION"] = 1,
                        ["STYLED_ITEM"] = 1,
                        ["SURFACE_SIDE_STYLE"] = 1,
                        ["SURFACE_STYLE_FILL_AREA"] = 1,
                        ["SURFACE_STYLE_USAGE"] = 1,
                        ["UNCERTAINTY_MEASURE_WITH_UNIT"] = 2,
                        ["VECTOR"] = 2,
                        ["VERTEX_POINT"] = 4
                    },
                    readStatusDistribution:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["Complete"] = 125
                    }),

            "T101_BASIC_POCKET.step" =>
                new GoldenInventoryBaseline(
                    totalEntityCount: 329,
                    resultDiagnosticCount: 0,
                    entityDiagnosticCount: 0,
                    typeCounts:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["ADVANCED_BREP_SHAPE_REPRESENTATION"] = 1,
                        ["ADVANCED_FACE"] = 11,
                        ["APPLICATION_CONTEXT"] = 2,
                        ["APPLICATION_PROTOCOL_DEFINITION"] = 2,
                        ["AXIS2_PLACEMENT_3D"] = 12,
                        ["CARTESIAN_POINT"] = 52,
                        ["CLOSED_SHELL"] = 1,
                        ["COLOUR_RGB"] = 1,
                        ["COMPLEX_ENTITY"] = 8,
                        ["DIRECTION"] = 48,
                        ["EDGE_CURVE"] = 24,
                        ["EDGE_LOOP"] = 12,
                        ["FACE_BOUND"] = 1,
                        ["FACE_OUTER_BOUND"] = 11,
                        ["FILL_AREA_STYLE"] = 1,
                        ["FILL_AREA_STYLE_COLOUR"] = 1,
                        ["LINE"] = 24,
                        ["MANIFOLD_SOLID_BREP"] = 1,
                        ["MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION"] = 1,
                        ["ORIENTED_EDGE"] = 48,
                        ["PLANE"] = 11,
                        ["PRESENTATION_LAYER_ASSIGNMENT"] = 1,
                        ["PRESENTATION_STYLE_ASSIGNMENT"] = 1,
                        ["PRODUCT"] = 1,
                        ["PRODUCT_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION"] = 1,
                        ["PRODUCT_DEFINITION_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE"] = 1,
                        ["PRODUCT_DEFINITION_SHAPE"] = 1,
                        ["PRODUCT_RELATED_PRODUCT_CATEGORY"] = 1,
                        ["SHAPE_DEFINITION_REPRESENTATION"] = 1,
                        ["STYLED_ITEM"] = 1,
                        ["SURFACE_SIDE_STYLE"] = 1,
                        ["SURFACE_STYLE_FILL_AREA"] = 1,
                        ["SURFACE_STYLE_USAGE"] = 1,
                        ["UNCERTAINTY_MEASURE_WITH_UNIT"] = 2,
                        ["VECTOR"] = 24,
                        ["VERTEX_POINT"] = 16
                    },
                    readStatusDistribution:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["Complete"] = 329
                    }),

            "T103_MULTIPLE_ISLANDS.step" =>
                new GoldenInventoryBaseline(
                    totalEntityCount: 755,
                    resultDiagnosticCount: 0,
                    entityDiagnosticCount: 0,
                    typeCounts:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["ADVANCED_BREP_SHAPE_REPRESENTATION"] = 1,
                        ["ADVANCED_FACE"] = 26,
                        ["APPLICATION_CONTEXT"] = 2,
                        ["APPLICATION_PROTOCOL_DEFINITION"] = 2,
                        ["AXIS2_PLACEMENT_3D"] = 27,
                        ["CARTESIAN_POINT"] = 127,
                        ["CLOSED_SHELL"] = 1,
                        ["COLOUR_RGB"] = 1,
                        ["COMPLEX_ENTITY"] = 8,
                        ["DIRECTION"] = 114,
                        ["EDGE_CURVE"] = 60,
                        ["EDGE_LOOP"] = 30,
                        ["FACE_BOUND"] = 4,
                        ["FACE_OUTER_BOUND"] = 26,
                        ["FILL_AREA_STYLE"] = 1,
                        ["FILL_AREA_STYLE_COLOUR"] = 1,
                        ["LINE"] = 60,
                        ["MANIFOLD_SOLID_BREP"] = 1,
                        ["MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION"] = 1,
                        ["ORIENTED_EDGE"] = 120,
                        ["PLANE"] = 26,
                        ["PRESENTATION_LAYER_ASSIGNMENT"] = 1,
                        ["PRESENTATION_STYLE_ASSIGNMENT"] = 1,
                        ["PRODUCT"] = 1,
                        ["PRODUCT_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION"] = 1,
                        ["PRODUCT_DEFINITION_CONTEXT"] = 1,
                        ["PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE"] = 1,
                        ["PRODUCT_DEFINITION_SHAPE"] = 1,
                        ["PRODUCT_RELATED_PRODUCT_CATEGORY"] = 1,
                        ["SHAPE_DEFINITION_REPRESENTATION"] = 1,
                        ["STYLED_ITEM"] = 1,
                        ["SURFACE_SIDE_STYLE"] = 1,
                        ["SURFACE_STYLE_FILL_AREA"] = 1,
                        ["SURFACE_STYLE_USAGE"] = 1,
                        ["UNCERTAINTY_MEASURE_WITH_UNIT"] = 2,
                        ["VECTOR"] = 60,
                        ["VERTEX_POINT"] = 40
                    },
                    readStatusDistribution:
                    new Dictionary<string, int>(
                        StringComparer.Ordinal)
                    {
                        ["Complete"] = 755
                    }),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(fileName),
                    fileName,
                    "No Golden STEP inventory baseline is defined for this file.")
        };
    }

    private static string GetGoldenStepFilePath(
        string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
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
            var candidatePath =
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

    private sealed class GoldenInventoryBaseline
    {
        public GoldenInventoryBaseline(
            int totalEntityCount,
            int resultDiagnosticCount,
            int entityDiagnosticCount,
            IReadOnlyDictionary<string, int> typeCounts,
            IReadOnlyDictionary<string, int> readStatusDistribution)
        {
            TotalEntityCount =
                totalEntityCount;

            ResultDiagnosticCount =
                resultDiagnosticCount;

            EntityDiagnosticCount =
                entityDiagnosticCount;

            TypeCounts =
                typeCounts
                ?? throw new ArgumentNullException(
                    nameof(typeCounts));

            ReadStatusDistribution =
                readStatusDistribution
                ?? throw new ArgumentNullException(
                    nameof(readStatusDistribution));
        }

        public int TotalEntityCount { get; }

        public int ResultDiagnosticCount { get; }

        public int EntityDiagnosticCount { get; }

        public IReadOnlyDictionary<string, int> TypeCounts { get; }

        public IReadOnlyDictionary<string, int> ReadStatusDistribution { get; }
    }
}