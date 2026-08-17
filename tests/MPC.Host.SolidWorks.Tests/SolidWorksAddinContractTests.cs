using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace MPC.Host.SolidWorks.Tests;

public sealed class SolidWorksAddinContractTests
{
    private static readonly Type AddinType = typeof(SolidWorksAddin);

    [Fact]
    public void Addin_ImplementsRealISwAddinContract()
    {
        Assert.Contains(
            AddinType.GetInterfaces(),
            interfaceType =>
                interfaceType.FullName ==
                "SolidWorks.Interop.swpublished.ISwAddin");
    }

    [Fact]
    public void Addin_IsComVisible()
    {
        ComVisibleAttribute attribute =
            Assert.IsType<ComVisibleAttribute>(
                AddinType
                    .GetCustomAttributes(
                        typeof(ComVisibleAttribute),
                        inherit: false)
                    .Single());

        Assert.True(attribute.Value);
    }

    [Fact]
    public void Addin_HasStableExplicitGuid()
    {
        GuidAttribute attribute =
            Assert.IsType<GuidAttribute>(
                AddinType
                    .GetCustomAttributes(
                        typeof(GuidAttribute),
                        inherit: false)
                    .Single());

        Assert.Equal(
            "9A54C5C5-5457-4B41-9147-184FF399B057",
            attribute.Value,
            ignoreCase: true);

        Assert.Equal(
            SolidWorksAddin.AddinGuid,
            attribute.Value);
    }

    [Fact]
    public void Addin_HasStableExplicitProgId()
    {
        ProgIdAttribute attribute =
            Assert.IsType<ProgIdAttribute>(
                AddinType
                    .GetCustomAttributes(
                        typeof(ProgIdAttribute),
                        inherit: false)
                    .Single());

        Assert.Equal(
            "MPC.Host.SolidWorks.Addin",
            attribute.Value);

        Assert.Equal(
            SolidWorksAddin.ProgId,
            attribute.Value);
    }

    [Fact]
    public void Addin_DisablesGeneratedClassInterface()
    {
        ClassInterfaceAttribute attribute =
            Assert.IsType<ClassInterfaceAttribute>(
                AddinType
                    .GetCustomAttributes(
                        typeof(ClassInterfaceAttribute),
                        inherit: false)
                    .Single());

        Assert.Equal(
            ClassInterfaceType.None,
            attribute.Value);
    }

    [Fact]
    public void ConnectToSW_WithNonSolidWorksObject_IsRejected()
    {
        var addin = new SolidWorksAddin();

        bool result = addin.ConnectToSW(
            new object(),
            42);

        Assert.False(result);
    }

    [Fact]
    public void DisconnectFromSW_WithoutConnection_IsIdempotent()
    {
        var addin = new SolidWorksAddin();

        Assert.True(addin.DisconnectFromSW());
        Assert.True(addin.DisconnectFromSW());
    }
}
