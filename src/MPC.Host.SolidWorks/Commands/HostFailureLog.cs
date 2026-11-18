using System;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin,
            UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
    };
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
            TryCleanStaging(directory);
            string details = evidence.Exception?.ToString() ?? result.FailureMessage ?? "";
            string json = JsonSerializer.Serialize(new
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                RequestId = Limit(requestId, 64),
                HostStatus = result.Status.ToString(),
                WorkerExitCode = evidence.ExitCode,
                // Received fields are untrusted diagnostic data, never approval inputs.
                ReceivedRequestId = result.WorkerResult is null ? null : Limit(result.WorkerResult.RequestId ?? "", 64),
                ReceivedContractVersion = result.WorkerResult is null ? null : Limit(result.WorkerResult.ContractVersion ?? "", 32),
                ReceivedOperation = result.WorkerResult is null ? (int?)null : (int)result.WorkerResult.Operation,
                ReceivedOutcome = result.WorkerResult is null ? (int?)null : (int)result.WorkerResult.Outcome,
                FailureType = Limit(evidence.Exception?.GetType().FullName ?? evidence.FailureType ?? "", 256),
                FailureMessage = Limit(result.FailureMessage ?? "", 1024),
                DetailsTruncated = details.Length > 8192,
                ExceptionDetails = Limit(details, 8192)
            }, JsonOptions);
            temporary = Path.Combine(directory, "host-" + Guid.NewGuid().ToString("N") + ".tmp");
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(json);
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

    private static void TryCleanStaging(string directory)
    {
        try
        {
            DateTime cutoff = DateTime.UtcNow.AddDays(-1);
            int examined = 0;
            foreach (string path in Directory.EnumerateFiles(directory, "host-*.tmp"))
            {
                if (++examined > 64) break; // Bounded best-effort work per failure.
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Length != 37 ||
                    !Guid.TryParseExact(name.Substring(5), "N", out _)) continue;
                try
                {
                    if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 ||
                        File.GetLastWriteTimeUtc(path) >= cutoff) continue;
                    // Windows sharing rules exclude any active staging writer.
                    // Keep the claim open while deleting to avoid a check/delete race.
                    using (var claim = new FileStream(path, FileMode.Open,
                        FileAccess.ReadWrite, FileShare.Delete))
                    {
                        if (File.GetLastWriteTimeUtc(path) < cutoff) File.Delete(path);
                    }
                }
                catch (Exception) { /* Busy/inaccessible candidates do not stop logging. */ }
            }
        }
        catch (Exception) { /* Cleanup is optional, independently of record creation. */ }
    }

    private static string Limit(string value, int length)
    {
        if (value.Length <= length) return value;
        if (char.IsHighSurrogate(value[length - 1]) && char.IsLowSurrogate(value[length]))
            length--;
        return value.Substring(0, length);
    }
}
