using System;
using System.IO;
using System.Text.Json;
using MPC.Contracts;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerContractJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };

    internal string SerializeRequest(
        WorkerOperationRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return JsonSerializer.Serialize(
            request,
            JsonOptions);
    }

    internal WorkerOperationResult DeserializeResult(
        string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException(
                "Worker result JSON cannot be null, empty, or whitespace.",
                nameof(json));
        }

        WorkerOperationResult? result =
            JsonSerializer.Deserialize<WorkerOperationResult>(
                json,
                JsonOptions);

        if (result is null)
        {
            throw new InvalidDataException(
                "Worker result JSON did not contain a result.");
        }

        return result;
    }
}
