using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TaskProcessing.Functions.Functions;

public static class DurableOrchestration
{
    // Step 1 — HTTP trigger starts the orchestration and returns status URLs
    [Function("StartTaskOrchestration")]
    public static async Task<HttpResponseData> Start(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orchestrate")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("DurableOrchestration");
        var taskName = await req.ReadAsStringAsync() ?? "DefaultTask";

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(TaskOrchestrator), taskName);
        logger.LogInformation("Orchestration started: {InstanceId} for task: {TaskName}", instanceId, taskName);

        // Returns JSON with statusQueryGetUri, sendEventPostUri, terminatePostUri, etc.
        return client.CreateCheckStatusResponse(req, instanceId);
    }

    // Step 2 — Orchestrator calls activities in sequence (must be deterministic — no I/O here)
    [Function(nameof(TaskOrchestrator))]
    public static async Task<string> TaskOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var taskName = context.GetInput<string>() ?? "Unknown";

        var isValid = await context.CallActivityAsync<bool>(nameof(ValidateTask), taskName);
        if (!isValid)
            return $"Task '{taskName}' failed validation";

        var result = await context.CallActivityAsync<string>(nameof(ProcessTask), taskName);
        await context.CallActivityAsync(nameof(NotifyCompletion), result);

        return result;
    }

    // Step 3a — Activity: validate input
    [Function(nameof(ValidateTask))]
    public static bool ValidateTask([ActivityTrigger] string taskName, FunctionContext context)
    {
        var logger = context.GetLogger("DurableOrchestration");
        logger.LogInformation("Validating: {TaskName}", taskName);
        return !string.IsNullOrWhiteSpace(taskName) && taskName.Length <= 100;
    }

    // Step 3b — Activity: do the work
    [Function(nameof(ProcessTask))]
    public static string ProcessTask([ActivityTrigger] string taskName, FunctionContext context)
    {
        var logger = context.GetLogger("DurableOrchestration");
        logger.LogInformation("Processing: {TaskName}", taskName);
        return $"'{taskName}' completed at {DateTime.UtcNow:O}";
    }

    // Step 3c — Activity: notify
    [Function(nameof(NotifyCompletion))]
    public static void NotifyCompletion([ActivityTrigger] string result, FunctionContext context)
    {
        var logger = context.GetLogger("DurableOrchestration");
        logger.LogInformation("Notification sent: {Result}", result);
        // In a real app: send email, push notification, webhook, etc.
    }
}
