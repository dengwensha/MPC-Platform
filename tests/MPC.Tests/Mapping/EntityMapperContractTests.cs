using MPC.Mapping;

namespace MPC.Tests.Mapping;

public sealed class EntityMapperContractTests
{
    [Fact]
    public void MapperImplementation_CanMapSourceToTarget()
    {
        IEntityMapper<TestSource, TestTarget> mapper =
            new TestMapper();

        var source = new TestSource("source");
        var context = new MappingContext();

        var result = mapper.Map(source, context);

        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Value);
        Assert.Equal("source", result.Value.Name);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void MapperImplementation_WithNullSource_ThrowsArgumentNullException()
    {
        IEntityMapper<TestSource, TestTarget> mapper =
            new TestMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                new MappingContext()));
    }

    [Fact]
    public void MapperImplementation_WithNullContext_ThrowsArgumentNullException()
    {
        IEntityMapper<TestSource, TestTarget> mapper =
            new TestMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                new TestSource("source"),
                null!));
    }

    private sealed class TestMapper :
        IEntityMapper<TestSource, TestTarget>
    {
        public MappingResult<TestTarget> Map(
            TestSource source,
            MappingContext context)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(context);

            return MappingResult<TestTarget>.Success(
                new TestTarget(source.Name));
        }
    }

    private sealed class TestSource
    {
        public TestSource(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    private sealed class TestTarget
    {
        public TestTarget(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}