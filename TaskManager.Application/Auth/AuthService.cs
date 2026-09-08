using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IClock clock,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        logger.LogInformation("Registering user with email {Email}", normalizedEmail);

        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            logger.LogWarning("Registration rejected because email {Email} is already registered", normalizedEmail);
            throw new AppException(AppErrorType.Conflict, "Email is already registered.");
        }

        var user = new User(Guid.NewGuid(), request.Name.Trim(), normalizedEmail, passwordHasher.Hash(request.Password), clock.UtcNow);
        await users.AddAsync(user, cancellationToken);
        await users.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} registered successfully", user.Id);

        return user.ToAuthResponse(jwtTokenGenerator.Generate(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Login failed for email {Email}", normalizedEmail);
            throw new AppException(AppErrorType.Unauthorized, "Invalid email or password.");
        }

        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return user.ToAuthResponse(jwtTokenGenerator.Generate(user));
    }

    public async Task<UserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw NotFound(userId);

        return user.ToResponse();
    }

    private AppException NotFound(Guid userId)
    {
        logger.LogWarning("Current user lookup failed for user {UserId}", userId);
        return new AppException(AppErrorType.NotFound, "User was not found.");
    }
}
