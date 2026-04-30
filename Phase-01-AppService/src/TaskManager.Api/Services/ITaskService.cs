using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetAllAsync(CancellationToken ct = default);
    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaskResponse> CreateAsync(TaskRequest request, CancellationToken ct = default);
    Task<TaskResponse?> UpdateAsync(int id, TaskRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
