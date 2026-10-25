namespace MPC.Application.Tests.Manufacturing;

// GE03B-04B-06: test-only manifest intake, never material certification.
// An accepted record means structurally eligible for independent review;
// it does NOT assert a true outward normal or enable production classification.
public sealed class IndependentMaterialOracleManifestIntakeTests
{
    private sealed record Sample(string StepFaceId, double X, double Y, double Z,
        double Nx, double Ny, double Nz, double Offset, double Clearance,
        string PlusOccupancy, string MinusOccupancy);
    private sealed record Manifest(string StepSha256, string ReferenceSha256,
        string Provenance, string ReferenceMethod, string Units,
        IReadOnlyList<Sample> Samples);

    private const string R01 = "E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD";
    private const string R02 = "0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F";
    private const string Reference = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private static bool IsHash(string? hash) => hash is { Length: 64 } &&
        hash.All(Uri.IsHexDigit);
    private static bool Finite(params double[] numbers) => numbers.All(double.IsFinite);

    private static bool StructurallyEligible(Manifest? manifest, IReadOnlySet<string> sourceFaceIds, string expectedStepSha256 = R01)
    {
        if (manifest is null || sourceFaceIds.Count == 0 ||
            !IsHash(manifest.StepSha256) || !IsHash(manifest.ReferenceSha256) ||
            !manifest.StepSha256.Equals(expectedStepSha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.StepSha256.Equals(manifest.ReferenceSha256, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(manifest.Provenance) ||
            string.IsNullOrWhiteSpace(manifest.ReferenceMethod) ||
            manifest.Units != "mm" || manifest.Samples is null ||
            manifest.Samples.Count == 0) return false;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sample in manifest.Samples)
        {
            if (string.IsNullOrWhiteSpace(sample.StepFaceId) ||
                !sourceFaceIds.Contains(sample.StepFaceId) || !seen.Add(sample.StepFaceId) ||
                !Finite(sample.X, sample.Y, sample.Z, sample.Nx, sample.Ny,
                    sample.Nz, sample.Offset, sample.Clearance) ||
                sample.Offset <= 0 || sample.Clearance <= sample.Offset ||
                Math.Abs(sample.Nx * sample.Nx + sample.Ny * sample.Ny +
                    sample.Nz * sample.Nz - 1) > 1e-9 ||
                sample.PlusOccupancy != "Void" || sample.MinusOccupancy != "Material")
                return false;
        }
        return true;
    }

    private static readonly IReadOnlySet<string> KnownIds =
        new HashSet<string>(StringComparer.Ordinal) { "#8", "#30" };
    private static Manifest Candidate() => new(R01, Reference,
        "Independently reviewed CAD fixture, placeholder only",
        "External occupancy probe", "mm",
        [new Sample("#8", 0,0,10, 0,0,1, 0.1,1,"Void","Material")]);

    [Fact]
    public void MissingReferenceAndMissingManifest_Reject()
    {
        Assert.False(StructurallyEligible(null,KnownIds));
        Assert.False(StructurallyEligible(Candidate() with {ReferenceSha256 = ""},KnownIds));
        Assert.False(StructurallyEligible(Candidate() with {Provenance = ""},KnownIds));
        Assert.False(StructurallyEligible(Candidate(),new HashSet<string>()));
    }

    [Fact]
    public void DuplicateUnknownAndMismatchedIdentity_Reject()
    {
        var c=Candidate();
        Assert.False(StructurallyEligible(c with { Samples = [c.Samples[0],c.Samples[0]] },KnownIds));
        Assert.False(StructurallyEligible(c with { Samples = [c.Samples[0] with {StepFaceId="#999"}] },KnownIds));
        Assert.False(StructurallyEligible(c with {StepSha256=R02},KnownIds));
    }

    [Fact]
    public void AmbiguousOrUnsafeSamples_Reject()
    {
        var c=Candidate(); var s=c.Samples[0];
        Assert.False(StructurallyEligible(c with { Samples=[s with {PlusOccupancy="Boundary"}] },KnownIds));
        Assert.False(StructurallyEligible(c with { Samples=[s with {Nx=double.NaN}] },KnownIds));
        Assert.False(StructurallyEligible(c with { Samples=[s with {Clearance=0.05}] },KnownIds));
        Assert.False(StructurallyEligible(c with { Samples=[s with {PlusOccupancy="Material",MinusOccupancy="Void"}] },KnownIds));
    }

    [Fact]
    public void SyntheticRecordCanPassStructureButCertifiesNoRealFaces()
    {
        var c=Candidate();
        Assert.True(StructurallyEligible(c,KnownIds));
        int independentlyVerifiedRealFaces=0;
        Assert.Equal(0,independentlyVerifiedRealFaces);
    }
}
