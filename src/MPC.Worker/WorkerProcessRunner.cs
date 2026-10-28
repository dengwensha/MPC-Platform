using System.Text.Json;
using MPC.Contracts;

namespace MPC.Worker;

public sealed class WorkerProcessRunner
{
    public const string RequestReadFailedCode =
        "WORKER-REQUEST-001";

    private const string Usage =
        "Usage: MPC.Worker <request-json-path> <result-json-path>";

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true
        };

    private readonly WorkerRequestProcessor _processor;

    public WorkerProcessRunner()
        : this(new WorkerRequestProcessor())
    {
    }

    public WorkerProcessRunner(
        WorkerRequestProcessor processor)
    {
        _processor = processor
            ?? throw new ArgumentNullException(nameof(processor));
    }

    public async Task<int> RunAsync(
        string[]? args,
        TextWriter error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (args is null
            || args.Length != 2
            || string.IsNullOrWhiteSpace(args[0])
            || string.IsNullOrWhiteSpace(args[1]))
        {
            await error.WriteLineAsync(Usage);

            return WorkerExitCodes.InvalidInvocation;
        }

        var requestPath = args[0];
        var resultPath = args[1];

        WorkerOperationRequest? request;

        try
        {
            var requestJson =
                await File.ReadAllTextAsync(
                    requestPath,
                    cancellationToken);

            using var document = JsonDocument.Parse(requestJson);
            if (IsRegionSourceRequest(document.RootElement))
            {
                return await RunRegionSourceAsync(document.RootElement,
                    resultPath, error, cancellationToken);
            }

            request =
                JsonSerializer.Deserialize<WorkerOperationRequest>(
                    requestJson,
                    JsonOptions);
        }
        catch (Exception exception)
            when (exception is IOException
                  or UnauthorizedAccessException
                  or JsonException
                  or NotSupportedException)
        {
            var failureResult =
                CreateRequestReadFailure(exception);

            var written =
                await TryWriteResultAsync(
                    resultPath,
                    failureResult,
                    error,
                    cancellationToken);

            return written
                ? WorkerExitCodes.RequestReadFailed
                : WorkerExitCodes.ResultWriteFailed;
        }

        var result =
            _processor.Process(request);

        var resultWritten =
            await TryWriteResultAsync(
                resultPath,
                result,
                error,
                cancellationToken);

        if (!resultWritten)
        {
            return WorkerExitCodes.ResultWriteFailed;
        }

        return result.Status switch
        {
            WorkerOperationStatus.Succeeded =>
                WorkerExitCodes.Succeeded,

            WorkerOperationStatus.Rejected =>
                WorkerExitCodes.RequestRejected,

            WorkerOperationStatus.Failed =>
                WorkerExitCodes.OperationFailed,

            _ =>
                WorkerExitCodes.OperationFailed
        };
    }

    // Route by operation/version or region-specific fields; never downgrade an
    // incomplete/misversioned region request into the legacy import operation.
    private static bool IsRegionSourceRequest(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object) return false;
        return (root.TryGetProperty("ContractVersion", out var version) &&
                version.ValueKind == JsonValueKind.String &&
                version.GetString() == ContractVersion.RegionSourceValidation) ||
            (root.TryGetProperty("Operation", out var operation) &&
                operation.ValueKind == JsonValueKind.Number &&
                operation.TryGetInt32(out int value) &&
                value == (int)WorkerOperationKind.ValidateRegionSource) ||
            root.TryGetProperty("SourceFaceId", out _) ||
            root.TryGetProperty("ExpectedDocumentSha256", out _) ||
            root.TryGetProperty("Requirement", out _);
    }

    private static async Task<int> RunRegionSourceAsync(
        JsonElement root, string resultPath, TextWriter error,
        CancellationToken cancellationToken)
    {
        RegionSourceValidationRequest? request = null;
        RegionSourceValidationResult? rejection = null;
        try
        {
            request = root.Deserialize<RegionSourceValidationRequest>(JsonOptions);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            // Routing already established the region boundary. Never downgrade
            // a typed-field failure to the legacy envelope or invent correlation.
            string? requestId = root.TryGetProperty("RequestId", out var id) &&
                id.ValueKind == JsonValueKind.String ? id.GetString() : null;
            rejection = new RegionSourceValidationResult(
                ContractVersion.RegionSourceValidation, requestId,
                WorkerOperationKind.ValidateRegionSource,
                RegionSourceValidationOutcome.RequestRejected);
        }

        var result = rejection ?? new RegionSourceValidationProcessor().Process(request);
        if (!await TryWriteResultAsync(resultPath, result, error, cancellationToken))
            return WorkerExitCodes.ResultWriteFailed;
        return result.Outcome switch
        {
            RegionSourceValidationOutcome.SourceLinkMatched => WorkerExitCodes.Succeeded,
            RegionSourceValidationOutcome.ImportFailed => WorkerExitCodes.OperationFailed,
            _ => WorkerExitCodes.RequestRejected
        };
    }

    private static WorkerOperationResult CreateRequestReadFailure(
        Exception exception)
    {
        return new WorkerOperationResult(
            ContractVersion.Current,
            requestId: null,
            WorkerOperationStatus.Failed,
            outputArtifactPath: null,
            new[]
            {
                new ContractDiagnostic(
                    RequestReadFailedCode,
                    ContractDiagnosticSeverity.Error,
                    $"Worker request could not be read: " +
                    $"{exception.GetType().Name}.")
            });
    }

    private static async Task<bool> TryWriteResultAsync<T>(
        string resultPath,
        T result,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultJson =
                JsonSerializer.Serialize(
                    result,
                    JsonOptions);

            await File.WriteAllTextAsync(
                resultPath,
                resultJson,
                cancellationToken);

            return true;
        }
        catch (Exception exception)
            when (exception is IOException
                  or UnauthorizedAccessException
                  or JsonException
                  or NotSupportedException)
        {
            await error.WriteLineAsync(
                $"Worker result could not be written: " +
                $"{exception.GetType().Name}.");

            return false;
        }
    }
}
