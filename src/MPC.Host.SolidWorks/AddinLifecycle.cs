using System;

namespace MPC.Host.SolidWorks;

internal sealed class AddinLifecycle
{
    private object? _application;

    internal bool IsConnected => _application is not null;

    internal int? AddinCookie { get; private set; }

    internal bool Connect(
        object? application,
        int addinCookie,
        Func<object, int, bool> registerCallback)
    {
        if (application is null ||
            addinCookie <= 0 ||
            registerCallback is null ||
            IsConnected)
        {
            return false;
        }

        bool callbackRegistered;

        try
        {
            callbackRegistered = registerCallback(application, addinCookie);
        }
        catch
        {
            return false;
        }

        if (!callbackRegistered)
        {
            return false;
        }

        _application = application;
        AddinCookie = addinCookie;

        return true;
    }

    internal bool Disconnect()
    {
        _application = null;
        AddinCookie = null;

        return true;
    }
}
