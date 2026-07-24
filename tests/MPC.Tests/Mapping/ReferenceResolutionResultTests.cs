using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class ReferenceResolutionResultTests
{
    [Fact]
    public void Resolved_WithReferenceAndValue_ReturnsResolvedResult()
    {
        var value = new TestEntity("entity");

        var result =
            ReferenceResolutionResult<TestEntity>.Resolved(
                "#57",
                value);

        Assert.Equal("#57", result.ReferenceId);
        Assert.Equal(
            ReferenceResolutionStatus.Resolved,
            result.Status);

        Assert.True(result.IsResolved);
        Assert.Same(value, result.Value);
        Assert.Empty(result.Diagnostics);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolved_WithInvalidReferenceId_ThrowsArgumentException(
        string? referenceId)
    {
        Assert.Throws<ArgumentException>(
            () => ReferenceResolutionResult<TestEntity>.Resolved(
                referenceId!,
                new TestEntity("entity")));
    }

    [Fact]
    public void Resolved_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => ReferenceResolutionResult<TestEntity>.Resolved(
                "#57",
                null!));
    }

    [Fact]
    public void Failure_WithDiagnostic_ReturnsFailedResult()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP-REF-001",
            "Referenced entity could not be found.",
            MappingDiagnosticSeverity.Error);

        var result =
            ReferenceResolutionResult<TestEntity>.Failure(
                "#57",
                new[] { diagnostic });

        Assert.Equal("#57", result.ReferenceId);
        Assert.Equal(
            ReferenceResolutionStatus.Failed,
            result.Status);

        Assert.False(result.IsResolved);
        Assert.Null(result.Value);

        var actualDiagnostic = Assert.Single(result.Diagnostics);
        Assert.Same(diagnostic, actualDiagnostic);
    }

    [Fact]
    public void Failure_WithEmptyDiagnostics_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () => ReferenceResolutionResult<TestEntity>.Failure(
                "#57",
                Array.Empty<MappingDiagnostic>()));
    }

    [Fact]
    public void Diagnostics_AreDefensivelyCopied()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP-REF-001",
            "Referenced entity could not be found.",
            MappingDiagnosticSeverity.Error);

        var diagnostics = new List<MappingDiagnostic>
        {
            diagnostic
        };

        var result =
            ReferenceResolutionResult<TestEntity>.Failure(
                "#57",
                diagnostics);

        diagnostics.Clear();

        var actualDiagnostic = Assert.Single(result.Diagnostics);
        Assert.Same(diagnostic, actualDiagnostic);
    }

    [Fact]
    public void Diagnostics_CannotBeModifiedThroughResult()
    {
        var diagnostic = new MappingDiagnostic(
            "MAP-REF-001",
            "Referenced entity could not be found.",
            MappingDiagnosticSeverity.Error);

        var result =
            ReferenceResolutionResult<TestEntity>.Failure(
                "#57",
                new[] { diagnostic });

        var diagnostics =
            Assert.IsAssignableFrom<IList<MappingDiagnostic>>(
                result.Diagnostics);

        Assert.Throws<NotSupportedException>(
            () => diagnostics.Add(
                new MappingDiagnostic(
                    "MAP-REF-002",
                    "Another resolution error.",
                    MappingDiagnosticSeverity.Error)));
    }

    private sealed class TestEntity
    {
        public TestEntity(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}