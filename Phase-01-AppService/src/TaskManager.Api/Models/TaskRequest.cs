namespace TaskManager.Api.Models;

public class TaskRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
