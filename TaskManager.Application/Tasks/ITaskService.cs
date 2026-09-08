namespace TaskManager.Application.Tasks;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> GetAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
}
