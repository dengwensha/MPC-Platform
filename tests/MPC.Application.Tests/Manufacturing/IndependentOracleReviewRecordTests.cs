namespace MPC.Application.Tests.Manufacturing;

// GE03B-04B-07C: independent review RECORD gate only.
// Passing a metadata gate is NOT proof that the CAD oracle is independent.
public sealed class IndependentOracleReviewRecordTests
{
    private sealed record Review(
        string StepSha256, string ReferenceSha256,
        string ProducerId, string ReviewerId, string ReferenceToolVersion,
        string ExternalEvidenceLocator, string Decision, string Scope,
        string ReviewedAtUtc);

    private const string R01 = "E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD";
    private const string RefHash = "D7E3C91A2B4F6085D1C7E9A3206B4F8D5C1E7A3902B6D4F8C5E1A7B3902D6F4C";

    private static bool NonPlaceholder(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        !new[] { "placeholder", "dummy", "example", "synthetic", "tbd", "not independently" }
            .Any(word => value.Contains(word, StringComparison.OrdinalIgnoreCase));

    private static bool QualifiesForManualReview(Review? r, string expectedStep,
        string expectedReference)
    {
        return r is not null &&
            r.StepSha256.Equals(expectedStep, StringComparison.OrdinalIgnoreCase) &&
            r.ReferenceSha256.Equals(expectedReference, StringComparison.OrdinalIgnoreCase) &&
            !r.StepSha256.Equals(r.ReferenceSha256, StringComparison.OrdinalIgnoreCase) &&
            NonPlaceholder(r.ProducerId) && NonPlaceholder(r.ReviewerId) &&
            !r.ProducerId.Equals(r.ReviewerId, StringComparison.OrdinalIgnoreCase) &&
            NonPlaceholder(r.ReferenceToolVersion) &&
            NonPlaceholder(r.ExternalEvidenceLocator) &&
            r.Decision == "ApprovedForIndependentReview" &&
            r.Scope == "R01-SelectedFaces" &&
            DateTimeOffset.TryParse(r.ReviewedAtUtc, out var date) &&
            date.Offset == TimeSpan.Zero;
    }

    private static Review Candidate() => new(R01,RefHash,
        "fixture-producer-A","separate-reviewer-B","external-CAD-tool-v1",
        "controlled-evidence-record-001","ApprovedForIndependentReview",
        "R01-SelectedFaces","2026-10-25T12:00:00Z");

    [Fact]
    public void TiedEvidenceAndDistinctReviewer_CanEnterManualReviewOnly()
    {
        Assert.True(QualifiesForManualReview(Candidate(),R01,RefHash));
        // No accepted independence and no real face verification result.
        const int realFacesIndependentlyVerified = 0;
        Assert.Equal(0,realFacesIndependentlyVerified);
    }

    [Fact]
    public void WrongHashesAndSelfReview_Reject()
    {
        var c=Candidate();
        Assert.False(QualifiesForManualReview(c,"other-step",RefHash));
        Assert.False(QualifiesForManualReview(c,R01,"other-ref"));
        Assert.False(QualifiesForManualReview(c with {ReviewerId=c.ProducerId},R01,RefHash));
        Assert.False(QualifiesForManualReview(c with {ExternalEvidenceLocator=""},R01,RefHash));
    }

    [Fact]
    public void MissingOrUnapprovedProvenance_Reject()
    {
        var c=Candidate();
        Assert.False(QualifiesForManualReview(null,R01,RefHash));
        Assert.False(QualifiesForManualReview(c with {ReferenceToolVersion="placeholder"},R01,RefHash));
        Assert.False(QualifiesForManualReview(c with {ProducerId="dummy"},R01,RefHash));
        Assert.False(QualifiesForManualReview(c with {Decision="Rejected"},R01,RefHash));
        Assert.False(QualifiesForManualReview(c with {Scope="AllFaces"},R01,RefHash));
        Assert.False(QualifiesForManualReview(c with {ReviewedAtUtc="not-a-date"},R01,RefHash));
    }
}
