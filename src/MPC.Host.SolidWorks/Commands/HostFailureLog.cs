using System;
using System.IO;
using System.Text.Json;

namespace MPC.Host.SolidWorks.Commands;

internal sealed class HostFailureEvidence
{
    internal Exception? Exception;
    internal string? FailureType;
    internal int? ExitCode;
}

// Optional local evidence, never an input to source-link approval.
internal sealed class HostFailureLog
{
    private readonly Func<string> _directory;

    internal HostFailureLog() : this(() =>
    {
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local))
            throw new IOException("Local application data is unavailable.");
        return Path.Combine(local, "MPC", "Diagnostics");
    }) { }

    internal HostFailureLog(string directory) : this(() => directory) { }

    private HostFailureLog(Func<string> directory) { _directory = directory; }

    internal void TryWrite(string requestId, RegionSourceValidationHostCommandResult result,
        HostFailureEvidence evidence)
    {
        string? temporary = null;
        try
        {
            string directory = _directory();
            Directory.CreateDirectory(directory);
            string details = evidence.Exception?.ToString() ?? result.FailureMessage ?? "";
            string json = JsonSerializer.Serialize(new
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                RequestId = Limit(requestId, 64),
                HostStatus = result.Status.ToString(),
                WorkerExitCode = evidence.ExitCode,
                FailureType = Limit(evidence.Exception?.GetType().FullName ?? evidence.FailureType ?? "", 256),
                FailureMessage = Limit(result.FailureMessage ?? "", 1024),
                DetailsTruncated = details.Length > 8192,
                ExceptionDetails = Limit(details, 8192)
            });
            temporary = Path.Combine(directory, "host-" + Guid.NewGuid().ToString("N") + ".tmp");
            File.WriteAllText(temporary, json);
            string destination = Path.Combine(directory, "host-last-failure.json");
            // net48 has no overwrite overload for File.Move. A first-write race
            // may drop one optional record; an existing record is replaced atomically.
            if (File.Exists(destination)) File.Replace(temporary, destination, null);
            else File.Move(temporary, destination);
        }
        catch (Exception)
        {
            // Diagnostic-only failures must never replace the Host command result.
        }
        finally
        {
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (Exception) { /* Optional cleanup must not affect the command. */ }
            }
        }
    }

    private static string Limit(string value, int length)
    {
        if (value.Length <= length) return value;
        if (char.IsHighSurrogate(value[length - 1]) && char.IsLowSurrogate(value[length]))
            length--;
        return value.Substring(0, length);
    }
}
