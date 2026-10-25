namespace MPC.Tests.Tooling;

// TOOL-01B-03: test-only vendor vocabulary mapping proof.
// No runtime importer, no claim that mounting-style codes are established.
public sealed class VendorFieldMappingContractTests
{
    public enum MappingStatus { AcceptedForReview, NeedsMapping, Rejected }
    private sealed record VendorField(string ProductId, string Name, string? RawValue,
        string? RawUnit, string? SourceRevision);

    private static MappingStatus Map(VendorField field)
    {
        if (string.IsNullOrWhiteSpace(field.ProductId) ||
            string.IsNullOrWhiteSpace(field.SourceRevision) ||
            string.IsNullOrWhiteSpace(field.RawValue))
            return MappingStatus.Rejected;

        // Numeric dimensions require explicit, recognized source units.
        if (field.Name is "IC" or "Thickness")
        {
            if (field.RawUnit != "mm" ||
                !double.TryParse(field.RawValue,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double value) ||
                !double.IsFinite(value) || value <= 0)
                return MappingStatus.NeedsMapping;
            return MappingStatus.AcceptedForReview;
        }

        if (field.Name == "Grade")
            return MappingStatus.AcceptedForReview;

        // No independently reviewed vendor-to-integer conversion is available.
        if (field.Name is "MTP" or "IFS" or "MountingStyleCode" or
            "MachineConnection" or "SeatSizeCode" or "Hand")
            return MappingStatus.NeedsMapping;

        return MappingStatus.NeedsMapping;
    }

    [Theory]
    [InlineData("7860702", "IC", "19.05", "mm", MappingStatus.AcceptedForReview)]
    [InlineData("7860702", "Thickness", "6.35", "mm", MappingStatus.AcceptedForReview)]
    [InlineData("7860702", "Grade", "4425", null, MappingStatus.AcceptedForReview)]
    [InlineData("7860702", "IC", "0.75", "inch", MappingStatus.NeedsMapping)]
    [InlineData("7860702", "Thickness", "6.35", null, MappingStatus.NeedsMapping)]
    [InlineData("7860702", "IC", "NaN", "mm", MappingStatus.NeedsMapping)]
    [InlineData("5729638", "MTP", "clamp", null, MappingStatus.NeedsMapping)]
    [InlineData("7860702", "IFS", "hole", null, MappingStatus.NeedsMapping)]
    [InlineData("5729638", "MachineConnection", "C8", null, MappingStatus.NeedsMapping)]
    [InlineData("7860702", "SeatSizeCode", "19", null, MappingStatus.NeedsMapping)]
    public void SourceFieldMapping_StaysWithinReviewedVocabulary(
        string productId, string field, string? rawValue, string? rawUnit,
        MappingStatus expected)
    {
        Assert.Equal(expected, Map(new VendorField(productId,field,
            rawValue,rawUnit,"vendor-review-v1")));
    }

    [Fact]
    public void MissingSourceRevisionAndProductIdentity_Reject()
    {
        Assert.Equal(MappingStatus.Rejected,
            Map(new VendorField("", "IC", "19.05", "mm", "rev1")));
        Assert.Equal(MappingStatus.Rejected,
            Map(new VendorField("7860702", "IC", "19.05", "mm", null)));
        Assert.Equal(MappingStatus.Rejected,
            Map(new VendorField("7860702", "IC", null, "mm", "rev1")));
    }

    [Fact]
    public void UnknownSourceVocabulary_NeverInventsMountingCode()
    {
        Assert.Equal(MappingStatus.NeedsMapping,
            Map(new VendorField("5729638","vendor-new-mount-code","M42",null,"rev1")));
        const int IndependentlyVerifiedAssemblyMappings = 0;
        Assert.Equal(0, IndependentlyVerifiedAssemblyMappings);
    }
}
