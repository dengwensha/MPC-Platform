using System.Security.Cryptography;
using System.Text.Json;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;
if(args.Length!=2)throw new ArgumentException("Expected private input and neutral AST output directory.");
var parser=new StepParameterParser();
object Parameter(StepParameter p)=>p switch{
 StepEntityReferenceParameter r=>new{Kind="ref",Id=r.EntityId},
 StepAggregateParameter a=>new{Kind="aggregate",Items=a.Items.Select(Parameter).ToArray()},
 StepTypedParameter t=>new{Kind="typed",Name=t.TypeName,Items=t.Parameters.Select(Parameter).ToArray()},
 StepEnumerationParameter e=>new{Kind="enum",Value=e.Name},
 StepIntegerParameter i=>new{Kind="integer",Value=i.RawText},
 StepRealParameter n=>new{Kind="number",Value=n.RawText},
 StepStringParameter=>new{Kind="string"},StepNullParameter=>new{Kind="null"},StepOmittedParameter=>new{Kind="omitted"},
 _=>new{Kind="unknown"}};
IReadOnlyList<StepTypedParameter> Parts(string raw){
 int equals=raw.IndexOf('=');if(equals<0)throw new FormatException("Missing assignment.");
 string body=raw[(equals+1)..].Trim().TrimEnd(';').Trim();
 if(!body.StartsWith('(')){var values=parser.Parse(body);return values.Count==1&&values[0] is StepTypedParameter t?[t]:throw new FormatException("Not a typed declaration.");}
 if(!body.EndsWith(')'))throw new FormatException("Incomplete complex declaration.");
 body=body[1..^1];var parts=new List<StepTypedParameter>();int start=0,depth=0;bool quote=false,binary=false;
 for(int i=0;i<body.Length;i++){
  char c=body[i];
  if(!binary&&c=='\''){if(quote&&i+1<body.Length&&body[i+1]=='\''){i++;continue;}quote=!quote;}
  else if(!quote&&c=='"')binary=!binary;
  if(quote||binary)continue;
  if(c=='(')depth++;
  if(c==')'){
   depth--;if(depth<0)throw new FormatException("Unbalanced component.");
   if(depth==0){var values=parser.Parse(body[start..(i+1)]);if(values.Count!=1||values[0] is not StepTypedParameter t)throw new FormatException("Invalid component.");parts.Add(t);start=i+1;}
  }
 }
 if(depth!=0||quote||binary||body[start..].Trim().Length!=0||parts.Count==0)throw new FormatException("Incomplete component list.");
 return parts;
}
Directory.CreateDirectory(args[1]);
foreach(string path in Directory.GetFiles(args[0]).Where(p=>Path.GetExtension(p).Equals(".step",StringComparison.OrdinalIgnoreCase)||Path.GetExtension(p).Equals(".stp",StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal)){
 string before=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
 var result=new StepDataEntityReader(new StepDataInventoryValidator()).Read(path);
 var records=new List<object>();
 foreach(var r in result.Value?.Entities??[]){
  object[] components=[];string? failure=null;
  try{components=Parts(r.RawStatement).Select(t=>(object)new{Name=t.TypeName,Parameters=t.Parameters.Select(Parameter).ToArray()}).ToArray();}
  catch(FormatException ex){failure=ex.GetType().FullName;}
  records.Add(new{Id=r.SourceEntityId,Type=r.SourceEntityType,Status=r.ReadStatus.ToString(),References=r.ReferencedEntityIds,Components=components,ParseFailure=failure});
 }
 var row=new{InputName=Path.GetFileName(path),InputSha256=before,InputUnchanged=before==Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),ReaderAssemblySha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(StepDataEntityReader).Assembly.Location))),Status=result.Status.ToString(),Diagnostics=result.Diagnostics.Select(d=>new{d.Code,Severity=d.Severity.ToString(),d.SourceEntityId}).ToArray(),Records=records};
 // PRIVATE intermediate only: this AST contains coordinate values. Never commit it.
 File.WriteAllText(Path.Combine(args[1],Path.GetFileName(path)+".json"),JsonSerializer.Serialize(row));Console.WriteLine(row.InputName+": "+records.Count);
}
