using MPC.Connectors.Step.Models;

namespace MPC.Tests.Connectors;

public sealed class StepHeaderInfoTests
{
    [Fact]
    public void Constructor_WithValidHeader_CreatesHeaderInfo()
    {
        var header = new StepHeaderInfo(
            descriptions: new[]
            {
                "ISO 10303-21"
            },
            implementationLevel: "2;1",
            fileName: "GoldenPart.step",
            timeStamp: "2026-07-14T22:00:00",
            author: "MPC",
            organization: "MPC Platform",
            preprocessorVersion: "Test Writer",
            originatingSystem: "STEP",
            authorization: null,
            schemas: new[]
            {
                "AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF"
            });

        Assert.Single(header.Descriptions);
        Assert.Equal("ISO 10303-21", header.Descriptions[0]);
        Assert.Equal("2;1", header.ImplementationLevel);
        Assert.Equal("GoldenPart.step", header.FileName);
        Assert.Single(header.Schemas);
    }

    [Fact]
    public void Constructor_CopiesCollections()
    {
        var descriptions = new List<string>
        {
            "ISO 10303-21"
        };

        var schemas = new List<string>
        {
            "AUTOMOTIVE_DESIGN"
        };

        var header = new StepHeaderInfo(
            descriptions,
            implementationLevel: null,
            fileName: null,
            timeStamp: null,
            author: null,
            organization: null,
            preprocessorVersion: null,
            originatingSystem: null,
            authorization: null,
            schemas);

        descriptions.Clear();
        schemas.Clear();

        Assert.Single(header.Descriptions);
        Assert.Single(header.Schemas);
    }

    [Fact]
    public void Constructor_WithEmptyDescriptions_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new StepHeaderInfo(
                descriptions: Array.Empty<string>(),
                implementationLevel: null,
                fileName: null,
                timeStamp: null,
                author: null,
                organization: null,
                preprocessorVersion: null,
                originatingSystem: null,
                authorization: null,
                schemas: new[]
                {
                    "AUTOMOTIVE_DESIGN"
                }));
    }

    [Fact]
    public void Constructor_WithEmptySchemas_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new StepHeaderInfo(
                descriptions: new[]
                {
                    "ISO 10303-21"
                },
                implementationLevel: null,
                fileName: null,
                timeStamp: null,
                author: null,
                organization: null,
                preprocessorVersion: null,
                originatingSystem: null,
                authorization: null,
                schemas: Array.Empty<string>()));
    }
}