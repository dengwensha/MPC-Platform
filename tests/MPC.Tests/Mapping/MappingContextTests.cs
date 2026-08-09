using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class MappingContextTests
{
    [Fact]
    public void Constructor_WithResolver_CreatesEmptyContext()
    {
        var resolver =
            new StubReferenceResolver();

        var context =
            new MappingContext(
                resolver);

        Assert.Same(
            resolver,
            context.ReferenceResolver);

        Assert.Empty(
            context.Diagnostics);

        Assert.False(
            context.HasDiagnostics);

        Assert.False(
            context.HasErrors);
    }

    [Fact]
    public void Constructor_WithNullResolver_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MappingContext(
                null!));
    }

    [Fact]
    public void AddDiagnostic_WithValidDiagnostic_AddsDiagnostic()
    {
        var context =
            CreateContext();

        var diagnostic =
            new MappingDiagnostic(
                "MAP001",
                "Source information was incomplete.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            diagnostic);

        MappingDiagnostic actualDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            diagnostic,
            actualDiagnostic);

        Assert.True(
            context.HasDiagnostics);

        Assert.False(
            context.HasErrors);
    }

    [Fact]
    public void AddDiagnostic_WithNullDiagnostic_ThrowsArgumentNullException()
    {
        var context =
            CreateContext();

        Assert.Throws<ArgumentNullException>(
            () => context.AddDiagnostic(
                null!));
    }

    [Fact]
    public void AddDiagnostic_PreservesInsertionOrder()
    {
        var context =
            CreateContext();

        var firstDiagnostic =
            new MappingDiagnostic(
                "MAP001",
                "First diagnostic.",
                MappingDiagnosticSeverity.Information);

        var secondDiagnostic =
            new MappingDiagnostic(
                "MAP002",
                "Second diagnostic.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            firstDiagnostic);

        context.AddDiagnostic(
            secondDiagnostic);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

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
        var context =
            CreateContext();

        var firstDiagnostic =
            new MappingDiagnostic(
                "REF001",
                "Referenced entity could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var secondDiagnostic =
            new MappingDiagnostic(
                "REF002",
                "Referenced entity had an unexpected type.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostics(
            [
                firstDiagnostic,
                secondDiagnostic
            ]);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            firstDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            secondDiagnostic,
            context.Diagnostics[1]);

        Assert.True(
            context.HasDiagnostics);

        Assert.True(
            context.HasErrors);
    }

    [Fact]
    public void AddDiagnostics_WithEmptyCollection_DoesNotAddDiagnostic()
    {
        var context =
            CreateContext();

        context.AddDiagnostics(
            Array.Empty<MappingDiagnostic>());

        Assert.Empty(
            context.Diagnostics);

        Assert.False(
            context.HasDiagnostics);

        Assert.False(
            context.HasErrors);
    }

    [Fact]
    public void AddDiagnostics_WithNullCollection_ThrowsArgumentNullException()
    {
        var context =
            CreateContext();

        Assert.Throws<ArgumentNullException>(
            () => context.AddDiagnostics(
                null!));
    }

    [Fact]
    public void AddDiagnostics_WithNullItem_ThrowsArgumentNullException()
    {
        var context =
            CreateContext();

        var validDiagnostic =
            new MappingDiagnostic(
                "REF001",
                "Valid diagnostic.",
                MappingDiagnosticSeverity.Warning);

        MappingDiagnostic[] diagnostics =
        [
            validDiagnostic,
            null!
        ];

        Assert.Throws<ArgumentNullException>(
            () => context.AddDiagnostics(
                diagnostics));

        Assert.Single(
            context.Diagnostics);

        Assert.Same(
            validDiagnostic,
            context.Diagnostics[0]);
    }

    [Fact]
    public void HasErrors_WithErrorDiagnostic_ReturnsTrue()
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            new MappingDiagnostic(
                "MAP003",
                "Mapping failed.",
                MappingDiagnosticSeverity.Error));

        Assert.True(
            context.HasDiagnostics);

        Assert.True(
            context.HasErrors);
    }

    [Fact]
    public void HasErrors_WithInformationAndWarningDiagnostics_ReturnsFalse()
    {
        var context =
            CreateContext();

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

        Assert.True(
            context.HasDiagnostics);

        Assert.False(
            context.HasErrors);
    }

    [Fact]
    public void Diagnostics_CannotBeModifiedThroughContext()
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            new MappingDiagnostic(
                "MAP001",
                "Mapping warning.",
                MappingDiagnosticSeverity.Warning));

        var diagnostics =
            Assert.IsAssignableFrom<
                IList<MappingDiagnostic>>(
                context.Diagnostics);

        Assert.Throws<NotSupportedException>(
            () => diagnostics.Add(
                new MappingDiagnostic(
                    "MAP002",
                    "Another warning.",
                    MappingDiagnosticSeverity.Warning)));
    }

    [Fact]
    public void CaptureDiagnosticCheckpoint_WithEmptyContext_ReturnsZero()
    {
        var context =
            CreateContext();

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        Assert.Equal(
            0,
            checkpoint);
    }

    [Fact]
    public void CaptureDiagnosticCheckpoint_ReturnsCurrentDiagnosticCount()
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "MAP001",
                MappingDiagnosticSeverity.Information));

        context.AddDiagnostic(
            CreateDiagnostic(
                "MAP002",
                MappingDiagnosticSeverity.Warning));

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        Assert.Equal(
            2,
            checkpoint);
    }

    [Fact]
    public void GetDiagnosticsSince_WithNoNewDiagnostics_ReturnsEmptyCollection()
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "MAP001",
                MappingDiagnosticSeverity.Warning));

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        IReadOnlyList<MappingDiagnostic> diagnostics =
            context.GetDiagnosticsSince(
                checkpoint);

        Assert.Empty(
            diagnostics);
    }

    [Fact]
    public void GetDiagnosticsSince_ReturnsOnlyDiagnosticsAddedAfterCheckpoint()
    {
        var context =
            CreateContext();

        MappingDiagnostic previousDiagnostic =
            CreateDiagnostic(
                "PREVIOUS",
                MappingDiagnosticSeverity.Warning);

        MappingDiagnostic firstOperationDiagnostic =
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Information);

        MappingDiagnostic secondOperationDiagnostic =
            CreateDiagnostic(
                "OP002",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            previousDiagnostic);

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        context.AddDiagnostic(
            firstOperationDiagnostic);

        context.AddDiagnostic(
            secondOperationDiagnostic);

        IReadOnlyList<MappingDiagnostic> diagnostics =
            context.GetDiagnosticsSince(
                checkpoint);

        Assert.Equal(
            2,
            diagnostics.Count);

        Assert.Same(
            firstOperationDiagnostic,
            diagnostics[0]);

        Assert.Same(
            secondOperationDiagnostic,
            diagnostics[1]);

        Assert.Equal(
            3,
            context.Diagnostics.Count);

        Assert.Same(
            previousDiagnostic,
            context.Diagnostics[0]);
    }

    [Fact]
    public void GetDiagnosticsSince_ReturnsDefensiveSnapshot()
    {
        var context =
            CreateContext();

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        MappingDiagnostic firstDiagnostic =
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            firstDiagnostic);

        IReadOnlyList<MappingDiagnostic> snapshot =
            context.GetDiagnosticsSince(
                checkpoint);

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP002",
                MappingDiagnosticSeverity.Warning));

        MappingDiagnostic actualDiagnostic =
            Assert.Single(
                snapshot);

        Assert.Same(
            firstDiagnostic,
            actualDiagnostic);

        Assert.Equal(
            2,
            context.Diagnostics.Count);
    }

    [Fact]
    public void GetDiagnosticsSince_ReturnedCollectionCannotBeModified()
    {
        var context =
            CreateContext();

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Warning));

        IReadOnlyList<MappingDiagnostic> operationDiagnostics =
            context.GetDiagnosticsSince(
                checkpoint);

        var diagnostics =
            Assert.IsAssignableFrom<
                IList<MappingDiagnostic>>(
                operationDiagnostics);

        Assert.Throws<NotSupportedException>(
            () => diagnostics.Add(
                CreateDiagnostic(
                    "OP002",
                    MappingDiagnosticSeverity.Warning)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void GetDiagnosticsSince_WithInvalidCheckpoint_ThrowsArgumentOutOfRangeException(
        int checkpoint)
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "MAP001",
                MappingDiagnosticSeverity.Warning));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => context.GetDiagnosticsSince(
                checkpoint));
    }

    [Fact]
    public void HasDiagnosticsSince_WithNoNewDiagnostics_ReturnsFalse()
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "PREVIOUS",
                MappingDiagnosticSeverity.Warning));

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        Assert.False(
            context.HasDiagnosticsSince(
                checkpoint));
    }

    [Fact]
    public void HasDiagnosticsSince_WithNewDiagnostic_ReturnsTrue()
    {
        var context =
            CreateContext();

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Information));

        Assert.True(
            context.HasDiagnosticsSince(
                checkpoint));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void HasDiagnosticsSince_WithInvalidCheckpoint_ThrowsArgumentOutOfRangeException(
        int checkpoint)
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "MAP001",
                MappingDiagnosticSeverity.Warning));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => context.HasDiagnosticsSince(
                checkpoint));
    }

    [Fact]
    public void HasErrorsSince_IgnoresErrorBeforeCheckpoint()
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "PREVIOUS",
                MappingDiagnosticSeverity.Error));

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Warning));

        Assert.False(
            context.HasErrorsSince(
                checkpoint));

        Assert.True(
            context.HasErrors);
    }

    [Fact]
    public void HasErrorsSince_WithErrorAfterCheckpoint_ReturnsTrue()
    {
        var context =
            CreateContext();

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Information));

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP002",
                MappingDiagnosticSeverity.Error));

        Assert.True(
            context.HasErrorsSince(
                checkpoint));
    }

    [Fact]
    public void HasErrorsSince_WithOnlyNonErrorDiagnostics_ReturnsFalse()
    {
        var context =
            CreateContext();

        int checkpoint =
            context.CaptureDiagnosticCheckpoint();

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP001",
                MappingDiagnosticSeverity.Information));

        context.AddDiagnostic(
            CreateDiagnostic(
                "OP002",
                MappingDiagnosticSeverity.Warning));

        Assert.False(
            context.HasErrorsSince(
                checkpoint));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void HasErrorsSince_WithInvalidCheckpoint_ThrowsArgumentOutOfRangeException(
        int checkpoint)
    {
        var context =
            CreateContext();

        context.AddDiagnostic(
            CreateDiagnostic(
                "MAP001",
                MappingDiagnosticSeverity.Warning));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => context.HasErrorsSince(
                checkpoint));
    }

    private static MappingDiagnostic CreateDiagnostic(
        string code,
        MappingDiagnosticSeverity severity)
    {
        return new MappingDiagnostic(
            code,
            $"Diagnostic '{code}'.",
            severity);
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