using System;
using System.Runtime.InteropServices;

namespace MPC.Host.SolidWorks;

[ComVisible(true)]
[Guid("A359A421-D86F-4727-82C4-3CFA9A10277B")]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
public sealed class SolidWorksAddinCallbackProxy
{
    private readonly SolidWorksAddin
        _addin;

    internal SolidWorksAddinCallbackProxy(
        SolidWorksAddin addin)
    {
        _addin =
            addin
            ?? throw new ArgumentNullException(
                nameof(addin));
    }

    public void OnImportStep()
    {
        _addin.OnImportStep();
    }

    public int CanImportStep()
    {
        return _addin.CanImportStep();
    }
}
