using System;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace MPC.Host.SolidWorks.Commands;

internal sealed class SolidWorksStepImportCommandRegistration
{
    internal const int CommandGroupId = 1001;
    internal const int ImportStepCommandId = 10001;

    private CommandManager? _commandManager;
    private CommandGroup? _commandGroup;

    internal bool IsRegistered =>
        _commandManager is not null &&
        _commandGroup is not null;

    internal string? LastFailureMessage
    {
        get;
        private set;
    }

    internal bool Register(
        ISldWorks application,
        int addinCookie)
    {
        LastFailureMessage = null;

        if (application is null)
        {
            throw new ArgumentNullException(
                nameof(application));
        }

        if (addinCookie <= 0)
        {
            LastFailureMessage =
                "The add-in cookie is invalid.";

            return false;
        }

        if (IsRegistered)
        {
            LastFailureMessage =
                "The MPC command group is already registered.";

            return false;
        }

        try
        {
            CommandManager commandManager =
                application.GetCommandManager(
                    addinCookie);

            if (commandManager is null)
            {
                LastFailureMessage =
                    "SOLIDWORKS did not return a CommandManager.";

                return false;
            }

            int creationErrors = 0;

            CommandGroup commandGroup =
                commandManager.CreateCommandGroup2(
                    CommandGroupId,
                    "MPC",
                    "MPC manufacturing intelligence commands",
                    "MPC",
                    -1,
                    true,
                    ref creationErrors);

            if (commandGroup is null)
            {
                LastFailureMessage =
                    $"SOLIDWORKS did not create the MPC command group. " +
                    $"Creation error: {creationErrors}.";

                return false;
            }

            int commandIndex =
                commandGroup.AddCommandItem2(
                    "Import STEP with MPC",
                    -1,
                    "Import a STEP file through the MPC canonical pipeline",
                    "MPC STEP Import",
                    -1,
                    nameof(SolidWorksAddin.OnImportStep),
                    nameof(SolidWorksAddin.CanImportStep),
                    ImportStepCommandId,
                    (int)swCommandItemType_e.swMenuItem |
                    (int)swCommandItemType_e.swToolbarItem);

            commandGroup.HasMenu = true;
            commandGroup.HasToolbar = true;

            bool activated =
                commandGroup.Activate();

            if (commandIndex < 0 ||
                !activated)
            {
                LastFailureMessage =
                    $"MPC command group activation failed. " +
                    $"Creation error: {creationErrors}; " +
                    $"command index: {commandIndex}; " +
                    $"activated: {activated}.";

                commandManager.RemoveCommandGroup2(
                    CommandGroupId,
                    true);

                return false;
            }

            _commandManager = commandManager;
            _commandGroup = commandGroup;

            return true;
        }
        catch (Exception exception)
            when (exception is COMException
                  || exception is InvalidCastException
                  || exception is ArgumentException)
        {
            LastFailureMessage =
                $"Command registration raised " +
                $"{exception.GetType().Name}: " +
                $"{exception.Message}";

            _commandManager = null;
            _commandGroup = null;

            return false;
        }
    }

    internal bool Unregister()
    {
        LastFailureMessage = null;

        if (_commandManager is null)
        {
            _commandGroup = null;
            return true;
        }

        bool removed;

        try
        {
            int result =
                _commandManager.RemoveCommandGroup2(
                    CommandGroupId,
                    true);

            removed =
                result >= 0;

            if (!removed)
            {
                LastFailureMessage =
                    $"SOLIDWORKS returned command removal result " +
                    $"'{result}'.";
            }
        }
        catch (COMException exception)
        {
            LastFailureMessage =
                $"Command removal raised COMException: " +
                $"{exception.Message}";

            removed = false;
        }

        _commandGroup = null;
        _commandManager = null;

        return removed;
    }
}
