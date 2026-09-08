using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tasks;

public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.DueDate, task.UserId, task.CreatedAt, task.UpdatedAt);
}
