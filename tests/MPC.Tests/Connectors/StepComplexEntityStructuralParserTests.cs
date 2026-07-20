using MPC.Connectors.Results;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Tests.Connectors;

public sealed class StepComplexEntityStructuralParserTests
{
    [Fact]
    public void Read_WithComponentLikeTextInsideString_DoesNotCreateFalseComponent()
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
                    'ComplexEntityString.step',
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
                #50 = (
                    REPRESENTATION_ITEM(
                        'TEXT FAKE_COMPONENT('
                    ),
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
                "#50",
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

            Assert.DoesNotContain(
                "FAKE_COMPONENT",
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
    public void Read_WithNestedTypedParameter_DoesNotTreatNestedTypeAsComponent()
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
                    'ComplexEntityNestedType.step',
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
                #51 = (
                    REPRESENTATION_ITEM(''),
                    SURFACE(
                        LENGTH_MEASURE(1.0)
                    )
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
                "#51",
                entity.SourceEntityId);

            Assert.Equal(
                "COMPLEX_ENTITY",
                entity.SourceEntityType);

            Assert.Equal(
                new[]
                {
                    "REPRESENTATION_ITEM",
                    "SURFACE"
                },
                entity.ComponentTypes);

            Assert.DoesNotContain(
                "LENGTH_MEASURE",
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