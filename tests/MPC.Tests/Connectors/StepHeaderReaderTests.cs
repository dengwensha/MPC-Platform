using MPC.Connectors.Results;
using MPC.Connectors.Step.Readers;

namespace MPC.Tests.Connectors;

public sealed class StepHeaderReaderTests
{
    [Fact]
    public void Read_WithValidStepHeader_ReturnsSucceededResult()
    {
        var filePath = CreateTemporaryStepFile(
            """
            ISO-10303-21;
            HEADER;
            FILE_DESCRIPTION(
                ('ISO 10303-21'),
                '2;1');
            FILE_NAME(
                'GoldenPart.step',
                '2026-07-14T22:00:00',
                ('MPC'),
                ('MPC Platform'),
                'MPC STEP Writer',
                'MPC Platform',
                '');
            FILE_SCHEMA(
                ('AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF'));
            ENDSEC;
            DATA;
            ENDSEC;
            END-ISO-10303-21;
            """);

        try
        {
            var reader = new StepHeaderReader();

            var result = reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.NotNull(result.Value);

            Assert.Equal(
                "GoldenPart.step",
                result.Value.FileName);

            Assert.Equal(
                "2;1",
                result.Value.ImplementationLevel);

            Assert.Single(result.Value.Descriptions);
            Assert.Single(result.Value.Schemas);

            Assert.Equal(
                "AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF",
                result.Value.Schemas[0]);

            Assert.Empty(result.Diagnostics);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithEscapedApostrophe_DecodesStepString()
    {
        var filePath = CreateTemporaryStepFile(
            """
            ISO-10303-21;
            HEADER;
            FILE_DESCRIPTION(('Author''s model'),'2;1');
            FILE_NAME(
                'GoldenPart.step',
                '',
                ('MPC'),
                ('MPC'),
                '',
                '',
                '');
            FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
            ENDSEC;
            DATA;
            ENDSEC;
            END-ISO-10303-21;
            """);

        try
        {
            var reader = new StepHeaderReader();

            var result = reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.Equal(
                "Author's model",
                result.Value!.Descriptions[0]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithMissingFile_ReturnsFailedResult()
    {
        var reader = new StepHeaderReader();

        var result = reader.Read(
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.step"));

        Assert.Equal(
            CadConnectorResultStatus.Failed,
            result.Status);

        Assert.Null(result.Value);
        Assert.Single(result.Diagnostics);

        Assert.Equal(
            "STEP_FILE_NOT_FOUND",
            result.Diagnostics[0].Code);
    }

    [Fact]
    public void Read_WithUnsupportedExtension_ReturnsFailedResult()
    {
        var filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.txt");

        File.WriteAllText(
            filePath,
            "ISO-10303-21;");

        try
        {
            var reader = new StepHeaderReader();

            var result = reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Failed,
                result.Status);

            Assert.Equal(
                "STEP_FILE_EXTENSION_UNSUPPORTED",
                result.Diagnostics[0].Code);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithoutHeaderSection_ReturnsFailedResult()
    {
        var filePath = CreateTemporaryStepFile(
            """
            ISO-10303-21;
            DATA;
            ENDSEC;
            END-ISO-10303-21;
            """);

        try
        {
            var reader = new StepHeaderReader();

            var result = reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Failed,
                result.Status);

            Assert.Equal(
                "STEP_HEADER_NOT_FOUND",
                result.Diagnostics[0].Code);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithoutFileSchema_ReturnsFailedResult()
    {
        var filePath = CreateTemporaryStepFile(
            """
            ISO-10303-21;
            HEADER;
            FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
            FILE_NAME(
                'GoldenPart.step',
                '',
                ('MPC'),
                ('MPC'),
                '',
                '',
                '');
            ENDSEC;
            DATA;
            ENDSEC;
            END-ISO-10303-21;
            """);

        try
        {
            var reader = new StepHeaderReader();

            var result = reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Failed,
                result.Status);

            Assert.Equal(
                "STEP_HEADER_PARSE_FAILED",
                result.Diagnostics[0].Code);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static string CreateTemporaryStepFile(
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