using TaskManager.Domain.Entities;

namespace TaskManager.Application.Auth;

public static class AuthMappings
{
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Name, user.Email, user.CreatedAt);

    public static AuthResponse ToAuthResponse(this User user, string token) =>
        new(token, user.ToResponse());
}
