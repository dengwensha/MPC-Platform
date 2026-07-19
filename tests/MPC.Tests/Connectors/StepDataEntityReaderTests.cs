using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Tests.Connectors;

public sealed class StepDataEntityReaderTests
{
    [Fact]
    public void Read_WithValidDataSection_CreatesEntityInventory()
    {
        var filePath =
            CreateTemporaryStepFile(
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
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #1 = APPLICATION_CONTEXT('configuration controlled 3d designs');
                #2 = PRODUCT('PART','PART','',(#1));
                #42 = ADVANCED_FACE('',(#10),#20,.T.);
                #43 = CYLINDRICAL_SURFACE('',#44,0.025);
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.NotNull(
                result.Value);

            Assert.Equal(
                4,
                result.Value.TotalEntityCount);

            Assert.Equal(
                1,
                result.Value.GetCount(
                    "APPLICATION_CONTEXT"));

            Assert.Equal(
                1,
                result.Value.GetCount(
                    "PRODUCT"));

            Assert.Equal(
                1,
                result.Value.GetCount(
                    "ADVANCED_FACE"));

            Assert.Equal(
                1,
                result.Value.GetCount(
                    "CYLINDRICAL_SURFACE"));

            Assert.All(
                result.Value.Entities,
                entity =>
                    Assert.Equal(
                        CadReadStatus.Complete,
                        entity.ReadStatus));

            Assert.Empty(
                result.Diagnostics);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithMultilineStatement_PreservesCompleteRawStatement()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #100 = CYLINDRICAL_SURFACE(
                    'Cylinder Surface',
                    #101,
                    0.025);
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            var entity =
                Assert.Single(
                    result.Value!.Entities);

            Assert.Equal(
                "#100",
                entity.SourceEntityId);

            Assert.Equal(
                "CYLINDRICAL_SURFACE",
                entity.SourceEntityType);

            Assert.Contains(
                "0.025",
                entity.RawStatement);

            Assert.EndsWith(
                ";",
                entity.RawStatement);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithSemicolonInsideString_DoesNotSplitEntity()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #1 = PRODUCT('PART;A','PART','',(#2));
                #2 = APPLICATION_CONTEXT('');
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.Equal(
                2,
                result.Value!.TotalEntityCount);

            Assert.Contains(
                "'PART;A'",
                result.Value
                    .Entities[0]
                    .RawStatement);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithUnrecognizedStatement_PreservesStatementAndReportsDiagnostic()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #1 = PRODUCT('PART','PART','',(#2));
                INVALID DATA RECORD;
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            Assert.Equal(
                2,
                result.Value!.TotalEntityCount);

            var unknown =
                Assert.Single(
                    result.Value.Entities,
                    entity =>
                        entity.SourceEntityType ==
                        "UNKNOWN");

            Assert.Equal(
                CadReadStatus.Partial,
                unknown.ReadStatus);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_DATA_ENTITY_HEADER_UNRECOGNIZED");
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithDuplicateEntityIds_PreservesAllEntitiesAndReportsDiagnostic()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #25 = CARTESIAN_POINT('',(0.0,0.0,0.0));
                #25 = DIRECTION('',(1.0,0.0,0.0));
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            Assert.NotNull(
                result.Value);

            Assert.Equal(
                2,
                result.Value.TotalEntityCount);

            Assert.Equal(
                2,
                result.Value.Entities.Count(
                    entity =>
                        entity.SourceEntityId ==
                        "#25"));

            Assert.Contains(
                result.Value.Entities,
                entity =>
                    entity.SourceEntityType ==
                    "CARTESIAN_POINT");

            Assert.Contains(
                result.Value.Entities,
                entity =>
                    entity.SourceEntityType ==
                    "DIRECTION");

            var diagnostic =
                Assert.Single(
                    result.Diagnostics,
                    item =>
                        item.Code ==
                        "STEP_DUPLICATE_SOURCE_ENTITY_ID");

            Assert.Equal(
                CadDiagnosticSeverity.Error,
                diagnostic.Severity);

            Assert.Equal(
                "#25",
                diagnostic.SourceEntityId);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithEquivalentNumericEntityIds_ReportsDuplicate()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                #25 = CARTESIAN_POINT('',(0.0,0.0,0.0));
                #025 = DIRECTION('',(1.0,0.0,0.0));
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            Assert.Equal(
                2,
                result.Value!.TotalEntityCount);

            var diagnostic =
                Assert.Single(
                    result.Diagnostics,
                    item =>
                        item.Code ==
                        "STEP_DUPLICATE_SOURCE_ENTITY_ID");

            Assert.Equal(
                "#25",
                diagnostic.SourceEntityId);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithoutDataSection_ReturnsFailedResult()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Failed,
                result.Status);

            Assert.Equal(
                "STEP_DATA_SECTION_NOT_FOUND",
                result.Diagnostics[0].Code);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Read_WithEmptyDataSection_CreatesEmptyInventory()
    {
        var filePath =
            CreateTemporaryStepFile(
                """
                ISO-10303-21;
                HEADER;
                FILE_DESCRIPTION(('ISO 10303-21'),'2;1');
                FILE_NAME('Part.step','',('MPC'),('MPC'),'','','');
                FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));
                ENDSEC;
                DATA;
                ENDSEC;
                END-ISO-10303-21;
                """);

        try
        {
            var reader =
                CreateReader();

            var result =
                reader.Read(filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.Equal(
                0,
                result.Value!.TotalEntityCount);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Constructor_WithNullInventoryValidator_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StepDataEntityReader(
                inventoryValidator: null!));
    }

    private static StepDataEntityReader
        CreateReader()
    {
        return new StepDataEntityReader(
            inventoryValidator:
                new StepDataInventoryValidator());
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