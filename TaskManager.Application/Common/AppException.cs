namespace TaskManager.Application.Common;

public enum AppErrorType
{
    Validation,
    Conflict,
    Unauthorized,
    Forbidden,
    NotFound
}

public sealed class AppException(AppErrorType type, string message) : Exception(message)
{
    public AppErrorType Type { get; } = type;
}
