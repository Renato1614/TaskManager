using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using TaskManager.Api.IntegrationTests.Helpers;
using TaskManager.Application.Auth;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.IntegrationTests;

public sealed class TaskManagerApiTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RegisterUser_ReturnsJwt()
    {
        var response = await RegisterAsync("Ada Lovelace", UniqueEmail(), "Password@123");

        response.Token.Should().NotBeNullOrWhiteSpace();
        response.User.Email.Should().Contain("@example.com");
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ReturnsConflict()
    {
        var email = UniqueEmail();
        await RegisterAsync("Ada Lovelace", email, "Password@123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Ada Lovelace", email, "Password@123"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Ada Lovelace", "not-an-email", "Password@123"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginUser_ReturnsJwt()
    {
        var email = UniqueEmail();
        await RegisterAsync("Grace Hopper", email, "Password@123");

        var response = await LoginAsync(email, "Password@123");

        response.Token.Should().NotBeNullOrWhiteSpace();
        response.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginUser_WithInvalidPassword_ReturnsUnauthorized()
    {
        var email = UniqueEmail();
        await RegisterAsync("Grace Hopper", email, "Password@123");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentUserWithoutPasswordHash()
    {
        var auth = await RegisterAsync("Current User", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await _client.GetAsync("/api/auth/me");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain(auth.User.Email);
        body.Should().NotContain("passwordHash");
        body.Should().NotContain("PasswordHash");
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/private-demo");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithToken_Succeeds()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/auth/private-demo");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginSeededDemoUser_ReturnsJwt()
    {
        var response = await LoginAsync("demo@taskmanager.com", "Demo@123");

        response.Token.Should().NotBeNullOrWhiteSpace();
        response.User.Email.Should().Be("demo@taskmanager.com");
    }

    [Fact]
    public async Task TaskCrud_ForAuthenticatedUser_Succeeds()
    {
        await AuthenticateAsync();

        var create = await _client.PostAsJsonAsync("/api/tasks", NewTask("Interview prep"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await create.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        task.Should().NotBeNull();

        var list = await _client.GetFromJsonAsync<IReadOnlyList<TaskResponse>>("/api/tasks", JsonOptions);
        list.Should().Contain(x => x.Id == task!.Id);

        var update = await _client.PutAsJsonAsync($"/api/tasks/{task!.Id}", new UpdateTaskRequest("Interview prep done", null, TaskItemStatus.Done, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2)));
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await update.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        updated!.Status.Should().Be(TaskItemStatus.Done);

        var delete = await _client.DeleteAsync($"/api/tasks/{task.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetTask_WhenTaskDoesNotExist_ReturnsNoContent()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateTask_WhenTaskDoesNotExist_ReturnsNoContent()
    {
        await AuthenticateAsync();

        var response = await _client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", NewUpdateTask("Missing task", TaskItemStatus.Done));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNoContent()
    {
        await AuthenticateAsync();

        var response = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateTask_WithLongTitle_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/tasks", NewTask(new string('A', 121)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WithLongDescription_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        var request = new CreateTaskRequest("Valid title", new string('A', 1001), TaskItemStatus.Pending, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WithInvalidStatusJson_ReturnsBadRequest()
    {
        await AuthenticateAsync();
        using var content = new StringContent(
            """
            {
              "title": "Invalid status",
              "description": "Bad enum value",
              "status": "Blocked",
              "dueDate": "2026-12-31"
            }
            """,
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/tasks", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UserCannotAccessAnotherUsersTask()
    {
        var userA = await RegisterAsync("User A", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Token);
        var create = await _client.PostAsJsonAsync("/api/tasks", NewTask("User A private task"));
        var task = await create.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        var userB = await RegisterAsync("User B", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userB.Token);

        var response = await _client.GetAsync($"/api/tasks/{task!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserCannotUpdateAnotherUsersTask()
    {
        var userA = await RegisterAsync("User A", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Token);
        var task = await CreateTaskAsync("User A private task");

        var userB = await RegisterAsync("User B", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userB.Token);

        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", NewUpdateTask("Stolen update", TaskItemStatus.Done));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserCannotDeleteAnotherUsersTask()
    {
        var userA = await RegisterAsync("User A", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Token);
        var task = await CreateTaskAsync("User A private task");

        var userB = await RegisterAsync("User B", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userB.Token);

        var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListTasks_ReturnsOnlyAuthenticatedUsersTasks()
    {
        var userA = await RegisterAsync("User A", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Token);
        var userATask = await CreateTaskAsync("User A visible task");

        var userB = await RegisterAsync("User B", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userB.Token);
        var userBTask = await CreateTaskAsync("User B visible task");

        var list = await _client.GetFromJsonAsync<IReadOnlyList<TaskResponse>>("/api/tasks", JsonOptions);

        list.Should().Contain(x => x.Id == userBTask.Id);
        list.Should().NotContain(x => x.Id == userATask.Id);
    }

    private async Task AuthenticateAsync()
    {
        var auth = await RegisterAsync("Test User", UniqueEmail(), "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
    }

    private async Task<AuthResponse> RegisterAsync(string name, string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(name, email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private async Task<TaskResponse> CreateTaskAsync(string title)
    {
        var create = await _client.PostAsJsonAsync("/api/tasks", NewTask(title));
        create.EnsureSuccessStatusCode();
        return (await create.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions))!;
    }

    private static CreateTaskRequest NewTask(string title) =>
        new(title, "Created from integration test", TaskItemStatus.Pending, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));

    private static UpdateTaskRequest NewUpdateTask(string title, TaskItemStatus status) =>
        new(title, "Updated from integration test", status, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2));

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
}
