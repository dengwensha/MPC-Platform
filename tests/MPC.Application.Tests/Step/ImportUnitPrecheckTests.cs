using MPC.Application.Step;
using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Application.Tests.Step;

public sealed class ImportUnitPrecheckTests
{
    private static string Golden => File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"TestData/Step/Golden/T001_PRISM.step"));
    private static string Add(string s,string data) => s.Replace("ENDSEC;\nEND-ISO-10303-21;",data+"\nENDSEC;\nEND-ISO-10303-21;");
    internal static string Variant(string kind)
    {
        string s=Golden.Replace("\r\n","\n");
        string Context(string units)=>s.Replace("GLOBAL_UNIT_ASSIGNED_CONTEXT ( ( #147, #38, #5 ) )", "GLOBAL_UNIT_ASSIGNED_CONTEXT ( ( "+units+" ) )");
        return kind switch {
            "metre"=>s.Replace("#147 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( .MILLI., .METRE. ) );","#147 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( $, .METRE. ) );"),
            "centimetre"=>s.Replace("#147 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( .MILLI., .METRE. ) );","#147 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( .CENTI., .METRE. ) );"),
            "inch"=>Add(Context("#900,#38,#5"),"#900=(LENGTH_UNIT() NAMED_UNIT(*) CONVERSION_BASED_UNIT('INCH',#901));#901=LENGTH_MEASURE_WITH_UNIT(LENGTH_MEASURE(25.4),#147);"),
            "degree"=>Add(Context("#147,#900,#5"),"#900=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) CONVERSION_BASED_UNIT('DEGREE',#901));#901=PLANE_ANGLE_MEASURE_WITH_UNIT(PLANE_ANGLE_MEASURE(0.0174532925199433),#38);"),
            "prefixed-radian"=>s.Replace("SI_UNIT ( $, .RADIAN. )", "SI_UNIT ( .MILLI., .RADIAN. )"),
            "2d"=>s.Replace("GEOMETRIC_REPRESENTATION_CONTEXT ( 3 )", "GEOMETRIC_REPRESENTATION_CONTEXT ( 2 )"),
            "transformed"=>Add(s,"#900=(REPRESENTATION_RELATIONSHIP('','',#47,#901) REPRESENTATION_RELATIONSHIP_WITH_TRANSFORMATION(#175) SHAPE_REPRESENTATION_RELATIONSHIP());#901=SHAPE_REPRESENTATION('',(#175),#61);"),
            "bridge-reverse"=>Variant("bridge").Replace("RELATIONSHIP('',$,#900,#47)","RELATIONSHIP('',$,#47,#900)"),
            "missing-angle"=>Context("#147,#5"),
            "duplicate-angle"=>Context("#147,#38,#38,#5"),
            "missing-length"=>Context("#38,#5"),
            "duplicate-length"=>Context("#147,#147,#38,#5"),
            "unknown-unit"=>Add(Context("#147,#38,#5,#900"),"#900=UNKNOWN_UNIT();"),
            "two-reps"=>Add(s,"#900=ADVANCED_BREP_SHAPE_REPRESENTATION('',(#63),#61);"),
            "duplicate-item"=>s.Replace("( #63, #175 )", "( #63,#63,#175 )"),
            "missing-sdr"=>s.Replace("#27 = SHAPE_DEFINITION_REPRESENTATION ( #4, #47 ) ;", ""),
            "unknown-incoming"=>Add(s,"#900=UNREVIEWED_REFERENCE(#63);"),
            "mapped"=>Add(s,"#900=MAPPED_ITEM(#63,#175);"),
            "overriding-style"=>Add(s,"#900=OVER_RIDING_STYLED_ITEM('',(#135),#63,#48);"),
            "bridge"=>Add(s.Replace("#27 = SHAPE_DEFINITION_REPRESENTATION ( #4, #47 ) ;","#27=SHAPE_DEFINITION_REPRESENTATION(#4,#900);"),"#900=SHAPE_REPRESENTATION('',(#175),#89);#901=SHAPE_REPRESENTATION_RELATIONSHIP('',$,#900,#47);"),
            "dual-sdr"=>Add(s,"#900=SHAPE_REPRESENTATION('',(#175),#89);#901=SHAPE_REPRESENTATION_RELATIONSHIP('',$,#900,#47);#902=SHAPE_DEFINITION_REPRESENTATION(#4,#900);"),
            "bridge-different-unit"=>Variant("bridge").Replace("#16 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( .MILLI., .METRE. ) );","#16 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( $, .METRE. ) );"),
            "bridge-chain"=>Add(Variant("bridge"),"#902=SHAPE_REPRESENTATION('',(#175),#61);#903=SHAPE_REPRESENTATION_RELATIONSHIP('',$,#900,#902);"),
            "assembly"=>Add(s,"#900=NEXT_ASSEMBLY_USAGE_OCCURRENCE('','',$,#162,#162,$);"),
            _=>throw new ArgumentException(kind)
        };
    }
    [Theory]
    [InlineData("metre")][InlineData("centimetre")][InlineData("inch")][InlineData("degree")]
    [InlineData("missing-angle")][InlineData("duplicate-angle")][InlineData("missing-length")][InlineData("duplicate-length")]
    [InlineData("unknown-unit")][InlineData("two-reps")][InlineData("duplicate-item")][InlineData("missing-sdr")]
    [InlineData("unknown-incoming")][InlineData("mapped")][InlineData("overriding-style")][InlineData("dual-sdr")]
    [InlineData("bridge-different-unit")][InlineData("bridge-chain")][InlineData("assembly")]
    [InlineData("prefixed-radian")][InlineData("2d")][InlineData("transformed")]
    public void RejectsBeforePublishingAndUsesExistingErrorEnvelope(string kind)=>WithFile(Variant(kind),path=>{
        var result=new StepImportApplicationService().Execute(new StepImportRequest(path));
        Assert.Equal(StepImportStatus.Failed,result.Status);Assert.Null(result.Summary);
        var d=Assert.Single(result.Diagnostics.Where(d=>d.Code==StepImportApplicationService.SemanticFailureCode));
        Assert.StartsWith("STEP unit/context precheck: ",d.Message);Assert.Contains("#63",d.Message);
        var ex=Assert.Throws<InvalidDataException>(()=>ManufacturingRegionImportService.TryCreate(path,"#30",out _));
        Assert.StartsWith("STEP unit/context precheck: ",ex.Message);
    });
    [Theory][InlineData(false)][InlineData(true)]
    public void PreservesStyledDirectOrEqualUnitBridge(bool bridge)=>WithFile(bridge?Variant("bridge"):Golden,path=>{
        Assert.Equal(StepImportStatus.Succeeded,new StepImportApplicationService().Execute(new StepImportRequest(path)).Status);
        Assert.True(ManufacturingRegionImportService.TryCreate(path,"#30",out var region));Assert.NotNull(region);
    });
    [Fact]
    public void EqualUnitBridgeSupportsReversedEndpoints()=>WithFile(Variant("bridge-reverse"),path=>{
        Assert.Equal(StepImportStatus.Succeeded,new StepImportApplicationService().Execute(new StepImportRequest(path)).Status);
        Assert.True(ManufacturingRegionImportService.TryCreate(path,"#30",out _));
    });
    [Fact]
    public void PrecheckUsesRetainedInventoryAfterSourceChanges()=>WithFile(Golden,path=>{
        var inventory=new StepDataEntityReader(new StepDataInventoryValidator()).Read(path).Value!;
        File.WriteAllText(path,Variant("metre"));
        ImportUnitPrecheck.EnsureCompatible(inventory,"#63");
        Assert.Throws<InvalidDataException>(()=>ManufacturingRegionImportService.TryCreate(path,"#30",out _));
    });
    internal static void WithFile(string text,Action<string> action){string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");try{File.WriteAllText(path,text);action(path);}finally{File.Delete(path);}}
}

