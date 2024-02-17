using Temporalio.Testing;
using Temporalio.Worker;

namespace OrderingService.Tests.Setup;

public static class TemporalSetupHelpers
{

    public static async Task<(WorkflowEnvironment, TemporalWorker)> SetupWorker(Action<TemporalWorkerOptions> callback)
    {
        var env = await WorkflowEnvironment.StartTimeSkippingAsync();

        var options = new TemporalWorkerOptions($"task-queue-{Guid.NewGuid()}");
        callback(options);

        var worker = new TemporalWorker(env.Client, options);

        return (env, worker);
    }
}