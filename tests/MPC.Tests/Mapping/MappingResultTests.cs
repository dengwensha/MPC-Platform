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