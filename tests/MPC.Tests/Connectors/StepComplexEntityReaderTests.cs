using MPC.Connectors.Results;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Tests.Connectors;

public sealed class StepComplexEntityReaderTests
{
    [Fact]
    public void Read_WithComplexEntity_PreservesComponentTypesInSourceOrder()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(
                    ('ISO 10303-21'),
                    '2;1');
                FILE_NAME(
                    'ComplexEntity.step',
                    '',
                    ('MPC'),
                    ('MPC'),
                    '',
                    '',
                    '');
                FILE_SCHEMA(
                    ('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #42 = (
                    REPRESENTATION_ITEM(''),
                    GEOMETRIC_REPRESENTATION_ITEM(),
                    SURFACE()
                );
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(
                    filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.NotNull(
                result.Value);

            var entity =
                Assert.Single(
                    result.Value.Entities);

            Assert.Equal(
                "#42",
                entity.SourceEntityId);

            Assert.Equal(
                "COMPLEX_ENTITY",
                entity.SourceEntityType);

            Assert.Equal(
                new[]
                {
                    "REPRESENTATION_ITEM",
                    "GEOMETRIC_REPRESENTATION_ITEM",
                    "SURFACE"
                },
                entity.ComponentTypes);

            Assert.Empty(
                result.Diagnostics);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Read_WithRepeatedComplexComponent_PreservesEveryOccurrence()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(
                    ('ISO 10303-21'),
                    '2;1');
                FILE_NAME(
                    'RepeatedComponent.step',
                    '',
                    ('MPC'),
                    ('MPC'),
                    '',
                    '',
                    '');
                FILE_SCHEMA(
                    ('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #43 = (
                    REPRESENTATION_ITEM('A'),
                    GEOMETRIC_REPRESENTATION_ITEM(),
                    REPRESENTATION_ITEM('B')
                );
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(
                    filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.NotNull(
                result.Value);

            var entity =
                Assert.Single(
                    result.Value.Entities);

            Assert.Equal(
                "#43",
                entity.SourceEntityId);

            Assert.Equal(
                "COMPLEX_ENTITY",
                entity.SourceEntityType);

            Assert.Equal(
                3,
                entity.ComponentTypes.Count);

            Assert.Equal(
                "REPRESENTATION_ITEM",
                entity.ComponentTypes[0]);

            Assert.Equal(
                "GEOMETRIC_REPRESENTATION_ITEM",
                entity.ComponentTypes[1]);

            Assert.Equal(
                "REPRESENTATION_ITEM",
                entity.ComponentTypes[2]);

            Assert.Empty(
                result.Diagnostics);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    private static StepDataEntityReader
        CreateReader()
    {
        return new StepDataEntityReader(
            inventoryValidator:
                new StepDataInventoryValidator());
    }

    private static string
        CreateTemporaryStepFile(
            string content)
    {
        var filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.step");

        File.WriteAllText(
            filePath,
            content);

        return filePath;
    }
}