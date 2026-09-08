namespace TaskManager.Application.Auth;

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, UserResponse User);

public sealed record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt);
