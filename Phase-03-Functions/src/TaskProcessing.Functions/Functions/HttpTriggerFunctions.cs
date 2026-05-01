using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaskProcessing.Functions.Models;

namespace TaskProcessing.Functions.Functions;

public class HttpTriggerFunctions(ILogger<HttpTriggerFunctions> logger)
{
    [Function("GetStatus")]
    public async Task<HttpResponseData> GetStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequestData req)
    {
        logger.LogInformation("Status check called from {Ip}", req.Headers.TryGetValues("X-Forwarded-For", out var values) ? values.First() : "unknown");
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Functions are running. Time: {DateTime.UtcNow:O}");
        return response;
    }

    [Function("CreateTask")]
    public async Task<HttpResponseData> CreateTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks")] HttpRequestData req,
        CancellationToken ct)
    {
        var body = await req.ReadAsStringAsync();
        var taskRequest = JsonSerializer.Deserialize<TaskRequest>(body ?? "{}",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (taskRequest is null || string.IsNullOrWhiteSpace(taskRequest.Title))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("title is required");
            return bad;
        }

        var message = new TaskMessage
        {
            TaskId = Guid.NewGuid().ToString(),
            Title = taskRequest.Title,
            EnqueuedAt = DateTime.UtcNow,
        };

        logger.LogInformation("Task {TaskId} created via HTTP: {Title}", message.TaskId, message.Title);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(message, ct);
        return response;
    }
}
