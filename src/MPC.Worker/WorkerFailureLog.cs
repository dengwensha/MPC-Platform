using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace MPC.Worker;

/// <summary>Best-effort local evidence only; never a source of operation results.</summary>
internal sealed class WorkerFailureLog
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin,
            UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
    };
    private const int DetailLimit = 8192;
    private readonly Func<string> _directory;

    internal WorkerFailureLog() : this(() =>
    {
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local))
            throw new IOException("Local application data is unavailable.");
        return Path.Combine(local, "MPC", "Diagnostics");
    }) { }

    internal WorkerFailureLog(string directory) : this(() => directory) { }

    private WorkerFailureLog(Func<string> directory) => _directory = directory;

    internal void TryWrite(string requestId, Exception exception)
    {
        string? temporary = null;
        try
        {
            string directory = _directory();
            Directory.CreateDirectory(directory);
            TryCleanStaging(directory);
            string details = exception.ToString();
            string json = JsonSerializer.Serialize(new
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                RequestId = Limit(requestId, 64),
                ExceptionType = Limit(exception.GetType().FullName ?? exception.GetType().Name, 256),
                DetailsTruncated = details.Length > DetailLimit,
                ExceptionDetails = Limit(details, DetailLimit)
            }, JsonOptions);
            // Only a completed record replaces the previous failure. Concurrent
            // Workers use separate staging files; the last successful move wins.
            temporary = Path.Combine(directory, "worker-" + Guid.NewGuid().ToString("N") + ".tmp");
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(json);
            File.Move(temporary, Path.Combine(directory, "worker-last-failure.json"), overwrite: true);
        }
        catch (Exception)
        {
            // Deliberately confined to optional diagnostics. Never reinterpret
            // the import exception or let logging replace the existing result.
        }
        finally
        {
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (Exception) { /* Optional staging cleanup must not affect the result. */ }
            }
        }
    }

    private static void TryCleanStaging(string directory)
    {
        try
        {
            DateTime cutoff = DateTime.UtcNow.AddDays(-1);
            int examined = 0;
            foreach (string path in Directory.EnumerateFiles(directory, "worker-*.tmp"))
            {
                if (++examined > 64) break; // Bounded best-effort work per failure.
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Length != 39 ||
                    !Guid.TryParseExact(name.Substring(7), "N", out _)) continue;
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
