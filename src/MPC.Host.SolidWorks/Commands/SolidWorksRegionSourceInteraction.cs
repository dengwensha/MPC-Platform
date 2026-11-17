using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MPC.Host.SolidWorks.Commands;

internal sealed class SolidWorksRegionSourceInteraction : IRegionSourceValidationInteraction
{
    private readonly ISldWorks _application;
    internal SolidWorksRegionSourceInteraction(ISldWorks application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
    }

    public string? ChooseStepFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Kaynak yüzü kontrol edilecek STEP dosyasını seçin",
            Filter = "STEP Files (*.step;*.stp)|*.step;*.stp",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            RestoreDirectory = true
        };
        using var process = Process.GetCurrentProcess();
        var owner = new HostWindow(process.MainWindowHandle);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
    }

    private sealed class HostWindow : IWin32Window
    {
        internal HostWindow(IntPtr handle) => Handle = handle;
        public IntPtr Handle { get; }
    }

    public string? ChooseSourceFace(string filePath, string fingerprint)
    {
        string value = Interaction.InputBox(
            "Dosya: " + Path.GetFileName(filePath) + System.Environment.NewLine +
            "STEP yüz kimliğini girin (örnek: #8)." + System.Environment.NewLine +
            "SOLIDWORKS'te seçili yüz kullanılmaz." + System.Environment.NewLine +
            "SHA-256: " + fingerprint,
            "MPC kaynak yüz kontrolü", string.Empty);
        return string.IsNullOrEmpty(value) ? null : value;
    }

    public void ShowResult(string message, bool isError) =>
        _application.SendMsgToUser2(message,
            (int)(isError ? swMessageBoxIcon_e.swMbStop : swMessageBoxIcon_e.swMbInformation),
            (int)swMessageBoxBtn_e.swMbOk);
}
