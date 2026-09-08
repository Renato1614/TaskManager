namespace TaskManager.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "TaskManager";
    public string Audience { get; init; } = "TaskManager";
    public string Secret { get; init; } = "development-secret-change-me-development-secret";
    public int ExpirationMinutes { get; init; } = 120;
}
