using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Tasks;

public sealed class TaskServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateOnly _today = new(2026, 9, 8);
    private readonly Mock<ITaskItemRepository> _tasks = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var clock = new FixedClock(new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc));
        _tasks.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _service = new TaskService(
            _tasks.Object,
            clock,
            new CreateTaskRequestValidator(clock),
            new UpdateTaskRequestValidator(clock),
            NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidTask_CreatesTask()
    {
        var request = new CreateTaskRequest("Write tests", "Cover main flow", TaskItemStatus.Pending, _today);

        var result = await _service.CreateAsync(request, _userId, CancellationToken.None);

        result.Title.Should().Be("Write tests");
        result.UserId.Should().Be(_userId);
        _tasks.Verify(x => x.AddAsync(It.Is<TaskItem>(t => t.UserId == _userId), It.IsAny<CancellationToken>()), Times.Once);
        _tasks.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithoutTitle_ThrowsValidationException()
    {
        var request = new CreateTaskRequest("", null, TaskItemStatus.Pending, _today);

        var act = () => _service.CreateAsync(request, _userId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithPastDueDate_ThrowsValidationException()
    {
        var request = new CreateTaskRequest("Late task", null, TaskItemStatus.Pending, _today.AddDays(-1));

        var act = () => _service.CreateAsync(request, _userId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Due date cannot be in the past*");
    }

    [Fact]
    public async Task CreateAsync_WithLongTitle_ThrowsValidationException()
    {
        var request = new CreateTaskRequest(new string('A', 121), null, TaskItemStatus.Pending, _today);

        var act = () => _service.CreateAsync(request, _userId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithLongDescription_ThrowsValidationException()
    {
        var request = new CreateTaskRequest("Valid title", new string('A', 1001), TaskItemStatus.Pending, _today);

        var act = () => _service.CreateAsync(request, _userId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_ChangesStatus()
    {
        var task = new TaskItem(Guid.NewGuid(), "Old", null, TaskItemStatus.Pending, _today, _userId, DateTime.UtcNow);
        _tasks.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _service.UpdateAsync(task.Id, new UpdateTaskRequest("New", null, TaskItemStatus.Done, _today), _userId, CancellationToken.None);

        result.Status.Should().Be(TaskItemStatus.Done);
        result.Title.Should().Be("New");
        _tasks.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ThrowsNotFound()
    {
        var taskId = Guid.NewGuid();
        _tasks.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        var act = () => _service.UpdateAsync(taskId, new UpdateTaskRequest("New", null, TaskItemStatus.Done, _today), _userId, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Type.Should().Be(AppErrorType.NotFound);
    }

    [Fact]
    public async Task GetAsync_ForAnotherUsersTask_ThrowsForbidden()
    {
        var task = new TaskItem(Guid.NewGuid(), "Private", null, TaskItemStatus.Pending, _today, Guid.NewGuid(), DateTime.UtcNow);
        _tasks.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var act = () => _service.GetAsync(task.Id, _userId, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Type.Should().Be(AppErrorType.Forbidden);
    }

    [Fact]
    public async Task DeleteAsync_ForOwnedTask_RemovesTask()
    {
        var task = new TaskItem(Guid.NewGuid(), "Delete me", null, TaskItemStatus.Pending, _today, _userId, DateTime.UtcNow);
        _tasks.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        await _service.DeleteAsync(task.Id, _userId, CancellationToken.None);

        _tasks.Verify(x => x.Remove(task), Times.Once);
        _tasks.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ForAnotherUsersTask_ThrowsForbidden()
    {
        var task = new TaskItem(Guid.NewGuid(), "Private", null, TaskItemStatus.Pending, _today, Guid.NewGuid(), DateTime.UtcNow);
        _tasks.Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var act = () => _service.DeleteAsync(task.Id, _userId, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Type.Should().Be(AppErrorType.Forbidden);
    }

    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow => utcNow;
        public DateOnly Today => DateOnly.FromDateTime(utcNow);
    }
}
