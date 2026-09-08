using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface ITaskItemRepository : IRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
