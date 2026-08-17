using System;
using Xunit;

namespace MPC.Host.SolidWorks.Tests;

public sealed class AddinLifecycleTests
{
    [Fact]
    public void Connect_WithValidApplicationAndCookie_Connects()
    {
        var lifecycle = new AddinLifecycle();
        var application = new object();

        bool result = lifecycle.Connect(
            application,
            42,
            (registeredApplication, registeredCookie) =>
                ReferenceEquals(application, registeredApplication) &&
                registeredCookie == 42);

        Assert.True(result);
        Assert.True(lifecycle.IsConnected);
        Assert.Equal(42, lifecycle.AddinCookie);
    }

    [Fact]
    public void Connect_WithNullApplication_IsRejected()
    {
        var lifecycle = new AddinLifecycle();

        bool result = lifecycle.Connect(
            null,
            42,
            (_, _) => true);

        Assert.False(result);
        Assert.False(lifecycle.IsConnected);
        Assert.Null(lifecycle.AddinCookie);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Connect_WithInvalidCookie_IsRejected(int addinCookie)
    {
        var lifecycle = new AddinLifecycle();

        bool result = lifecycle.Connect(
            new object(),
            addinCookie,
            (_, _) => true);

        Assert.False(result);
        Assert.False(lifecycle.IsConnected);
        Assert.Null(lifecycle.AddinCookie);
    }

    [Fact]
    public void Connect_WhenCallbackRegistrationFails_RemainsDisconnected()
    {
        var lifecycle = new AddinLifecycle();

        bool result = lifecycle.Connect(
            new object(),
            42,
            (_, _) => false);

        Assert.False(result);
        Assert.False(lifecycle.IsConnected);
        Assert.Null(lifecycle.AddinCookie);
    }

    [Fact]
    public void Connect_WhenCallbackRegistrationThrows_RemainsDisconnected()
    {
        var lifecycle = new AddinLifecycle();

        bool result = lifecycle.Connect(
            new object(),
            42,
            (_, _) => throw new InvalidOperationException());

        Assert.False(result);
        Assert.False(lifecycle.IsConnected);
        Assert.Null(lifecycle.AddinCookie);
    }

    [Fact]
    public void Connect_WhenAlreadyConnected_IsRejected()
    {
        var lifecycle = new AddinLifecycle();

        Assert.True(
            lifecycle.Connect(
                new object(),
                42,
                (_, _) => true));

        bool secondResult = lifecycle.Connect(
            new object(),
            84,
            (_, _) => true);

        Assert.False(secondResult);
        Assert.True(lifecycle.IsConnected);
        Assert.Equal(42, lifecycle.AddinCookie);
    }

    [Fact]
    public void Disconnect_AfterConnect_ClearsConnectionState()
    {
        var lifecycle = new AddinLifecycle();

        Assert.True(
            lifecycle.Connect(
                new object(),
                42,
                (_, _) => true));

        bool result = lifecycle.Disconnect();

        Assert.True(result);
        Assert.False(lifecycle.IsConnected);
        Assert.Null(lifecycle.AddinCookie);
    }

    [Fact]
    public void Disconnect_WhenAlreadyDisconnected_IsIdempotent()
    {
        var lifecycle = new AddinLifecycle();

        Assert.True(lifecycle.Disconnect());
        Assert.True(lifecycle.Disconnect());
        Assert.False(lifecycle.IsConnected);
        Assert.Null(lifecycle.AddinCookie);
    }
}
