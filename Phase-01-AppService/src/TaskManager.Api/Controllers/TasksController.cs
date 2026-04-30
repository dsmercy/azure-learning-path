using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll(CancellationToken ct)
    {
        var tasks = await taskService.GetAllAsync(ct);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id, CancellationToken ct)
    {
        var task = await taskService.GetByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(TaskRequest request, CancellationToken ct)
    {
        var task = await taskService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, TaskRequest request, CancellationToken ct)
    {
        var task = await taskService.UpdateAsync(id, request, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await taskService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
