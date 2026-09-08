namespace TaskManager.Domain.Entities;

public sealed class User : IEntity
{
    private readonly List<TaskItem> _tasks = [];

    private User()
    {
    }

    public User(Guid id, string name, string email, string passwordHash, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<TaskItem> Tasks => _tasks;
}
