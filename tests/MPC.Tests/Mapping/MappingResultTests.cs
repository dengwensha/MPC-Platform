using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class MappingResultTests
{
    [Fact]
    public void Success_WithValue_ReturnsSucceededResult()
    {
        var value = new TestCanonicalObject("canonical");

        var result = MappingResult<TestCanonicalObject>.Success(value);

        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.True(result.IsSuccessful);
        Assert.Same(value, result.Value);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => MappingResult<TestCanonicalObject>.Success(null!));
    }

    [Fact]
    public void SuccessWithDiagnostics_WithValueAndDiagnostic_ReturnsExpectedResult()
    {
        var value = new TestCanonicalObject("canonical");

        var diagnostic = new MappingDiagnostic(
            "MAP001",
            "Optional source information was not available.",
            MappingDiagnosticSeverity.Warning);

        var result =
            MappingResult<TestCanonicalObject>.SuccessWithDiagnostics(
                value,
                new[] { diagnostic });

        Assert.Equal(
            MappingStatus.SucceededWithDiagnostics,
            result.Status);

        Assert.True(result.IsSuccessful);
        Assert.Same(value, result.Value);

        var actualDiagnostic = Assert.Single(result.Diagnostics);
        Assert.Same(diagnostic, actualDiagnostic);
    }

    [Fact]
    public void SuccessWithDiagnostics_WithEmptyDiagnostics_ThrowsInvalidOperationException()
    {
        var value = new TestCanonicalObject("canonical");

        Assert.Throws<InvalidOperationException>(
            () => MappingResult<TestCanonicalObject>
                .SuccessWithDiagnostics(
                    value,
                    Array.Empty<MappingDiagnostic>()));
    }

    [Fact]
    public void Failure_WithDiagnostic_ReturnsFailedResult()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP002",
            "Source entity could not be mapped.",
            MappingDiagnosticSeverity.Error);

        var result =
            MappingResult<TestCanonicalObject>.Failure(
                new[] { diagnostic });

        Assert.Equal(MappingStatus.Failed, result.Status);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);

        var actualDiagnostic = Assert.Single(result.Diagnostics);
        Assert.Same(diagnostic, actualDiagnostic);
    }

    [Fact]
    public void Failure_WithEmptyDiagnostics_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => MappingResult<TestCanonicalObject>.Failure(
                Array.Empty<MappingDiagnostic>()));
    }

    [Fact]
    public void Failure_WithEquivalentDiagnostics_DeduplicatesAndPreservesFirstInstance()
    {
        var first = new MappingDiagnostic(
            "MAP-STEP-FACE-001",
            "STEP ADVANCED_FACE '#100' references unsupported surface '#200'.",
            MappingDiagnosticSeverity.Error);

        var equivalent = new MappingDiagnostic(
            "MAP-STEP-FACE-001",
            "STEP ADVANCED_FACE '#100' references unsupported surface '#200'.",
            MappingDiagnosticSeverity.Error);

        var result =
            MappingResult<TestCanonicalObject>.Failure(
                new[] { first, equivalent });

        var actualDiagnostic = Assert.Single(result.Diagnostics);
        Assert.Same(first, actualDiagnostic);
    }

    [Fact]
    public void Failure_WithDifferentSourceMessages_PreservesBothDiagnostics()
    {
        var first = new MappingDiagnostic(
            "MAP-STEP-FACE-001",
            "STEP ADVANCED_FACE '#100' references unsupported surface '#200'.",
            MappingDiagnosticSeverity.Error);

        var second = new MappingDiagnostic(
            "MAP-STEP-FACE-001",
            "STEP ADVANCED_FACE '#101' references unsupported surface '#200'.",
            MappingDiagnosticSeverity.Error);

        var result =
            MappingResult<TestCanonicalObject>.Failure(
                new[] { first, second });

        Assert.Equal(2, result.Diagnostics.Count);
        Assert.Same(first, result.Diagnostics[0]);
        Assert.Same(second, result.Diagnostics[1]);
    }

    [Fact]
    public void Failure_WithDifferentSeverities_PreservesBothDiagnostics()
    {
        const string message =
            "STEP entity '#100' could not be mapped.";

        var warning = new MappingDiagnostic(
            "MAP001",
            message,
            MappingDiagnosticSeverity.Warning);

        var error = new MappingDiagnostic(
            "MAP001",
            message,
            MappingDiagnosticSeverity.Error);

        var result =
            MappingResult<TestCanonicalObject>.Failure(
                new[] { warning, error });

        Assert.Equal(2, result.Diagnostics.Count);
        Assert.Same(warning, result.Diagnostics[0]);
        Assert.Same(error, result.Diagnostics[1]);
    }

    [Fact]
    public void Failure_WithInterleavedDuplicates_PreservesFirstSeenOrder()
    {
        var first = new MappingDiagnostic(
            "MAP001",
            "First diagnostic.",
            MappingDiagnosticSeverity.Error);

        var second = new MappingDiagnostic(
            "MAP002",
            "Second diagnostic.",
            MappingDiagnosticSeverity.Error);

        var duplicateFirst = new MappingDiagnostic(
            "MAP001",
            "First diagnostic.",
            MappingDiagnosticSeverity.Error);

        var result =
            MappingResult<TestCanonicalObject>.Failure(
                new[] { first, second, duplicateFirst });

        Assert.Equal(2, result.Diagnostics.Count);
        Assert.Same(first, result.Diagnostics[0]);
        Assert.Same(second, result.Diagnostics[1]);
    }

    [Fact]
    public void Diagnostics_AreDefensivelyCopied()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP001",
            "Mapping warning.",
            MappingDiagnosticSeverity.Warning);

        var diagnostics = new List<MappingDiagnostic>
        {
            diagnostic
        };

        var result =
            MappingResult<TestCanonicalObject>.SuccessWithDiagnostics(
                new TestCanonicalObject("canonical"),
                diagnostics);

        diagnostics.Clear();

        var actualDiagnostic = Assert.Single(result.Diagnostics);
        Assert.Same(diagnostic, actualDiagnostic);
    }

    [Fact]
    public void Diagnostics_CannotBeModifiedThroughResult()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP001",
            "Mapping warning.",
            MappingDiagnosticSeverity.Warning);

        var result =
            MappingResult<TestCanonicalObject>.SuccessWithDiagnostics(
                new TestCanonicalObject("canonical"),
                new[] { diagnostic });

        var diagnostics =
            Assert.IsAssignableFrom<IList<MappingDiagnostic>>(
                result.Diagnostics);

        Assert.Throws<NotSupportedException>(
            () => diagnostics.Add(
                new MappingDiagnostic(
                    "MAP002",
                    "Another warning.",
                    MappingDiagnosticSeverity.Warning)));
    }

    private sealed class TestCanonicalObject
    {
        public TestCanonicalObject(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