public sealed class AnalyticConeUnitTests
{
    [Fact]
    public void IndependentClosedFrustumHasValidRadianGeometryAndImports()
    {
        string path=Path.Combine(AppContext.BaseDirectory,"TestData/Step/RU-QuarterCone.step");
        Assert.Equal(StepImportStatus.Succeeded,new StepImportApplicationService().Execute(new StepImportRequest(path)).Status);
        Assert.True(ManufacturingRegionImportService.TryCreate(path,"#801",out var region));Assert.NotNull(region);
        var inventory=new StepDataEntityReader(new StepDataInventoryValidator()).Read(path).Value!;
        var reader=new MPC.Connectors.Step.Semantics.StepSemanticEntityReader(new MPC.Connectors.Step.Parsing.StepParameterParser(),StepSemanticHandlerCatalog.Create());
        var entities=inventory.Entities.Select(reader.Read).OfType<MPC.Connectors.Step.Semantics.StepSemanticEntity>().ToArray();
        var operation=MPC.Mapping.Step.StepCanonicalMappingOperation.Create(new MPC.Connectors.Step.Semantics.StepSemanticEntityResolver(entities));
        var mapped=operation.Map(entities.OfType<MPC.Connectors.Step.Semantics.StepManifoldSolidBrepEntity>().Single());
        Assert.True(mapped.IsSuccessful);Assert.Equal(5,mapped.Value!.OuterShell.Faces.Count);
        Assert.True(mapped.Value.OuterShell.IsClosed);
        var edges=mapped.Value.OuterShell.Faces.SelectMany(f=>f.OuterLoop.EdgeUses).Select(u=>u.Edge).Distinct().ToArray();
        Assert.Equal(9,edges.Length);Assert.Equal(6,edges.SelectMany(e=>new[]{e.StartVertex,e.EndVertex}).Distinct().Count());
        Assert.True(operation.FaceIdentities.TryGetFace("#805",out var face));
        var cone=Assert.IsType<MPC.Geometry.Primitives.Cone3D>(face!.Surface);
        Assert.Equal(Math.PI/4,cone.SemiAngle,12);
        Assert.True(ConeLocalNormalQuery.TryEvaluate(cone,new MPC.Geometry.Primitives.Point3D(1.5/Math.Sqrt(2),1.5/Math.Sqrt(2),1.5),out var normal));
        Assert.Equal(.5,normal.X,10);Assert.Equal(.5,normal.Y,10);Assert.Equal(-Math.Sqrt(.5),normal.Z,10);
    }
    [Fact]
    public void PhysicallyEquivalentDegreeConeIsRejectedWithoutConversion()
    {
        string text=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"TestData/Step/RU-QuarterCone.step"))
            .Replace("1.,0.7853981633974483)","1.,45.)")
            .Replace("#1004=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) SI_UNIT($,.RADIAN.));",
                "#1004=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) CONVERSION_BASED_UNIT('DEGREE',#1012));#1012=PLANE_ANGLE_MEASURE_WITH_UNIT(PLANE_ANGLE_MEASURE(0.017453292519943295),#1013);#1013=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) SI_UNIT($,.RADIAN.));");
        ImportUnitPrecheckTests.WithFile(text,path=>{
            var result=new StepImportApplicationService().Execute(new StepImportRequest(path));Assert.Equal(StepImportStatus.Failed,result.Status);
            Assert.Contains(result.Diagnostics,d=>d.Code==StepImportApplicationService.SemanticFailureCode&&d.Message.StartsWith("STEP unit/context precheck: ",StringComparison.Ordinal));
            Assert.Throws<InvalidDataException>(()=>ManufacturingRegionImportService.TryCreate(path,"#801",out _));
        });
    }
}
