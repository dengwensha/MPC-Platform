using System.Reflection;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitOwnershipTests
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [Fact]
    public void BoundObservationCannotBeConstructedFromCallerMetadata()
    {
        var constructors = typeof(HeaderUnitObservation).GetConstructors(Members);
        Assert.NotEmpty(constructors);
        Assert.All(constructors, c => Assert.True(c.IsPrivate));
        Assert.All(typeof(HeaderUnitObservation).GetProperties(Members), p => Assert.Null(p.SetMethod));
    }

    [Fact]
    public void BoundObservationIsNotDirectlyReadableFromEvidence()
    {
        var property = typeof(InventoryEvidence).GetProperty("HeaderUnits", Members);
        Assert.NotNull(property);
        Assert.True(property.GetMethod!.IsPrivate);
        Assert.Null(property.SetMethod);
    }

    [Fact]
    public void SessionProvidesAnOwnershipCheckedObservationEntryPoint()
    {
        var method = typeof(InventoryEvidence.ImportSession).GetMethod("ReadHeaderUnits", Members);
        Assert.NotNull(method);
        Assert.Equal(typeof(HeaderUnitObservation), method.ReturnType);
        Assert.Equal(new[] { typeof(InventoryEvidence) }, method.GetParameters().Select(p => p.ParameterType));
    }

    [Fact]
    public void ParserOutputAndMetadataCannotIssueBoundObservations()
    {
        Assert.Equal(typeof(HeaderUnitData), typeof(HeaderUnitProducer).GetMethod("ReadCaptured",
            BindingFlags.Static | BindingFlags.NonPublic)!.ReturnType);
        foreach (var type in typeof(InventoryEvidence).Assembly.GetTypes()
            .Where(t => t.Namespace == "MPC.Application.Inventory"))
        foreach (var method in type.GetMethods(Members | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (method.ReturnType != typeof(HeaderUnitObservation)) continue;
            if (type == typeof(InventoryEvidence))
            {
                Assert.Equal("get_HeaderUnits", method.Name);
                Assert.True(method.IsPrivate);
            }
            else if (type == typeof(InventoryEvidence.ImportSession))
            {
                Assert.Equal("ReadHeaderUnits", method.Name);
                Assert.Equal(new[] { typeof(InventoryEvidence) }, method.GetParameters().Select(p => p.ParameterType));
            }
            else
            {
                Assert.Equal(typeof(HeaderUnitObservation.EvidenceBinding), type);
                Assert.Equal("Bind", method.Name);
                Assert.Equal(new[] { typeof(InventoryEvidence) }, method.GetParameters().Select(p => p.ParameterType));
            }
        }
        Assert.Throws<ArgumentNullException>(() => HeaderUnitObservation.EvidenceBinding.Bind(null!));
    }

    [Fact]
    public void SameHashFromAnotherCaptureDoesNotGrantOwnership()
    {
        WithSource(path =>
        {
            var first = Capture(path); var second = Capture(path);
            Assert.Equal(first.DocumentSha256, second.DocumentSha256);
            Assert.NotSame(first.Evidence, second.Evidence);
            Assert.NotNull(first.ReadHeaderUnits(first.Evidence));
            Assert.Same(first.ReadHeaderUnits(first.Evidence), first.ReadHeaderUnits(first.Evidence));
            Assert.Null(first.ReadHeaderUnits(second.Evidence));
            Assert.Null(second.ReadHeaderUnits(first.Evidence));
            Assert.Null(first.ReadHeaderUnits(null));
        });
    }

    [Fact]
    public void CleanNeutralDataCannotReplaceBlockedOwnedObservation()
    {
        WithSource(path =>
        {
            File.WriteAllText(path, Source("UNKNOWN"));
            var blocked = Capture(path);
            File.WriteAllText(path, Source("AUTOMOTIVE_DESIGN"));
            var clean = Capture(path);
            var fakeClean = new HeaderUnitData("AUTOMOTIVE_DESIGN", "1", [], [], []);
            Assert.Empty(fakeClean.HeaderReasons);
            Assert.NotEqual(blocked.DocumentSha256, clean.DocumentSha256);
            Assert.Null(blocked.ReadHeaderUnits(clean.Evidence));
            Assert.Contains("UnsupportedSchema", blocked.ReadHeaderUnits(blocked.Evidence)!.HeaderReasons);
            Assert.Empty(clean.ReadHeaderUnits(clean.Evidence)!.HeaderReasons);
            Assert.Contains("UnsupportedSchema", HeaderUnitObservation.EvidenceBinding.Bind(blocked.Evidence).HeaderReasons);
        });
    }

    [Fact]
    public void BoundObservationDoesNotRetainOwnerOrMutableCapture()
    {
        Assert.All(typeof(HeaderUnitObservation).GetFields(Members), field =>
        {
            Assert.True(field.IsInitOnly);
            Assert.NotEqual(typeof(InventoryEvidence), field.FieldType);
            Assert.NotEqual(typeof(InventoryEvidence.ImportSession), field.FieldType);
            Assert.NotEqual(typeof(byte[]), field.FieldType);
            Assert.False(typeof(Stream).IsAssignableFrom(field.FieldType));
        });
        WithSource(path =>
        {
            var session = Capture(path);
            var observation = session.ReadHeaderUnits(session.Evidence)!;
            Assert.Throws<NotSupportedException>(() => ((IList<string>)observation.HeaderReasons).Add("fake"));
            Assert.Throws<NotSupportedException>(() => ((IList<ContextUnitData>)observation.Contexts).Clear());
            var context = Assert.Single(observation.Contexts);
            Assert.Throws<NotSupportedException>(() => ((IList<UnitDefinitionData>)context.Units).Clear());
        });
    }

    [Fact]
    public void SourceReplacementCannotChangeTheBoundObservation()
    {
        WithSource(path =>
        {
            var result = InventoryEvidence.ImportSession.CaptureFromFile(path,
                () => File.WriteAllText(path, Source("UNKNOWN")));
            Assert.Null(result.Failure);
            var session = Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
            Assert.Empty(session.ReadHeaderUnits(session.Evidence)!.HeaderReasons);
            var replaced = Capture(path);
            Assert.Contains("UnsupportedSchema", replaced.ReadHeaderUnits(replaced.Evidence)!.HeaderReasons);
        });
    }

    private static InventoryEvidence.ImportSession Capture(string path)
    {
        var result = InventoryEvidence.ImportSession.CaptureFromFile(path);
        Assert.Null(result.Failure);
        return Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
    }

    private static string Source(string schema) =>
        "ISO-10303-21;HEADER;FILE_DESCRIPTION(('test'),'1');FILE_NAME('a','',(''),(''),'','','');FILE_SCHEMA(('" + schema + "'));ENDSEC;DATA;" +
        "#1=(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT(.MILLI.,.METRE.));" +
        "#2=(NAMED_UNIT(*) PLANE_ANGLE_UNIT() SI_UNIT($,.RADIAN.));" +
        "#9=(GEOMETRIC_REPRESENTATION_CONTEXT(3) GLOBAL_UNIT_ASSIGNED_CONTEXT((#1,#2)) REPRESENTATION_CONTEXT('',''));ENDSEC;END-ISO-10303-21;";

    private static void WithSource(Action<string> action)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try { File.WriteAllText(path, Source("AUTOMOTIVE_DESIGN")); action(path); }
        finally { File.Delete(path); }
    }
}
