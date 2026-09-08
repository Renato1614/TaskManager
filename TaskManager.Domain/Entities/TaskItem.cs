using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public sealed class TaskItem : IEntity
{
    private TaskItem()
    {
    }

    public TaskItem(
        Guid id,
        string title,
        string? description,
        TaskItemStatus status,
        DateOnly dueDate,
        Guid userId,
        DateTime createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        DueDate = dueDate;
        UserId = userId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateOnly DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public User? User { get; private set; }

    public void Update(string title, string? description, TaskItemStatus status, DateOnly dueDate, DateTime updatedAt)
    {
        Title = title;
        Description = description;
        Status = status;
        DueDate = dueDate;
        UpdatedAt = updatedAt;
    }
}
