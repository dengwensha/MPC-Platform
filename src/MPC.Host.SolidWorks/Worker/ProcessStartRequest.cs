using System;
using System.Collections.Generic;
using System.Linq;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class ProcessStartRequest
{
    private readonly IReadOnlyList<string> _arguments;

    internal string ExecutablePath { get; }

    internal IReadOnlyList<string> Arguments =>
        _arguments;

    internal string WorkingDirectory { get; }

    internal ProcessStartRequest(
        string executablePath,
        IEnumerable<string> arguments,
        string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException(
                "Process executable path is required.",
                nameof(executablePath));
        }

        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        var argumentArray =
            arguments.ToArray();

        if (argumentArray.Any(argument => argument is null))
        {
            throw new ArgumentException(
                "Process arguments cannot contain null entries.",
                nameof(arguments));
        }

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ArgumentException(
                "Process working directory is required.",
                nameof(workingDirectory));
        }

        ExecutablePath = executablePath.Trim();
        _arguments = Array.AsReadOnly(argumentArray);
        WorkingDirectory = workingDirectory.Trim();
    }
}
