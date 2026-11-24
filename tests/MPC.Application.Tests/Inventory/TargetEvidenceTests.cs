using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using MPC.Application.Inventory;
using MPC.Application.Step;
using MPC.Application.Tests.Step;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;
using Gate = MPC.Application.Inventory.TargetDecision.TargetEvidenceGate;

namespace MPC.Application.Tests.Inventory;

public sealed class TargetEvidenceTests
{
    private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    private static string Golden => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData/Step/Golden/T001_PRISM.step")).Replace("\r\n", "\n");
    private static InventoryEvidence.ImportSession Capture(string source, Action<string>? checkpoint = null)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try
        {
            File.WriteAllText(path, source);
            var capture = InventoryEvidence.ImportSession.CaptureFromFile(path, checkpoint is null ? null : () => checkpoint(path));
            Assert.Null(capture.Failure);
            return Assert.IsType<InventoryEvidence.ImportSession>(capture.Session);
        }
        finally { File.Delete(path); }
    }
    private static string Header(string data) => "ISO-10303-21;HEADER;FILE_DESCRIPTION(('test'),'1');FILE_NAME('a','',(''),(''),'','','');FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));ENDSEC;DATA;" + data + "ENDSEC;END-ISO-10303-21;";
    private static string Entity(string source, string id, string body) => Regex.Replace(source,
        Regex.Escape(id) + @"\s*=.*?;", id + "=" + body + ";", RegexOptions.Singleline | RegexOptions.CultureInvariant);
    private static string Add(string source, string record) => source.Insert(source.LastIndexOf("ENDSEC;", StringComparison.Ordinal), record);

    [Theory]
    [InlineData("T001_PRISM")][InlineData("T002_SPHERE")][InlineData("T003_CYLINDER")]
    [InlineData("T101_BASIC_POCKET")][InlineData("T103_MULTIPLE_ISLANDS")]
    public void FiveGoldensIssueOnlyOwnedTargetBinding(string name)
    {
        var session = Capture(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData/Step/Golden/" + name + ".step")));
        var observation = session.ReadTarget(session.Evidence)!;
        Assert.Equal(TargetStatus.TargetBoundOnly, Gate.Evaluate(session.Evidence, session).Status);
        Assert.Empty(observation.Reasons); Assert.True(observation.GuardCalled);
        Assert.Equal(TargetLayerStatus.Observed, observation.Binding);
        Assert.Equal(TargetLayerStatus.Observed, observation.Compatibility);
        Assert.Equal(session.RetainedEntityCount, observation.RecordCount);
        Assert.Equal(1, observation.ManifestRevision);
    }

    [Theory]
    [InlineData("metre")][InlineData("centimetre")][InlineData("inch")][InlineData("degree")]
    [InlineData("missing-angle")][InlineData("duplicate-angle")][InlineData("missing-length")][InlineData("duplicate-length")]
    [InlineData("unknown-unit")][InlineData("two-reps")][InlineData("duplicate-item")][InlineData("missing-sdr")]
    [InlineData("unknown-incoming")][InlineData("mapped")][InlineData("overriding-style")][InlineData("dual-sdr")]
    [InlineData("bridge-different-unit")][InlineData("bridge-chain")][InlineData("assembly")]
    [InlineData("prefixed-radian")][InlineData("2d")][InlineData("transformed")]
    public void A1_EveryExistingPrecheckNegativeCallsGuardAndCannotIssuePositive(string kind)
    {
        var session = Capture(ImportUnitPrecheckTests.Variant(kind));
        var observation = session.ReadTarget(session.Evidence)!;
        Assert.True(observation.GuardCalled);
        Assert.Equal(TargetLayerStatus.Blocked, observation.Compatibility);
        var rejection = Assert.Single(observation.Reasons.Where(r => r.Code == "CurrentImportIncompatible"));
        Assert.StartsWith(ImportUnitPrecheck.MessagePrefix, rejection.OriginalMessage);
        Assert.Equal(TargetStatus.Blocked, Gate.Evaluate(session.Evidence, session).Status);
    }

    [Theory][InlineData("metre")][InlineData("centimetre")]
    public void M1_StructuralUnitsStayIndependentOfImportCompatibility(string kind)
    {
        var session = Capture(ImportUnitPrecheckTests.Variant(kind));
        var o = session.ReadTarget(session.Evidence)!;
        Assert.Equal(TargetLayerStatus.Observed, o.Binding);
        Assert.Equal(TargetLayerStatus.Blocked, o.Compatibility);
        Assert.DoesNotContain(o.Reasons, r => r.Code == "ContextUnitsUnsupported");
    }

    [Theory][InlineData("$")][InlineData(".CENTI.")]
    public void A3_PresentationOnlyNonMmDoesNotImposeImportCompatibility(string prefix)
    {
        // #16 belongs to presentation context #89; target geometry uses #147/#61.
        string source = Entity(Golden, "#16", "(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT(" + prefix + ",.METRE.))");
        var session = Capture(source);
        Assert.Equal(TargetStatus.TargetBoundOnly, Gate.Evaluate(session.Evidence, session).Status);
    }

    [Fact]
    public void SameCapturedInventoryIsUsedDespiteReplacingSourceWithCompatibleInput()
    {
        var session = Capture(ImportUnitPrecheckTests.Variant("metre"), path => File.WriteAllText(path, Golden));
        Assert.Equal(TargetLayerStatus.Blocked, session.ReadTarget(session.Evidence)!.Compatibility);
        Assert.Equal(TargetStatus.Blocked, Gate.Evaluate(session.Evidence, session).Status);
    }

    [Fact]
    public void HistoricalConeMetadataGapIsBlockedAndSeparateCorrectedMetadataControlPasses()
    {
        string original=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"TestData/Step/RU-QuarterCone.step"));
        string corrected=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"TestData/Step/RU-QuarterCone-TargetMetadata.step"));
        var a=Capture(original);var b=Capture(corrected);
        Assert.Equal(TargetLayerStatus.Blocked,a.ReadTarget(a.Evidence)!.Binding);
        Assert.Contains(Gate.Evaluate(a.Evidence,a).Reasons,r=>r.Code=="ReferenceRoleMismatch"&&r.SourceId=="#1009"&&r.RelatedIds.Contains("#1011"));
        Assert.Equal(TargetStatus.TargetBoundOnly,Gate.Evaluate(b.Evidence,b).Status);
        // Every geometric record remains byte-identical; only PRODUCT and its new context differ.
        string Geometry(string text)=>string.Join("\n",text.Replace("\r\n","\n").Split('\n').Where(l=>l.StartsWith('#')&&!l.StartsWith("#1009=",StringComparison.Ordinal)&&!l.StartsWith("#1012=",StringComparison.Ordinal)));
        Assert.Equal(Geometry(original),Geometry(corrected));
    }

    [Theory]
    [InlineData("#7","CARTESIAN_POINT('',(1.,2.))")]
    [InlineData("#7","CARTESIAN_POINT('',(1.,2.,3.,4.))")]
    [InlineData("#21","DIRECTION('',(0.,0.,0.))")]
    [InlineData("#15","SURFACE_STYLE_USAGE(.UNKNOWN.,#46)")]
    [InlineData("#47","ADVANCED_BREP_SHAPE_REPRESENTATION('',(),#61)")]
    [InlineData("#47","ADVANCED_BREP_SHAPE_REPRESENTATION('',(#63,#63),#61)")]
    public void CardinalityEnumAndNumericBoundariesFailClosed(string id,string body)
    {
        var s=Capture(Entity(Golden,id,body));
        Assert.Equal(TargetStatus.Blocked,Gate.Evaluate(s.Evidence,s).Status);
        Assert.Contains(s.ReadTarget(s.Evidence)!.Reasons,r=>r.Code=="FieldConstraintFailed"&&r.SourceId==id);
    }

    [Fact]
    public void OptionalPlacementDirectionsAreExplicitNotOmittedOrGuessed()
    {
        string source=Entity(Golden,"#47","ADVANCED_BREP_SHAPE_REPRESENTATION('',(#63,#175,#990001),#61)");
        var a=Capture(Add(source,"#990001=AXIS2_PLACEMENT_3D('',#7,$,$);"));
        Assert.Equal(TargetStatus.TargetBoundOnly,Gate.Evaluate(a.Evidence,a).Status);
        var b=Capture(Add(source,"#990001=AXIS2_PLACEMENT_3D('',#7,*,*);"));
        Assert.Equal(TargetStatus.Blocked,Gate.Evaluate(b.Evidence,b).Status);
    }

    [Fact]
    public void MalformedPresentationUnitsAndForeignStyleTargetCannotHideAsMetadata()
    {
        string source=Entity(Golden,"#16","(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT(.KILO.,.METRE.))");
        source=Add(Entity(source,"#48","STYLED_ITEM('',(#135),#990001)"),"#990001=CARTESIAN_POINT('',(1.,2.,3.));");
        var s=Capture(source);var reasons=Gate.Evaluate(s.Evidence,s).Reasons;
        Assert.Contains(reasons,r=>r.Code=="ContextUnitsUnsupported");
        Assert.Contains(reasons,r=>r.Code=="PresentationInvalid");
        Assert.Contains(reasons,r=>r.Code=="UnownedGeometry");
    }

    [Fact]
    public void OwnershipAndMissingEvidenceCannotBorrowEqualHashes()
    {
        var a = Capture(Golden); var b = Capture(Golden);
        Assert.Equal(a.DocumentSha256, b.DocumentSha256);
        Assert.Null(a.ReadTarget(b.Evidence)); Assert.Null(a.ReadTargetData(b.Evidence));
        Assert.Null(TargetObservation.EvidenceBinding.Bind(b.Evidence, a));
        Assert.Equal(TargetStatus.Blocked, Gate.Evaluate(b.Evidence, a).Status);
        Assert.Equal("TargetOwnerMismatch", Assert.Single(Gate.Evaluate(b.Evidence, a).Reasons).Code);
        Assert.Equal(TargetStatus.NoEvidence, Gate.Evaluate(null, a).Status);
        Assert.Equal(TargetStatus.NoEvidence, Gate.Evaluate(a.Evidence, null).Status);
        var different = Capture(ImportUnitPrecheckTests.Variant("metre"));
        Assert.Null(a.ReadTarget(different.Evidence));
    }

    [Theory][InlineData(0,1,1,1,2,1)][InlineData(1,0,1,1,2,1)][InlineData(1,1,2,1,2,1)]
    [InlineData(1,1,1,0,2,1)][InlineData(1,1,1,1,1,1)][InlineData(1,1,1,1,2,2)]
    public void A2_OnlyExactApprovedRevisionTuplePasses(int manifest,int binding,int compatibility,int unit,int header,int envelope)
    {
        Assert.False(TargetRevisions.Approved(manifest,binding,compatibility,unit,header,envelope));
        Assert.True(TargetRevisions.Approved(1,1,1,1,2,1));
    }

    [Fact]
    public void BoundTypesArePrivateImmutableAndNoDetachedParameterPathExistsInApplication()
    {
        Type[] bound = [typeof(HeaderUnitObservation),typeof(EnvelopeObservation),typeof(TargetObservation)];
        Type[] protectedTypes = [..bound,typeof(HeaderUnitData),typeof(EnvelopeData),typeof(TargetData)];
        bool Protected(Type t) => protectedTypes.Contains(t) || (t.HasElementType && Protected(t.GetElementType()!))
            || (t.IsGenericType && t.GetGenericArguments().Any(Protected));
        bool ReturnsTarget(Type t) => t==typeof(TargetObservation)||(t.HasElementType&&ReturnsTarget(t.GetElementType()!))
            ||(t.IsGenericType&&t.GetGenericArguments().Any(ReturnsTarget));
        foreach (Type type in typeof(InventoryEvidence).Assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(Members))
            {
                Assert.DoesNotContain(method.GetParameters(), p => Protected(p.ParameterType));
                if(ReturnsTarget(method.ReturnType))
                {
                    Assert.Equal(typeof(TargetObservation),method.ReturnType);
                    Assert.True(type==typeof(TargetObservation.EvidenceBinding)||type==typeof(InventoryEvidence.ImportSession));
                }
            }
            foreach (var constructor in type.GetConstructors(Members))
            {
                if (bound.Contains(type) && constructor.IsPrivate) continue;
                Assert.DoesNotContain(constructor.GetParameters(), p => Protected(p.ParameterType));
            }
        }
        Assert.All(typeof(TargetObservation).GetConstructors(Members), c => Assert.True(c.IsPrivate));
        Assert.All(typeof(TargetObservation).GetProperties(Members), p => Assert.Null(p.SetMethod));
        foreach (var field in typeof(TargetObservation).GetFields(Members))
        {
            Assert.True(field.IsInitOnly);
            Assert.NotEqual(typeof(InventoryEvidence), field.FieldType);
            Assert.NotEqual(typeof(InventoryEvidence.ImportSession), field.FieldType);
        }
        Assert.True(typeof(InventoryEvidence).GetProperty("TargetData",Members)!.GetMethod!.IsPrivate);
        Assert.True(typeof(InventoryEvidence.ImportSession).GetProperty("Target",Members)!.GetMethod!.IsPrivate);
        var session = Capture(Golden); var observation = session.ReadTarget(session.Evidence)!;
        Assert.Throws<NotSupportedException>(() => ((IList<TargetReason>)observation.Reasons).Clear());
    }

    [Fact]
    public void UnownedAndUnknownRowsAndBothIndependentReasonsRemainDeterministic()
    {
        string source = Add(Golden,"#990001=CARTESIAN_POINT('',(1.,2.,3.));#990002=UNREVIEWED_RECORD();");
        var a = Capture(source); var b = Capture(source);
        var x = Gate.Evaluate(a.Evidence,a); var y = Gate.Evaluate(b.Evidence,b);
        Assert.Equal(TargetStatus.Blocked,x.Status);
        Assert.Contains(x.Reasons,r=>r.Code=="UnownedGeometry" && r.SourceId=="#990001");
        Assert.Contains(x.Reasons,r=>r.Code=="UnclassifiedSignature" && r.SourceId=="#990002");
        Assert.Equal(x.Reasons.Select(r=>(r.Layer,r.Code,r.SourceId,r.Occurrence)),y.Reasons.Select(r=>(r.Layer,r.Code,r.SourceId,r.Occurrence)));
    }

    [Theory]
    [InlineData("1","(2,2)","(0.,1.)",false)]
    [InlineData("2","(2,2)","(0.,1.)",true)]
    [InlineData("0","(2,2)","(0.,1.)",true)]
    [InlineData("1.0","(2,2)","(0.,1.)",true)]
    [InlineData("1","(1,1)","(0.,1.)",true)]
    [InlineData("1","(0,4)","(0.,1.)",true)]
    [InlineData("1","(2,2)","(0.,0.)",true)]
    [InlineData("1","(2,2)","(1.,0.)",true)]
    [InlineData("1","(2,2,1)","(0.,1.)",true)]
    public void BSplineDegreeKnotAndMultiplicityBoundaries(string degree,string multiplicities,string knots,bool invalid)
    {
        string source=Header("#1=B_SPLINE_CURVE_WITH_KNOTS('',"+degree+",(#2,#3),.UNSPECIFIED.,.F.,.F.,"+multiplicities+","+knots+",.UNSPECIFIED.);#2=CARTESIAN_POINT('',(0.,0.,0.));#3=CARTESIAN_POINT('',(1.,1.,1.));");
        using var bytes=new MemoryStream(System.Text.Encoding.UTF8.GetBytes(source));
        var inventory=new StepDataEntityReader(new StepDataInventoryValidator()).ReadCaptured(bytes).Value!;
        Assert.Equal(invalid,TargetProducer.Read(inventory).Reasons.Any(r=>r.Code=="FieldConstraintFailed"&&r.SourceId=="#1"));
    }

    [Fact]
    public void NumericReasonOrderingUsesNestedIntegerIndices()
    {
        var ordered=TargetReasonOrder.Order(new[]{new TargetReason("Records","FieldConstraintFailed",indices:[10]),new TargetReason("Records","FieldConstraintFailed",indices:[2,10]),new TargetReason("Records","FieldConstraintFailed",indices:[2,2])});
        Assert.Equal(new[]{2,2},ordered[0].Indices);Assert.Equal(new[]{2,10},ordered[1].Indices);Assert.Equal(new[]{10},ordered[2].Indices);
    }

    [Fact]
    public void All196AdmittedFieldWiringsRejectWrongParameterKindAtExactFieldPath()
    {
        using var stream = typeof(TargetProducer).Assembly.GetManifestResourceStream("MPC.TargetContract.v1.json")!;
        using var document = JsonDocument.Parse(stream);
        var profiles = document.RootElement.GetProperty("profiles");
        int reference=990000;
        string Default(JsonElement spec)
        {
            if(spec.ValueKind==JsonValueKind.String)return Default(profiles.GetProperty(spec.GetString()!));
            if(spec.TryGetProperty("one_of",out var choices))return Default(choices[0]);
            if(spec.TryGetProperty("base",out var basis))return Default(basis);
            if(spec.TryGetProperty("raw_literal",out var literal))return literal.GetString()!;
            if(spec.TryGetProperty("literals",out var literals))return literals[0].GetString()!;
            if(spec.TryGetProperty("member",out var member))return "("+string.Join(",",Enumerable.Range(1,spec.GetProperty("minimum_items").GetInt32()).Select(i=>spec.GetProperty("ordering").GetString()=="strictly increasing"?i.ToString(System.Globalization.CultureInfo.InvariantCulture):Default(member)))+")";
            if(spec.TryGetProperty("type_name",out var type))return type.GetString()+"("+string.Join(",",spec.GetProperty("parameters").EnumerateArray().Select(Default))+")";
            var kind=spec.GetProperty("kind");string k=kind.ValueKind==JsonValueKind.Array?kind[0].GetString()!:kind.GetString()!;
            return k switch {"StepStringParameter"=>"''","StepNullParameter"=>"$","StepOmittedParameter"=>"*","StepEntityReferenceParameter"=>"#"+(++reference),"StepIntegerParameter"=>"1",_=>throw new InvalidOperationException(k)};
        }
        int tested=0;
        foreach(var rule in document.RootElement.GetProperty("rules").EnumerateArray().Where(r=>r.GetProperty("admission").GetString()=="conditional"))
        foreach(var component in rule.GetProperty("components").EnumerateObject())
        foreach(var field in component.Value.GetProperty("fields").EnumerateArray())
        {
            int index=field.GetProperty("index").GetInt32();
            string[] parts=rule.GetProperty("components").EnumerateObject().Select(c=>c.Name+"("+string.Join(",",c.Value.GetProperty("fields").EnumerateArray().Select(f=>c.Name==component.Name&&f.GetProperty("index").GetInt32()==index
                ? f.GetProperty("constraint").GetString()=="Omitted"?"$":"*":Default(profiles.GetProperty(f.GetProperty("constraint").GetString()!))))+")").ToArray();
            string data="#1="+(parts.Length==1?parts[0]:"("+string.Join(" ",parts)+")")+";";
            using var bytes=new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Header(data)));
            var inventory=new StepDataEntityReader(new StepDataInventoryValidator()).ReadCaptured(bytes).Value;
            Assert.NotNull(inventory);
            var result=TargetProducer.Read(inventory);
            Assert.Contains(result.Reasons,r=>r.Code=="FieldConstraintFailed"&&r.SourceId=="#1"&&r.Component==component.Name&&r.Indices.SequenceEqual(new[]{index}));
            tested++;
        }
        Assert.Equal(196,tested);
    }
}
