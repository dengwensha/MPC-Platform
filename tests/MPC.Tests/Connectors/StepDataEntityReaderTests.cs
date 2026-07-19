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
                #10 = FACE_BOUND('',#20,.T.);
                #20 = PLANE('',#44);
                #42 = ADVANCED_FACE('',(#10),#20,.T.);
                #43 = CYLINDRICAL_SURFACE('',#44,0.025);
                #44 = AXIS2_PLACEMENT_3D('',#45,$,$);
                #45 = CARTESIAN_POINT('',(0.0,0.0,0.0));
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

            Assert.Equal(
                8,
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
            File.Delete(
                filePath);
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
                #101 = AXIS2_PLACEMENT_3D('',#102,$,$);
                #102 = CARTESIAN_POINT('',(0.0,0.0,0.0));
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

            var entity =
                Assert.Single(
                    result.Value!.Entities,
                    item =>
                        item.SourceEntityId ==
                        "#100");

            Assert.Equal(
                "CYLINDRICAL_SURFACE",
                entity.SourceEntityType);

            Assert.Contains(
                "0.025",
                entity.RawStatement);

            Assert.EndsWith(
                ";",
                entity.RawStatement);

            Assert.Equal(
                new[]
                {
                    "#101"
                },
                entity.ReferencedEntityIds);
        }
        finally
        {
            File.Delete(
                filePath);
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
                reader.Read(
                    filePath);

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
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Read_WithEntityReferences_PreservesSourceOrderAndDuplicates()
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
                #5 = CARTESIAN_POINT('',(0.0,0.0,0.0));
                #7 = CARTESIAN_POINT('',(1.0,0.0,0.0));
                #8 = CARTESIAN_POINT('',(0.0,1.0,0.0));
                #42 = TEST_ENTITY(#5,(#7,#8),#5);
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

            var entity =
                Assert.Single(
                    result.Value!.Entities,
                    item =>
                        item.SourceEntityId ==
                        "#42");

            Assert.Equal(
                new[]
                {
                    "#5",
                    "#7",
                    "#8",
                    "#5"
                },
                entity.ReferencedEntityIds);

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
    public void Read_WithReferenceLikeTextInsideString_DoesNotExtractStringContent()
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
                #5 = CARTESIAN_POINT('',(0.0,0.0,0.0));
                #42 = TEST_ENTITY('Text containing #999',#5);
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

            var entity =
                Assert.Single(
                    result.Value!.Entities,
                    item =>
                        item.SourceEntityId ==
                        "#42");

            Assert.Equal(
                new[]
                {
                    "#5"
                },
                entity.ReferencedEntityIds);

            Assert.DoesNotContain(
                "#999",
                entity.ReferencedEntityIds);

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
    public void Read_WithMissingEntityReference_PreservesEntityAndReportsDiagnostic()
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
                #42 = TEST_ENTITY(#999);
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
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            var entity =
                Assert.Single(
                    result.Value!.Entities);

            Assert.Equal(
                "#42",
                entity.SourceEntityId);

            Assert.Equal(
                new[]
                {
                    "#999"
                },
                entity.ReferencedEntityIds);

            var diagnostic =
                Assert.Single(
                    result.Diagnostics,
                    item =>
                        item.Code ==
                        "STEP_MISSING_ENTITY_REFERENCE");

            Assert.Equal(
                CadDiagnosticSeverity.Error,
                diagnostic.Severity);

            Assert.Equal(
                "#42",
                diagnostic.SourceEntityId);

            Assert.Contains(
                "#999",
                diagnostic.Message);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Read_WithRepeatedMissingReference_ReportsSingleDiagnostic()
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
                #42 = TEST_ENTITY(#999,(#999,#999));
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
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            var entity =
                Assert.Single(
                    result.Value!.Entities);

            Assert.Equal(
                new[]
                {
                    "#999",
                    "#999",
                    "#999"
                },
                entity.ReferencedEntityIds);

            var diagnostics =
                result.Diagnostics
                    .Where(
                        item =>
                            item.Code ==
                            "STEP_MISSING_ENTITY_REFERENCE")
                    .ToArray();

            var diagnostic =
                Assert.Single(
                    diagnostics);

            Assert.Equal(
                "#42",
                diagnostic.SourceEntityId);

            Assert.Contains(
                "#999",
                diagnostic.Message);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Read_WithMultipleMissingReferences_ReportsOneDiagnosticPerTarget()
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
                #42 = TEST_ENTITY(#100,#75,#100);
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
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            var diagnostics =
                result.Diagnostics
                    .Where(
                        item =>
                            item.Code ==
                            "STEP_MISSING_ENTITY_REFERENCE")
                    .ToArray();

            Assert.Equal(
                2,
                diagnostics.Length);

            Assert.Contains(
                diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "#75",
                        StringComparison.Ordinal));

            Assert.Contains(
                diagnostics,
                diagnostic =>
                    diagnostic.Message.Contains(
                        "#100",
                        StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Read_WithEquivalentNumericReference_RecognizesDeclaredTarget()
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
                #025 = CARTESIAN_POINT('',(0.0,0.0,0.0));
                #42 = TEST_ENTITY(#25);
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

            var entity =
                Assert.Single(
                    result.Value!.Entities,
                    item =>
                        item.SourceEntityId ==
                        "#42");

            Assert.Equal(
                new[]
                {
                    "#25"
                },
                entity.ReferencedEntityIds);

            Assert.DoesNotContain(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_MISSING_ENTITY_REFERENCE");
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Read_WithDuplicateTargetIds_DoesNotReportMissingReference()
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
                #42 = TEST_ENTITY(#25);
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
                CadConnectorResultStatus
                    .SucceededWithDiagnostics,
                result.Status);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_DUPLICATE_SOURCE_ENTITY_ID");

            Assert.DoesNotContain(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    "STEP_MISSING_ENTITY_REFERENCE");
        }
        finally
        {
            File.Delete(
                filePath);
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
                #1 = PRODUCT('PART','PART','',());
                INVALID DATA RECORD;
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
            File.Delete(
                filePath);
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
                reader.Read(
                    filePath);

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
            File.Delete(
                filePath);
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
                reader.Read(
                    filePath);

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
            File.Delete(
                filePath);
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
                reader.Read(
                    filePath);

            Assert.Equal(
                CadConnectorResultStatus.Failed,
                result.Status);

            Assert.Equal(
                "STEP_DATA_SECTION_NOT_FOUND",
                result.Diagnostics[0].Code);
        }
        finally
        {
            File.Delete(
                filePath);
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
                reader.Read(
                    filePath);

            Assert.Equal(
                CadConnectorResultStatus.Succeeded,
                result.Status);

            Assert.Equal(
                0,
                result.Value!.TotalEntityCount);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Constructor_WithNullInventoryValidator_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new StepDataEntityReader(
                    inventoryValidator:
                        null!));
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