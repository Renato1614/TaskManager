using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace TaskManager.Api.IntegrationTests.Helpers;

public sealed class TaskManagerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("taskmanager_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync() => await _database.StartAsync();

    public new async Task DisposeAsync() => await _database.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:taskmanagerdb"] = _database.GetConnectionString(),
                ["Jwt:Issuer"] = "TaskManager.Tests",
                ["Jwt:Audience"] = "TaskManager.Tests",
                ["Jwt:Secret"] = "integration-test-secret-with-enough-length",
                ["Jwt:ExpirationMinutes"] = "60"
            });
        });
    }
}
