using System;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;

namespace MPC.Host.SolidWorks;

[ComVisible(true)]
[Guid(AddinGuid)]
[ProgId(ProgId)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class SolidWorksAddin : ISwAddin
{
    public const string AddinGuid = "9A54C5C5-5457-4B41-9147-184FF399B057";
    public const string ProgId = "MPC.Host.SolidWorks.Addin";

    private readonly AddinLifecycle _lifecycle = new();
    private ISldWorks? _solidWorks;

    public bool ConnectToSW(object ThisSW, int Cookie)
    {
        if (ThisSW is not ISldWorks solidWorks)
        {
            return false;
        }

        bool connected = _lifecycle.Connect(
            solidWorks,
            Cookie,
            (application, addinCookie) =>
                ((ISldWorks)application).SetAddinCallbackInfo2(
                    0,
                    this,
                    addinCookie));

        if (!connected)
        {
            return false;
        }

        _solidWorks = solidWorks;

        return true;
    }

    public bool DisconnectFromSW()
    {
        bool disconnected = _lifecycle.Disconnect();

        _solidWorks = null;

        return disconnected;
    }
}
