using MPC.Worker;

var runner =
    new WorkerProcessRunner();

return await runner.RunAsync(
    args,
    Console.Error);
