using System.Text.Json;

namespace MPC.Worker;

/// <summary>Best-effort local evidence only; never a source of operation results.</summary>
internal sealed class WorkerFailureLog
{
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
            string details = exception.ToString();
            string json = JsonSerializer.Serialize(new
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                RequestId = Limit(requestId, 64),
                ExceptionType = Limit(exception.GetType().FullName ?? exception.GetType().Name, 256),
                DetailsTruncated = details.Length > DetailLimit,
                ExceptionDetails = Limit(details, DetailLimit)
            });
            // Only a completed record replaces the previous failure. Concurrent
            // Workers use separate staging files; the last successful move wins.
            temporary = Path.Combine(directory, "worker-" + Guid.NewGuid().ToString("N") + ".tmp");
            File.WriteAllText(temporary, json);
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

    private static string Limit(string value, int length) =>
        value.Length <= length ? value : value[..length];
}
