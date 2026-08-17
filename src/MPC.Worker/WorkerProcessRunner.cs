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

    private static async Task<bool> TryWriteResultAsync(
        string resultPath,
        WorkerOperationResult result,
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