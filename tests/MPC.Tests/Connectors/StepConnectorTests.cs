using MPC.Connectors.Results;
using MPC.Connectors.Snapshots.Units;
using MPC.Connectors.Step;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Resolution;

namespace MPC.Tests.Connectors;

public sealed class StepConnectorTests
{
    [Fact]
    public void Read_WithValidAp214FileAndResolvedUnits_ReturnsSucceededSnapshot()
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
                    'GoldenPart.step',
                    '2026-07-14T22:00:00',
                    ('MPC'),
                    ('MPC Platform'),
                    'MPC STEP Writer',
                    'MPC Platform',
                    '');
                FILE_SCHEMA(
                    ('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #16 =(
                    LENGTH_UNIT()
                    NAMED_UNIT(*)
                    SI_UNIT(.MILLI.,.METRE.)
                );
                #38 =(
                    NAMED_UNIT(*)
                    PLANE_ANGLE_UNIT()
                    SI_UNIT($,.RADIAN.)
                );
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var connector =
                CreateConnector();

            var result =
                connector.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.NotNull(result.Value);

            Assert.Equal(
                "STEP",
                result.Value.Source.SourceSystem);

            Assert.Equal(
                "AP214",
                result.Value.Source.SourceFormat);

            Assert.Equal(
                "GoldenPart.step",
                result.Value.Source.SourceFileName);

            Assert.Equal(
                "2;1",
                result.Value.Source.SourceVersion);

            Assert.Equal(
                "0.2.0",
                result.Value.Source.ConnectorVersion);

            Assert.Equal(
                CadLengthUnit.Millimetre,
                result.Value.Units.SourceLengthUnit);

            Assert.Equal(
                CadAngleUnit.Radian,
                result.Value.Units.SourceAngleUnit);

            Assert.Equal(
                CadLengthUnit.Metre,
                result.Value.Units.InternalLengthUnit);

            Assert.Equal(
                CadAngleUnit.Radian,
                result.Value.Units.InternalAngleUnit);

            Assert.Equal(
                0,
                result.Value.EntityCount);

            Assert.Empty(
                result.Diagnostics);

            Assert.Empty(
                result.Value.Diagnostics);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithMissingUnits_ReturnsSucceededWithDiagnostics()
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
                    'GoldenPart.step',
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
                #1 = APPLICATION_CONTEXT('');
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var connector =
                CreateConnector();

            var result =
                connector.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            Assert.NotNull(result.Value);

            Assert.Equal(
                CadLengthUnit.Unknown,
                result.Value.Units.SourceLengthUnit);

            Assert.Equal(
                CadAngleUnit.Unknown,
                result.Value.Units.SourceAngleUnit);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_LENGTH_UNIT_NOT_RESOLVED");

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_ANGLE_UNIT_NOT_RESOLVED");

            Assert.DoesNotContain(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_SOURCE_UNITS_NOT_READ");
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithMissingFile_PropagatesHeaderFailure()
    {
        var connector =
            CreateConnector();

        var filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.step");

        var result =
            connector.Read(filePath);

        Assert.Equal(
            CadConnectorResultStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        Assert.Single(
            result.Diagnostics);

        Assert.Equal(
            "STEP_FILE_NOT_FOUND",
            result.Diagnostics[0].Code);
    }

    [Fact]
    public void Read_WithoutDataSection_PropagatesDataFailure()
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
                    'GoldenPart.step',
                    '',
                    ('MPC'),
                    ('MPC'),
                    '',
                    '',
                    '');
                FILE_SCHEMA(
                    ('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var connector =
                CreateConnector();

            var result =
                connector.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Failed,
                result.Status);

            Assert.Null(
                result.Value);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_DATA_SECTION_NOT_FOUND");
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Constructor_WithNullHeaderReader_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepConnector(
                headerReader: null!,
                dataReader:
                    new StepDataEntityReader(),
                unitResolver:
                    new StepUnitResolver()));
    }

    [Fact]
    public void Constructor_WithNullDataReader_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepConnector(
                headerReader:
                    new StepHeaderReader(),
                dataReader: null!,
                unitResolver:
                    new StepUnitResolver()));
    }

    [Fact]
    public void Constructor_WithNullUnitResolver_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepConnector(
                headerReader:
                    new StepHeaderReader(),
                dataReader:
                    new StepDataEntityReader(),
                unitResolver: null!));
    }

    private static StepConnector CreateConnector()
    {
        return new StepConnector(
            headerReader:
                new StepHeaderReader(),
            dataReader:
                new StepDataEntityReader(),
            unitResolver:
                new StepUnitResolver());
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