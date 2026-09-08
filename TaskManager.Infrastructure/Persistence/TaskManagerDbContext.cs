using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Authentication;

namespace TaskManager.Infrastructure.Persistence;

public sealed class TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
            builder.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.HasIndex(x => x.Email).IsUnique();
            builder.Metadata.FindNavigation(nameof(User.Tasks))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<TaskItem>(builder =>
        {
            builder.ToTable("tasks");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(120).IsRequired();
            builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.DueDate).HasColumnName("due_date").IsRequired();
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            builder.HasOne(x => x.User).WithMany(nameof(User.Tasks)).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        SeedDemoData(modelBuilder);
    }

    private static void SeedDemoData(ModelBuilder modelBuilder)
    {
        var demoUserId = Guid.Parse("29ed1f5b-d6f4-4f06-8d8b-54089a0f2d13");
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateOnly(2026, 12, 31);

        modelBuilder.Entity<User>().HasData(new
        {
            Id = demoUserId,
            Name = "Demo User",
            Email = "demo@taskmanager.com",
            PasswordHash = PasswordHasher.HashWithSalt("Demo@123", "TaskManagerDemoSalt"),
            CreatedAt = createdAt
        });

        modelBuilder.Entity<TaskItem>().HasData(
            new
            {
                Id = Guid.Parse("f34f5b63-954d-40b4-aa90-af2174af5551"),
                Title = "Prepare project presentation",
                Description = "Create a concise walkthrough of the architecture and tests.",
                Status = TaskItemStatus.InProgress,
                DueDate = dueDate,
                UserId = demoUserId,
                CreatedAt = createdAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("312b1257-39f7-4ca6-a40b-4db4f934ce3f"),
                Title = "Review Clean Architecture notes",
                Description = "Check dependency boundaries before the interview.",
                Status = TaskItemStatus.Pending,
                DueDate = dueDate,
                UserId = demoUserId,
                CreatedAt = createdAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("9657b12d-8aac-4f60-9c9d-b4aa546582d8"),
                Title = "Submit technical exercise",
                Description = "Package the solution and README.",
                Status = TaskItemStatus.Done,
                DueDate = dueDate,
                UserId = demoUserId,
                CreatedAt = createdAt,
                UpdatedAt = (DateTime?)null
            });
    }
}
