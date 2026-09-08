using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Auth;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tests.Auth;

public sealed class AuthServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _users.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password");
        _passwordHasher.Setup(x => x.Verify("Password@123", "hashed-password")).Returns(true);
        _jwtTokenGenerator.Setup(x => x.Generate(It.IsAny<User>())).Returns("jwt-token");

        _service = new AuthService(
            _users.Object,
            _passwordHasher.Object,
            _jwtTokenGenerator.Object,
            new FixedClock(new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc)),
            new RegisterRequestValidator(),
            new LoginRequestValidator(),
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_CreatesUserAndReturnsToken()
    {
        var request = new RegisterRequest("Ada Lovelace", "ADA@EXAMPLE.COM", "Password@123");

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        result.User.Email.Should().Be("ada@example.com");
        _users.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Name == "Ada Lovelace" &&
            u.Email == "ada@example.com" &&
            u.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsConflict()
    {
        _users.Setup(x => x.EmailExistsAsync("ada@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _service.RegisterAsync(new RegisterRequest("Ada Lovelace", "ada@example.com", "Password@123"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Type.Should().Be(AppErrorType.Conflict);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ThrowsValidationException()
    {
        var act = () => _service.RegisterAsync(new RegisterRequest("Ada Lovelace", "not-an-email", "Password@123"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorized()
    {
        var user = new User(_userId, "Ada Lovelace", "ada@example.com", "hashed-password", DateTime.UtcNow);
        _users.Setup(x => x.GetByEmailAsync("ada@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var act = () => _service.LoginAsync(new LoginRequest("ada@example.com", "WrongPassword"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Type.Should().Be(AppErrorType.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserDoesNotExist_ThrowsNotFound()
    {
        _users.Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _service.GetCurrentUserAsync(_userId, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Type.Should().Be(AppErrorType.NotFound);
    }

    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow => utcNow;
        public DateOnly Today => DateOnly.FromDateTime(utcNow);
    }
}
