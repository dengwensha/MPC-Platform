namespace MPC.Tests.Tooling;

// TOOL-01C-01: in-memory identity proof only, not a persistent assembly model.
public sealed class ToolAssemblyRevisionContractTests
{
    private sealed record ProductRevision(string RevisionId, string ProductId,
        string Kind, string SourceHash, string? MappingStatus);
    private sealed record Component(string Role, string ProductRevisionId);
    private sealed record Assembly(string AssemblyId, int Revision,
        IReadOnlyList<Component> Components);

    private static ProductRevision Holder() =>
        new("holder-r1", "5729638", "ToolHolder", "source-a", "Reviewed");
    private static ProductRevision Insert() =>
        new("insert-r1", "7860702", "CuttingInsert", "source-b", "Reviewed");

    private static bool Eligible(Assembly assembly, IReadOnlyList<ProductRevision> products)
    {
        if (string.IsNullOrWhiteSpace(assembly.AssemblyId) || assembly.Revision < 1 ||
            assembly.Components.Count != 2 ||
            assembly.Components.Count(c => c.Role == "Holder") != 1 ||
            assembly.Components.Count(c => c.Role == "Insert") != 1)
            return false;
        var grouped = products.GroupBy(p => p.RevisionId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
        foreach (var component in assembly.Components)
        {
            if (!grouped.TryGetValue(component.ProductRevisionId, out var matches) ||
                matches.Length != 1 || matches[0].MappingStatus != "Reviewed" ||
                string.IsNullOrWhiteSpace(matches[0].SourceHash) ||
                matches[0].Kind != (component.Role == "Holder" ? "ToolHolder" : "CuttingInsert"))
                return false;
        }
        return true;
    }

    private static Assembly Sample(string id = "assembly-a", int revision = 1) =>
        new(id,revision,[new("Holder","holder-r1"),new("Insert","insert-r1")]);

    [Fact]
    public void TwoAssemblies_CanReuseSameCatalogRevisionWithoutMergingIdentity()
    {
        var catalog = new[] { Holder(),Insert() };
        var first = Sample();
        var second = Sample("assembly-b");
        Assert.True(Eligible(first,catalog));
        Assert.True(Eligible(second,catalog));
        Assert.NotEqual(first.AssemblyId,second.AssemblyId);
        Assert.Equal(first.Components[1].ProductRevisionId,
            second.Components[1].ProductRevisionId);
    }

    [Fact]
    public void MissingOrDuplicateReferences_Reject()
    {
        var catalog = new[] { Holder(),Insert() };
        Assert.False(Eligible(Sample(),[Holder()]));
        Assert.False(Eligible(Sample(),[Holder(),Insert(),Insert()]));
        Assert.False(Eligible(Sample() with {
            Components=[new("Holder","holder-r1"),new("Holder","holder-r1")] },catalog));
        Assert.False(Eligible(Sample() with {
            Components=[new("Holder","insert-r1"),new("Insert","holder-r1")] },catalog));
        Assert.False(Eligible(Sample() with {Revision=0},catalog));
    }

    [Fact]
    public void UnreviewedVendorMapping_RejectsAssembly()
    {
        Assert.False(Eligible(Sample(),[Holder(),Insert() with {MappingStatus="NeedsMapping"}]));
    }

    [Fact]
    public void CatalogRevisionChange_DoesNotSilentlyRebindHistoricalAssembly()
    {
        var historical=Sample();
        var oldCatalog=new[] {Holder(),Insert()};
        var newCatalog=new[] {Holder(),Insert() with {RevisionId="insert-r2"}};
        Assert.True(Eligible(historical,oldCatalog));
        Assert.False(Eligible(historical,newCatalog));
        var explicitlyRevised = new Assembly(historical.AssemblyId,2,
            [new("Holder","holder-r1"),new("Insert","insert-r2")]);
        Assert.True(Eligible(explicitlyRevised,newCatalog));
        Assert.Equal("insert-r1",historical.Components[1].ProductRevisionId);
    }
}
