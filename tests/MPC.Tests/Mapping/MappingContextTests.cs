using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class MappingContextTests
{
    [Fact]
    public void Constructor_WithResolver_CreatesEmptyContext()
    {
        var resolver = new StubReferenceResolver();

        var context = new MappingContext(resolver);

        Assert.Same(resolver, context.ReferenceResolver);
        Assert.Empty(context.Diagnostics);
        Assert.False(context.HasDiagnostics);
        Assert.False(context.HasErrors);
    }

    [Fact]
    public void Constructor_WithNullResolver_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MappingContext(null!));
    }

    [Fact]
    public void AddDiagnostic_WithValidDiagnostic_AddsDiagnostic()
    {
        var context = CreateContext();

        var diagnostic = new MappingDiagnostic(
            "MAP001",
            "Source information was incomplete.",
            MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(diagnostic);

        var actualDiagnostic =
            Assert.Single(context.Diagnostics);

        Assert.Same(diagnostic, actualDiagnostic);
        Assert.True(context.HasDiagnostics);
        Assert.False(context.HasErrors);
    }

    [Fact]
    public void AddDiagnostic_WithNullDiagnostic_ThrowsArgumentNullException()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentNullException>(
            () => context.AddDiagnostic(null!));
    }

    [Fact]
    public void AddDiagnostic_PreservesInsertionOrder()
    {
        var context = CreateContext();

        var firstDiagnostic = new MappingDiagnostic(
            "MAP001",
            "First diagnostic.",
            MappingDiagnosticSeverity.Information);

        var secondDiagnostic = new MappingDiagnostic(
            "MAP002",
            "Second diagnostic.",
            MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(firstDiagnostic);
        context.AddDiagnostic(secondDiagnostic);

        Assert.Equal(2, context.Diagnostics.Count);
        Assert.Same(
            firstDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            secondDiagnostic,
            context.Diagnostics[1]);
    }

    [Fact]
    public void AddDiagnostics_WithValidDiagnostics_AddsAllDiagnostics()
    {
        var context = CreateContext();

        var firstDiagnostic = new MappingDiagnostic(
            "REF001",
            "Referenced entity could not be resolved.",
            MappingDiagnosticSeverity.Error);

        var secondDiagnostic = new MappingDiagnostic(
            "REF002",
            "Referenced entity had an unexpected type.",
            MappingDiagnosticSeverity.Warning);

        context.AddDiagnostics(
            new[]
            {
                firstDiagnostic,
                secondDiagnostic
            });

        Assert.Equal(2, context.Diagnostics.Count);
        Assert.Same(
            firstDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            secondDiagnostic,
            context.Diagnostics[1]);

        Assert.True(context.HasDiagnostics);
        Assert.True(context.HasErrors);
    }

    [Fact]
    public void AddDiagnostics_WithEmptyCollection_DoesNotAddDiagnostic()
    {
        var context = CreateContext();

        context.AddDiagnostics(
            Array.Empty<MappingDiagnostic>());

        Assert.Empty(context.Diagnostics);
        Assert.False(context.HasDiagnostics);
        Assert.False(context.HasErrors);
    }

    [Fact]
    public void AddDiagnostics_WithNullCollection_ThrowsArgumentNullException()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentNullException>(
            () => context.AddDiagnostics(null!));
    }

    [Fact]
    public void AddDiagnostics_WithNullItem_ThrowsArgumentNullException()
    {
        var context = CreateContext();

        var validDiagnostic = new MappingDiagnostic(
            "REF001",
            "Valid diagnostic.",
            MappingDiagnosticSeverity.Warning);

        MappingDiagnostic[] diagnostics =
        {
            validDiagnostic,
            null!
        };

        Assert.Throws<ArgumentNullException>(
            () => context.AddDiagnostics(diagnostics));

        Assert.Single(context.Diagnostics);
        Assert.Same(
            validDiagnostic,
            context.Diagnostics[0]);
    }

    [Fact]
    public void HasErrors_WithErrorDiagnostic_ReturnsTrue()
    {
        var context = CreateContext();

        context.AddDiagnostic(
            new MappingDiagnostic(
                "MAP003",
                "Mapping failed.",
                MappingDiagnosticSeverity.Error));

        Assert.True(context.HasDiagnostics);
        Assert.True(context.HasErrors);
    }

    [Fact]
    public void HasErrors_WithInformationAndWarningDiagnostics_ReturnsFalse()
    {
        var context = CreateContext();

        context.AddDiagnostic(
            new MappingDiagnostic(
                "MAP001",
                "Mapping information.",
                MappingDiagnosticSeverity.Information));

        context.AddDiagnostic(
            new MappingDiagnostic(
                "MAP002",
                "Mapping warning.",
                MappingDiagnosticSeverity.Warning));

        Assert.True(context.HasDiagnostics);
        Assert.False(context.HasErrors);
    }

    [Fact]
    public void Diagnostics_CannotBeModifiedThroughContext()
    {
        var context = CreateContext();

        context.AddDiagnostic(
            new MappingDiagnostic(
                "MAP001",
                "Mapping warning.",
                MappingDiagnosticSeverity.Warning));

        var diagnostics =
            Assert.IsAssignableFrom<IList<MappingDiagnostic>>(
                context.Diagnostics);

        Assert.Throws<NotSupportedException>(
            () => diagnostics.Add(
                new MappingDiagnostic(
                    "MAP002",
                    "Another warning.",
                    MappingDiagnosticSeverity.Warning)));
    }

    private static MappingContext CreateContext()
    {
        return new MappingContext(
            new StubReferenceResolver());
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            throw new NotSupportedException(
                "Reference resolution is not required by these tests.");
        }
    }
}