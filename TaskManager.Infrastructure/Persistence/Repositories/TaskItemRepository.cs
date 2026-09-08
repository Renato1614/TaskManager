using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class TaskItemRepository(TaskManagerDbContext dbContext) : Repository<TaskItem>(dbContext), ITaskItemRepository
{
    public async Task<IReadOnlyList<TaskItem>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await Set
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}
