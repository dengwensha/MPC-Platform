using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MPC.Application.Manufacturing;
using MPC.Application.Step;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Resolution;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Geometry.Primitives;
using MPC.Mapping.Step;

if (args.Length != 2) throw new ArgumentException("Expected repository root and evidence JSON path.");
string root=Path.GetFullPath(args[0]);
string golden=Path.Combine(root,"tests/MPC.Tests/TestData/Step/Golden/T001_PRISM.step");
string source=File.ReadAllText(golden).Replace("\r\n","\n",StringComparison.Ordinal);
string Hash(byte[] b)=>Convert.ToHexString(SHA256.HashData(b));
string ReplaceOnce(string s,string old,string replacement)
{
    if (s.Split(old).Length!=2) throw new InvalidOperationException("Fixture anchor is not unique: "+old);
    return s.Replace(old,replacement,StringComparison.Ordinal);
}
string InContext(string s,string units)=>ReplaceOnce(s,"GLOBAL_UNIT_ASSIGNED_CONTEXT ( ( #147, #38, #5 ) )", "GLOBAL_UNIT_ASSIGNED_CONTEXT ( ( "+units+" ) )");
string Insert(string s,string records)=>ReplaceOnce(s,"ENDSEC;\nEND-ISO-10303-21;",records+"\nENDSEC;\nEND-ISO-10303-21;");
const string inch="#900=(LENGTH_UNIT() NAMED_UNIT(#902) CONVERSION_BASED_UNIT('INCH',#901));\n#901=LENGTH_MEASURE_WITH_UNIT(LENGTH_MEASURE(25.4),#147);\n#902=DIMENSIONAL_EXPONENTS(1.,0.,0.,0.,0.,0.,0.);";
const string degree="#903=(NAMED_UNIT(#905) PLANE_ANGLE_UNIT() CONVERSION_BASED_UNIT('DEGREE',#904));\n#904=PLANE_ANGLE_MEASURE_WITH_UNIT(PLANE_ANGLE_MEASURE(0.017453292519943295),#38);\n#905=DIMENSIONAL_EXPONENTS(0.,0.,0.,0.,0.,0.,0.);";
var cases=new List<(string Id,string Text,string DeclaredLength,string DeclaredAngle,bool Cone)>{
    ("C01-mm-radian",source,"Millimetre","Radian",false),
    ("C02-inch-radian",Insert(InContext(source,"#900, #38, #5"),inch),"Inch","Radian",false),
    ("C03-metre-radian",ReplaceOnce(source,"#147 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( .MILLI., .METRE. ) );","#147 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( $, .METRE. ) );"),"Metre","Radian",false)
};
string cone=ReplaceOnce(source,"#106 = PLANE ( 'NONE',  #102 ) ;","#106=CONICAL_SURFACE('RU angular negative control',#102,1.,0.5);");
cases.Add(("C04-cone-radian",cone,"Millimetre","Radian",true));
cases.Add(("C05-cone-degree",Insert(InContext(cone,"#147, #903, #5"),degree),"Millimetre","Degree",true));
cases.Add(("C06-cone-missing-angle",InContext(cone,"#147, #5"),"Millimetre","Missing",true));
cases.Add(("C07-cone-conflicting-angle",Insert(InContext(cone,"#147, #38, #903, #5"),degree),"Millimetre","RadianAndDegree",true));
var rows=new List<object>();
string temp=Path.Combine(Path.GetTempPath(),"mpc-ru-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
try {
 foreach(var c in cases){
  string path=Path.Combine(temp,c.Id+".step");File.WriteAllText(path,c.Text,new UTF8Encoding(false));
  string hash=Hash(File.ReadAllBytes(path));
  var inv=new StepDataEntityReader(new StepDataInventoryValidator()).Read(path);
  if(inv.Value is null)throw new InvalidOperationException("Invalid audit fixture inventory: "+c.Id);
  var unit=new StepUnitResolver().Resolve(inv.Value);
  var imported=new StepImportApplicationService().Execute(new StepImportRequest(path));
  string? regionException=null;ManufacturingRegion? region=null;bool regionOk=false;
  try{regionOk=ManufacturingRegionImportService.TryCreate(path,"#30",out region);}catch(Exception ex){regionException=ex.GetType().FullName;}
  string? gateException=null;ManufacturingRegion? published=null;RegionSourceGateCode? gate=null;
  try{gate=ValidatedRegionImportService.Execute(path,"#30",hash,"#30",RegionEvaluationRequirement.SourceLinkOnly,out published);}catch(Exception ex){gateException=ex.GetType().FullName;}
  // Audit-only reflection uses the exact existing production handler catalogue,
  // without granting friend access or minting Application inventory evidence.
  var catalog=typeof(StepImportApplicationService).Assembly.GetType("MPC.Application.Step.StepSemanticHandlerCatalog")!;
  var handlers=(IReadOnlyList<IStepSemanticEntityHandler>)catalog.GetMethod("Create")!.Invoke(null,[null])!;
  var reader=new StepSemanticEntityReader(new StepParameterParser(),handlers);
  var entities=inv.Value.Entities.Select(reader.Read).OfType<StepSemanticEntity>().ToArray();
  var op=StepCanonicalMappingOperation.Create(new StepSemanticEntityResolver(entities));
  var mapped=op.Map(entities.OfType<StepManifoldSolidBrepEntity>().Single());
  double? angle=null;
  if(mapped.IsSuccessful&&op.FaceIdentities.TryGetFace("#8",out var f)&&f?.Surface is Cone3D surface)angle=surface.SemiAngle;
  var row=new{Id=c.Id,InputSha256=hash,InputUnchanged=hash==Hash(File.ReadAllBytes(path)),c.DeclaredLength,c.DeclaredAngle,c.Cone,
   InventoryStatus=inv.Status.ToString(),InventoryDiagnostics=inv.Diagnostics.Select(d=>new{d.Code,Severity=d.Severity.ToString()}).ToArray(),
   LegacyResolverLength=unit.Value?.SourceLengthUnit.ToString(),LegacyResolverAngle=unit.Value?.SourceAngleUnit.ToString(),
   LegacyResolverDiagnostics=unit.Diagnostics.Select(d=>new{d.Code,Severity=d.Severity.ToString()}).ToArray(),
   LegacyStatus=imported.Status.ToString(),LegacyDiagnostics=imported.Diagnostics.Select(d=>d.Code).ToArray(),
   RegionSucceeded=regionOk,RegionException=regionException,RegionOrigin=region is null?null:new{region.Origin.X,region.Origin.Y,region.Origin.Z},
   SourceGate=gate?.ToString(),GateException=gateException,RegionPublished=published is not null,
   MappedRootSucceeded=mapped.IsSuccessful,MappedConeSemiAngle=angle,
   ExpectedRadiansForDeclaredHalfDegree=c.DeclaredAngle=="Degree"?0.5*Math.PI/180:(double?)null};
  rows.Add(row);Console.WriteLine("RU-OBSERVATION "+JsonSerializer.Serialize(row));
 }
 var report=new{Purpose="Baseline observation only; successful execution is not import safety acceptance",Runtime=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
  ApplicationAssemblySha256=Hash(File.ReadAllBytes(typeof(StepImportApplicationService).Assembly.Location)),ReaderAssemblySha256=Hash(File.ReadAllBytes(typeof(StepDataEntityReader).Assembly.Location)),
  GoldenSha256=Hash(File.ReadAllBytes(golden)),NormalizedGoldenSha256=Hash(Encoding.UTF8.GetBytes(source)),Rows=rows};
 Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
 File.WriteAllText(args[1],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
}finally{Directory.Delete(temp,true);}
