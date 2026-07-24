using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class ReferenceResolverContractTests
{
    [Fact]
    public void ResolverImplementation_CanResolveExpectedType()
    {
        var entity = new TestEntity("entity");

        IReferenceResolver resolver =
            new TestReferenceResolver(
                new Dictionary<string, object>
                {
                    ["#57"] = entity
                });

        var result = resolver.Resolve<TestEntity>("#57");

        Assert.True(result.IsResolved);
        Assert.Same(entity, result.Value);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void ResolverImplementation_WhenReferenceIsMissing_ReturnsFailure()
    {
        IReferenceResolver resolver =
            new TestReferenceResolver(
                new Dictionary<string, object>());

        var result = resolver.Resolve<TestEntity>("#57");

        Assert.False(result.IsResolved);
        Assert.Null(result.Value);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("MAP-REF-001", diagnostic.Code);
        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);
    }

    [Fact]
    public void ResolverImplementation_WhenTypeDoesNotMatch_ReturnsFailure()
    {
        IReferenceResolver resolver =
            new TestReferenceResolver(
                new Dictionary<string, object>
                {
                    ["#57"] = new DifferentEntity()
                });

        var result = resolver.Resolve<TestEntity>("#57");

        Assert.False(result.IsResolved);
        Assert.Null(result.Value);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("MAP-REF-002", diagnostic.Code);
        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);
    }

    private sealed class TestReferenceResolver :
        IReferenceResolver
    {
        private readonly IReadOnlyDictionary<string, object> _entities;

        public TestReferenceResolver(
            IReadOnlyDictionary<string, object> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            _entities = entities;
        }

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                throw new ArgumentException(
                    "Reference identifier cannot be null, empty, or whitespace.",
                    nameof(referenceId));
            }

            if (!_entities.TryGetValue(referenceId, out var entity))
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    new[]
                    {
                        new MappingDiagnostic(
                            "MAP-REF-001",
                            $"Referenced entity '{referenceId}' could not be found.",
                            MappingDiagnosticSeverity.Error)
                    });
            }

            if (entity is not T typedEntity)
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    new[]
                    {
                        new MappingDiagnostic(
                            "MAP-REF-002",
                            $"Referenced entity '{referenceId}' is not of the expected type '{typeof(T).Name}'.",
                            MappingDiagnosticSeverity.Error)
                    });
            }

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                typedEntity);
        }
    }

    private sealed class TestEntity
    {
        public TestEntity(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    private sealed class DifferentEntity
    {
    }
}