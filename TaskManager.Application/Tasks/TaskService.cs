using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tasks;

public sealed class TaskService(
    ITaskItemRepository tasks,
    IClock clock,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<UpdateTaskRequest> updateValidator,
    ILogger<TaskService> logger) : ITaskService
{
    public async Task<IReadOnlyList<TaskResponse>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await tasks.ListByUserIdAsync(userId, cancellationToken);
        logger.LogInformation("Listed {TaskCount} tasks for user {UserId}", items.Count, userId);
        return items.Select(task => task.ToResponse()).ToList();
    }

    public async Task<TaskResponse> GetAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        return task.ToResponse();
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = new TaskItem(
            Guid.NewGuid(),
            request.Title.Trim(),
            NormalizeDescription(request.Description),
            request.Status,
            request.DueDate,
            userId,
            clock.UtcNow);

        await tasks.AddAsync(task, cancellationToken);
        await tasks.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Task {TaskId} created for user {UserId} with status {Status}", task.Id, userId, task.Status);

        return task.ToResponse();
    }

    public async Task<TaskResponse> UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        task.Update(request.Title.Trim(), NormalizeDescription(request.Description), request.Status, request.DueDate, clock.UtcNow);
        await tasks.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Task {TaskId} updated by user {UserId} with status {Status}", task.Id, userId, task.Status);

        return task.ToResponse();
    }

    public async Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        tasks.Remove(task);
        await tasks.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Task {TaskId} deleted by user {UserId}", task.Id, userId);
    }

    private async Task<TaskItem> GetOwnedTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(taskId, cancellationToken);

        if (task is null)
        {
            logger.LogWarning("Task {TaskId} was not found for user {UserId}", taskId, userId);
            throw new AppException(AppErrorType.NotFound, "Task was not found.");
        }

        if (task.UserId != userId)
        {
            logger.LogWarning(
                "User {UserId} attempted to access task {TaskId} owned by user {OwnerUserId}",
                userId,
                task.Id,
                task.UserId);

            throw new AppException(AppErrorType.Forbidden, "You cannot access another user's task.");
        }

        return task;
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

}
