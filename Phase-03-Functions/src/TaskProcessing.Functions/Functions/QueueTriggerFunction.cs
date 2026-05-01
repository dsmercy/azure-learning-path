using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaskProcessing.Functions.Models;

namespace TaskProcessing.Functions.Functions;

public class QueueTriggerFunction(ILogger<QueueTriggerFunction> logger)
{
    // Fires when a message arrives in task-queue
    // The runtime deserializes the JSON message body into TaskMessage automatically
    [Function("ProcessTaskQueue")]
    public void Run(
        [QueueTrigger("task-queue", Connection = "AzureWebJobsStorage")] TaskMessage message)
    {
        logger.LogInformation("Queue trigger fired: TaskId={TaskId} Title={Title} EnqueuedAt={EnqueuedAt}",
            message.TaskId, message.Title, message.EnqueuedAt);

        // In a real app: send email, update database, call external API, etc.
        logger.LogInformation("Task {TaskId} processed successfully", message.TaskId);
    }
}
