namespace MPC.Worker;

public static class WorkerExitCodes
{
    public const int Succeeded = 0;

    public const int RequestRejected = 2;

    public const int OperationFailed = 3;

    public const int InvalidInvocation = 10;

    public const int RequestReadFailed = 11;

    public const int ResultWriteFailed = 12;
}
