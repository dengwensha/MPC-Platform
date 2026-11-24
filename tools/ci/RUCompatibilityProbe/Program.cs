using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections;
if(args.Length!=3)throw new ArgumentException("Expected Application bin directory, STEP input directory, output directory.");
string app=Path.GetFullPath(args[0]);
AssemblyLoadContext.Default.Resolving+=(_,n)=>Assembly.LoadFrom(Path.Combine(app,n.Name+".dll"));
var assembly=Assembly.LoadFrom(Path.Combine(app,"MPC.Application.dll"));
var connector=Assembly.LoadFrom(Path.Combine(app,"MPC.Connectors.Step.dll"));
object? P(object o,string n)=>o.GetType().GetProperty(n)!.GetValue(o);
string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));
object Invoke(MethodInfo method,object? target,object?[] values)=>method.Invoke(target,values)!;
string? Failure(Exception e)=>(e is TargetInvocationException t?t.InnerException:e)?.GetType().FullName;
string? SafeReason(Exception e){var x=e is TargetInvocationException t?t.InnerException:e;return x?.Message.StartsWith("STEP unit/context precheck: ",StringComparison.Ordinal)==true?x.Message:null;}
Directory.CreateDirectory(args[2]);
foreach(string path in Directory.GetFiles(args[1]).Where(p=>Path.GetExtension(p).Equals(".step",StringComparison.OrdinalIgnoreCase)||Path.GetExtension(p).Equals(".stp",StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal)){
 string before=Hash(File.ReadAllBytes(path));
 var rt=connector.GetType("MPC.Connectors.Step.Readers.StepDataEntityReader")!;
 var validator=Activator.CreateInstance(connector.GetType("MPC.Connectors.Step.Validation.StepDataInventoryValidator")!);
 var read=Invoke(rt.GetMethod("Read")!,Activator.CreateInstance(rt,validator),[path]);
 var inv=P(read,"Value");var records=inv is null?[]:((IEnumerable)P(inv,"Entities")!).Cast<object>().ToArray();
 var parser=Activator.CreateInstance(connector.GetType("MPC.Connectors.Step.Parsing.StepParameterParser")!)!;
 string? FaceSurface(object r){
  try{string raw=(string)P(r,"RawStatement")!;string body=raw[(raw.IndexOf('=')+1)..].Trim().TrimEnd(';');
   var parsed=(IEnumerable)Invoke(parser.GetType().GetMethod("Parse")!,parser,[body]);var typed=parsed.Cast<object>().Single();
   var ps=((IEnumerable)P(typed,"Parameters")!).Cast<object>().ToArray();return P(ps[2],"EntityId")?.ToString();}
  catch(Exception e)when(e is TargetInvocationException or InvalidOperationException or IndexOutOfRangeException or NullReferenceException){return null;}
 }
 var planes=records.Where(r=>(string)P(r,"SourceEntityType")! =="PLANE").Select(r=>P(r,"SourceEntityId")!.ToString()).ToHashSet();
 var faces=records.Where(r=>(string)P(r,"SourceEntityType")! =="ADVANCED_FACE").OrderBy(r=>P(r,"SourceEntityId")!.ToString(),StringComparer.Ordinal).ToArray();
 string face=(faces.FirstOrDefault(r=>planes.Contains(FaceSurface(r))) is {} selected?P(selected,"SourceEntityId")?.ToString():faces.FirstOrDefault() is {} any?P(any,"SourceEntityId")?.ToString():null)??"#999999";
 object legacy;
 try{var type=assembly.GetType("MPC.Application.Step.StepImportApplicationService")!;var request=Activator.CreateInstance(assembly.GetType("MPC.Application.Step.StepImportRequest")!,path);var result=Invoke(type.GetMethod("Execute")!,Activator.CreateInstance(type),[request]);
  var ds=((IEnumerable)P(result,"Diagnostics")!).Cast<object>().Select(d=>new{Code=P(d,"Code"),Severity=P(d,"Severity")!.ToString(),Reason=P(d,"Message")?.ToString() is {} m&&m.StartsWith("STEP unit/context precheck: ",StringComparison.Ordinal)?m:null}).ToArray();
  legacy=new{Available=true,Status=P(result,"Status")!.ToString(),SummaryPresent=P(result,"Summary") is not null,Diagnostics=ds};
 }catch(Exception e){legacy=new{Available=false,FailureType=Failure(e),Reason=SafeReason(e)};}
 object region;
 try{var values=new object?[]{path,face,null};bool ok=(bool)Invoke(assembly.GetType("MPC.Application.Manufacturing.ManufacturingRegionImportService")!.GetMethod("TryCreate")!,null,values);
  var origin=values[2] is null?null:P(values[2]!,"Origin");
  region=new{Available=true,Succeeded=ok,Published=values[2] is not null,OriginSha256=origin is null?null:Hash(JsonSerializer.SerializeToUtf8Bytes(origin,origin.GetType()))};
 }catch(Exception e){region=new{Available=false,FailureType=Failure(e),Reason=SafeReason(e)};}
 var row=new{InputName=Path.GetFileName(path),InputSha256=before,InputUnchanged=before==Hash(File.ReadAllBytes(path)),SelectedFaceId=face,
 ApplicationAssemblySha256=Hash(File.ReadAllBytes(assembly.Location)),ReaderAssemblySha256=Hash(File.ReadAllBytes(connector.Location)),
 InventoryStatus=P(read,"Status")!.ToString(),RootIds=records.Where(r=>(string)P(r,"SourceEntityType")! =="MANIFOLD_SOLID_BREP").Select(r=>P(r,"SourceEntityId")).ToArray(),
 BindingAndUnitRecordIds=records.Where(r=>((string)P(r,"SourceEntityType")!).Contains("REPRESENTATION",StringComparison.Ordinal)||((IEnumerable)P(r,"ComponentTypes")!).Cast<string>().Any(t=>t is "GEOMETRIC_REPRESENTATION_CONTEXT" or "LENGTH_UNIT" or "PLANE_ANGLE_UNIT")).Select(r=>P(r,"SourceEntityId")).ToArray(),Legacy=legacy,Region=region};
 File.WriteAllText(Path.Combine(args[2],Path.GetFileName(path)+".json"),JsonSerializer.Serialize(row,new JsonSerializerOptions{WriteIndented=true}));
 Console.WriteLine("RU-COMPATIBILITY "+JsonSerializer.Serialize(row));
}
