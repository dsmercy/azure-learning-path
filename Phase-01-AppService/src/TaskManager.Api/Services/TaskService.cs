using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public class TaskService(AppDbContext db, ILogger<TaskService> logger) : ITaskService
{
    public async Task<IEnumerable<TaskResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var tasks = await db.Tasks.ToListAsync(ct);
        return tasks.Select(MapToResponse);
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        return task is null ? null : MapToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(TaskRequest request, CancellationToken ct = default)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted,
            CreatedAt = DateTime.UtcNow
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Task {Id} created: {Title}", task.Id, task.Title);
        return MapToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, TaskRequest request, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) return null;

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Task {Id} updated", task.Id);
        return MapToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Task {Id} deleted", id);
        return true;
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        IsCompleted = task.IsCompleted,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
