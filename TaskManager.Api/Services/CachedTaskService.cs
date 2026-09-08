using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TaskManager.Application.Tasks;

namespace TaskManager.Api.Services;

public sealed class CachedTaskService(
    TaskService inner,
    IDistributedCache cache,
    ILogger<CachedTaskService> logger) : ITaskService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public async Task<IReadOnlyList<TaskResponse>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = GetUserTasksKey(userId);
        var cachedTasks = await GetAsync<IReadOnlyList<TaskResponse>>(cacheKey, cancellationToken);

        if (cachedTasks is not null)
        {
            logger.LogInformation("Task list cache hit for user {UserId}", userId);
            return cachedTasks;
        }

        var tasks = await inner.ListAsync(userId, cancellationToken);
        await SetAsync(cacheKey, tasks, cancellationToken);
        logger.LogInformation("Task list cached for user {UserId}", userId);

        return tasks;
    }

    public async Task<TaskResponse> GetAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = GetTaskKey(userId, taskId);
        var cachedTask = await GetAsync<TaskResponse>(cacheKey, cancellationToken);

        if (cachedTask is not null)
        {
            logger.LogInformation("Task cache hit for task {TaskId} and user {UserId}", taskId, userId);
            return cachedTask;
        }

        var task = await inner.GetAsync(taskId, userId, cancellationToken);
        await SetAsync(cacheKey, task, cancellationToken);
        logger.LogInformation("Task {TaskId} cached for user {UserId}", taskId, userId);

        return task;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var task = await inner.CreateAsync(request, userId, cancellationToken);

        await cache.RemoveAsync(GetUserTasksKey(userId), cancellationToken);
        await SetAsync(GetTaskKey(userId, task.Id), task, cancellationToken);

        return task;
    }

    public async Task<TaskResponse> UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var task = await inner.UpdateAsync(taskId, request, userId, cancellationToken);

        await cache.RemoveAsync(GetUserTasksKey(userId), cancellationToken);
        await SetAsync(GetTaskKey(userId, task.Id), task, cancellationToken);

        return task;
    }

    public async Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        await inner.DeleteAsync(taskId, userId, cancellationToken);

        await cache.RemoveAsync(GetUserTasksKey(userId), cancellationToken);
        await cache.RemoveAsync(GetTaskKey(userId, taskId), cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken)
    {
        var value = await cache.GetStringAsync(cacheKey, cancellationToken);

        return string.IsNullOrWhiteSpace(value)
            ? default
            : JsonSerializer.Deserialize<T>(value, JsonOptions);
    }

    private static string GetTaskKey(Guid userId, Guid taskId) => $"tasks:{userId}:item:{taskId}";

    private static string GetUserTasksKey(Guid userId) => $"tasks:{userId}:list";

    private static async Task SetAsync<T>(IDistributedCache cache, string cacheKey, T value, CancellationToken cancellationToken)
    {
        var serializedValue = JsonSerializer.Serialize(value, JsonOptions);
        await cache.SetStringAsync(cacheKey, serializedValue, CacheOptions, cancellationToken);
    }

    private Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken) =>
        SetAsync(cache, cacheKey, value, cancellationToken);
}
