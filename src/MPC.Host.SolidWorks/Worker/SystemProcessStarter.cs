using System;
using System.Diagnostics;
using System.Text;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class SystemProcessStarter : IProcessStarter
{
    public WorkerProcessHandle Start(ProcessStartRequest request)
    {
        var startInfo = CreateStartInfo(request);
        var process = Process.Start(startInfo);

        if (process is null)
        {
            throw new InvalidOperationException(
                "The operating system did not return a worker process.");
        }

        IProcessExecution? execution = null;

        try
        {
            execution = new SystemProcessExecution(process);
            return new WorkerProcessHandle(execution);
        }
        catch
        {
            if (execution is not null)
            {
                execution.Dispose();
            }
            else
            {
                process.Dispose();
            }

            throw;
        }
    }

    internal static ProcessStartInfo CreateStartInfo(ProcessStartRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            Arguments = BuildArguments(request),
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    internal static string BuildArguments(ProcessStartRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var builder = new StringBuilder();

        for (var index = 0; index < request.Arguments.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            AppendQuotedArgument(builder, request.Arguments[index]);
        }

        return builder.ToString();
    }

    private static void AppendQuotedArgument(
        StringBuilder builder,
        string argument)
    {
        builder.Append('"');
        var backslashCount = 0;

        foreach (var character in argument)
        {
            if (character == '\\')
            {
                backslashCount++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', (backslashCount * 2) + 1);
                builder.Append('"');
                backslashCount = 0;
                continue;
            }

            if (backslashCount > 0)
            {
                builder.Append('\\', backslashCount);
                backslashCount = 0;
            }

            builder.Append(character);
        }

        if (backslashCount > 0)
        {
            builder.Append('\\', backslashCount * 2);
        }

        builder.Append('"');
    }
}
