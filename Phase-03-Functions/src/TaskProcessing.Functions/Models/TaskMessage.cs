namespace TaskProcessing.Functions.Models;

public class TaskMessage
{
    public string TaskId { get; set; } = string.Empty;
    public required string Title { get; set; }
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
}
