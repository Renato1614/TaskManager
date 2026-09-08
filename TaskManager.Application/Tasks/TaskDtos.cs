using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks;

public sealed record CreateTaskRequest(string Title, string? Description, TaskItemStatus Status, DateOnly DueDate);

public sealed record UpdateTaskRequest(string Title, string? Description, TaskItemStatus Status, DateOnly DueDate);

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateOnly DueDate,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
