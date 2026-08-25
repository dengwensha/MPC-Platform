using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SystemEnvironment = System.Environment;
using MPC.Contracts;
using MPC.Host.SolidWorks.Commands;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace MPC.Host.SolidWorks;

[ComVisible(true)]
[Guid(AddinGuid)]
[ProgId(ProgId)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class SolidWorksAddin :
    ISwAddin
{
    static SolidWorksAddin()
    {
        HostAssemblyResolver.Install();
    }

    public const string AddinGuid =
        "9A54C5C5-5457-4B41-9147-184FF399B057";

    public const string ProgId =
        "MPC.Host.SolidWorks.Addin";

    private const int DefaultTimeoutMilliseconds =
        30000;

    private readonly AddinLifecycle
        _lifecycle = new AddinLifecycle();

    private readonly SolidWorksStepImportCommandRegistration
        _commandRegistration =
            new SolidWorksStepImportCommandRegistration();

    private readonly StepImportHostCommand
        _stepImportCommand =
            new StepImportHostCommand();

    private ISldWorks? _solidWorks;

    public bool ConnectToSW(
        object ThisSW,
        int Cookie)
    {
        if (ThisSW is not ISldWorks solidWorks)
        {
            return false;
        }

        var callbackProxy =
            new SolidWorksAddinCallbackProxy(
                this);

        bool connected =
            _lifecycle.Connect(
                solidWorks,
                Cookie,
                (_, addinCookie) =>
                {
                    solidWorks.SetAddinCallbackInfo2(
                        0,
                        callbackProxy,
                        addinCookie);

                    return true;
                });

        if (!connected)
        {
            return false;
        }

        _solidWorks = solidWorks;

        bool commandRegistered;

        try
        {
            commandRegistered =
                _commandRegistration.Register(
                    solidWorks,
                    Cookie);
        }
        catch
        {
            _solidWorks = null;
            _lifecycle.Disconnect();

            return false;
        }

        if (!commandRegistered)
        {
            solidWorks.SendMsgToUser(
                "MPC command registration failed: " +
                (_commandRegistration.LastFailureMessage
                 ?? "No failure details were returned."));

            _solidWorks = null;
            _lifecycle.Disconnect();

            return false;
        }

        return true;
    }

    public bool DisconnectFromSW()
    {
        bool commandRemoved =
            _commandRegistration.Unregister();

        bool disconnected =
            _lifecycle.Disconnect();

        _solidWorks = null;

        return commandRemoved &&
               disconnected;
    }

    public int CanImportStep()
    {
        return _solidWorks is null
            ? 0
            : 1;
    }

    public void OnImportStep()
    {
        ISldWorks? solidWorks =
            _solidWorks;

        if (solidWorks is null)
        {
            return;
        }

        try
        {
            int openOptions;
            string configurationName;
            string displayName;
            string displayStateName;

            string stepFilePath =
                solidWorks.GetOpenFileName2(
                    "Select STEP file for MPC import",
                    string.Empty,
                    "STEP Files (*.step;*.stp)|*.step;*.stp|",
                    out openOptions,
                    out configurationName,
                    out displayName,
                    out displayStateName);

            if (string.IsNullOrWhiteSpace(
                    stepFilePath))
            {
                return;
            }

            string assemblyDirectory =
                Path.GetDirectoryName(
                    typeof(SolidWorksAddin)
                        .Assembly
                        .Location)
                ?? string.Empty;

            string workerExecutablePath =
                Path.Combine(
                    assemblyDirectory,
                    "MPC.Worker.exe");

            StepImportHostCommandResult result =
                _stepImportCommand.Execute(
                    new StepImportHostCommandRequest(
                        workerExecutablePath,
                        stepFilePath,
                        DefaultTimeoutMilliseconds));

            if (result.IsSuccessful &&
                result.WorkerResult?.Summary is
                    WorkerOperationSummary summary)
            {
                solidWorks.SendMsgToUser2(
                    CreateSuccessMessage(summary),
                    (int)swMessageBoxIcon_e.swMbInformation,
                    (int)swMessageBoxBtn_e.swMbOk);

                return;
            }

            solidWorks.SendMsgToUser2(
                CreateFailureMessage(result),
                (int)swMessageBoxIcon_e.swMbStop,
                (int)swMessageBoxBtn_e.swMbOk);
        }
        catch (Exception exception)
        {
            try
            {
                solidWorks.SendMsgToUser2(
                    "MPC STEP import failed." +
                    SystemEnvironment.NewLine +
                    $"{exception.GetType().Name}: " +
                    $"{exception.Message}",
                    (int)swMessageBoxIcon_e.swMbStop,
                    (int)swMessageBoxBtn_e.swMbOk);
            }
            catch
            {
            }
        }
    }

    private static string CreateSuccessMessage(
        WorkerOperationSummary summary)
    {
        return
            "MPC STEP import succeeded." +
            SystemEnvironment.NewLine +
            $"Solid: {summary.SolidCount}" +
            SystemEnvironment.NewLine +
            $"Shells: " +
            $"{summary.OuterShellCount + summary.InnerShellCount}" +
            SystemEnvironment.NewLine +
            $"Faces: {summary.FaceCount}" +
            SystemEnvironment.NewLine +
            $"Loops: {summary.LoopCount}" +
            SystemEnvironment.NewLine +
            $"Edges: {summary.EdgeCount}" +
            SystemEnvironment.NewLine +
            $"Vertices: {summary.VertexCount}";
    }

    private static string CreateFailureMessage(
        StepImportHostCommandResult result)
    {
        if (result.WorkerResult is not null &&
            result.WorkerResult.Diagnostics.Count > 0)
        {
            return
                "MPC STEP import failed." +
                SystemEnvironment.NewLine +
                string.Join(
                    SystemEnvironment.NewLine,
                    result.WorkerResult.Diagnostics
                        .Select(
                            diagnostic =>
                                $"{diagnostic.Code}: " +
                                $"{diagnostic.Message}"));
        }

        return
            "MPC STEP import failed." +
            SystemEnvironment.NewLine +
            (result.FailureMessage
             ?? "No failure details were returned.");
    }
}
