using MPC.Connectors.Snapshots.Source;

namespace MPC.Tests.Connectors;

public sealed class CadSourceInfoTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesSourceInfo()
    {
        var source = new CadSourceInfo(
            sourceSystem: "STEP",
            sourceFormat: "AP242",
            sourceFileName: "GoldenCylinder.step",
            sourceVersion: "Edition 3",
            connectorVersion: "1.0.0");

        Assert.Equal("STEP", source.SourceSystem);
        Assert.Equal("AP242", source.SourceFormat);
        Assert.Equal("GoldenCylinder.step", source.SourceFileName);
        Assert.Equal("Edition 3", source.SourceVersion);
        Assert.Equal("1.0.0", source.ConnectorVersion);
    }

    [Fact]
    public void Constructor_WithNullSourceVersion_AllowsMissingVersion()
    {
        var source = new CadSourceInfo(
            sourceSystem: "SolidWorks",
            sourceFormat: "SLDPRT",
            sourceFileName: "GoldenPart.SLDPRT",
            sourceVersion: null,
            connectorVersion: "1.0.0");

        Assert.Null(source.SourceVersion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSourceSystem_ThrowsArgumentException(
        string? sourceSystem)
    {
        Assert.Throws<ArgumentException>(
            () => new CadSourceInfo(
                sourceSystem: sourceSystem!,
                sourceFormat: "AP242",
                sourceFileName: "GoldenPart.step",
                sourceVersion: null,
                connectorVersion: "1.0.0"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSourceFileName_ThrowsArgumentException(
        string? sourceFileName)
    {
        Assert.Throws<ArgumentException>(
            () => new CadSourceInfo(
                sourceSystem: "STEP",
                sourceFormat: "AP242",
                sourceFileName: sourceFileName!,
                sourceVersion: null,
                connectorVersion: "1.0.0"));
    }
}