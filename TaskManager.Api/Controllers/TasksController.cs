using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Services;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TasksController(ITaskService taskService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List(CancellationToken cancellationToken)
    {
        var response = await taskService.ListAsync(currentUser.UserId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await taskService.GetAsync(id, currentUser.UserId, cancellationToken);
            return Ok(response);
        }
        catch (AppException ex) when (ex.Type == AppErrorType.NotFound)
        {
            return NoContent();
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var response = await taskService.CreateAsync(request, currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await taskService.UpdateAsync(id, request, currentUser.UserId, cancellationToken);
            return Ok(response);
        }
        catch (AppException ex) when (ex.Type == AppErrorType.NotFound)
        {
            return NoContent();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(id, currentUser.UserId, cancellationToken);
        return NoContent();
    }
}
