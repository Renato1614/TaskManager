# Use of AI Tools

This file documents how AI was used in this technical interview project.

## How I Arrived at the Final Prompt

Before asking the AI to implement the project, I used it as a planning and analysis tool. The goal was to convert the interview exercise requirements into a clear technical prompt that could guide the implementation in a structured way.

The process followed an ask-and-planning strategy:

1. I first used AI to help understand the expected scope of the technical exercise.
2. I separated the requirements into backend, frontend, database, authentication, testing, Aspire, and documentation.
3. I asked the AI to help organize the work according to Clean Architecture, so that Domain, Application, Infrastructure, API, tests, and frontend responsibilities were clear before implementation started.
4. I used planning prompts to identify the public API endpoints, entities, DTOs, validation rules, authorization rules, seed data, and test cases.
5. After the requirements were organized, I combined them into one complete implementation prompt.

This approach helped me use AI with more control. Instead of only asking the AI to "create a task manager", I used it to reason about the architecture first, define the expected behavior, and create a detailed prompt that could be validated later through build, tests, and manual review.

The final prompt was intentionally specific about:

- Clean Architecture project boundaries;
- thin controllers and business rules in the Application layer;
- PostgreSQL and EF Core persistence in Infrastructure;
- JWT authentication and user ownership checks;
- Angular pages, guards, interceptors, and validation;
- Aspire orchestration;
- unit and integration tests;
- README and AI usage documentation.

## Prompt Used

The main prompt used to generate the project was:

```text
You are a senior full-stack .NET architect and engineer.

Build a complete technical interview project: a simple Task Management System using Clean Architecture, ASP.NET Core Web API, Angular, .NET 10, .NET Aspire, Entity Framework Core, JWT authentication, and Testcontainers for integration tests.

Project goal:
Create a full-stack application where authenticated users can register, log in, and manage their own tasks. Each user can create, read, update, and delete only their own tasks.

Business domain:
A TaskItem belongs to a User.

TaskItem fields:
- Id: Guid
- Title: string, required, max length 120
- Description: string, optional, max length 1000
- Status: enum with values Pending, InProgress, Done
- DueDate: DateOnly or DateTime
- UserId: Guid
- CreatedAt: DateTime
- UpdatedAt: DateTime?

User fields:
- Id: Guid
- Name: string, required
- Email: string, required, unique
- PasswordHash: string
- CreatedAt: DateTime

Required backend architecture:
Use Clean Architecture with the following projects:

- TaskManager.Domain
- TaskManager.Application
- TaskManager.Infrastructure
- TaskManager.Api
- TaskManager.AppHost
- TaskManager.ServiceDefaults

Also create test projects:

- TaskManager.Application.Tests
- TaskManager.Api.IntegrationTests

Backend requirements:
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL database
- JWT authentication
- Password hashing
- FluentValidation
- Repository or application service abstraction where appropriate
- No business rules inside controllers
- Controllers should delegate to the Application layer
- Infrastructure should implement persistence and authentication details
- Domain should not reference Application, Infrastructure, or API
- Application should not depend on Infrastructure or API

API endpoints:

Authentication:
- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/me, authorized
- GET /api/auth/public-demo, anonymous
- GET /api/auth/private-demo, authorized

Tasks:
- GET /api/tasks, authorized
- GET /api/tasks/{id}, authorized
- POST /api/tasks, authorized
- PUT /api/tasks/{id}, authorized
- DELETE /api/tasks/{id}, authorized

Authorization rule:
A user must only be able to view, update, or delete their own tasks. Add tests for this.

Seed data:
Create a demo user:
- Email: demo@taskmanager.com
- Password: Demo@123
- Name: Demo User

Create three demo tasks for this user:
- Prepare project presentation, InProgress
- Review Clean Architecture notes, Pending
- Submit technical exercise, Done

Testing requirements:
Use xUnit, FluentAssertions, and Moq for unit tests.

Application unit tests should cover:
- Creating a valid task
- Rejecting a task without title
- Rejecting an invalid due date
- Updating task status
- Preventing access to another user's task

Integration tests should use:
- Microsoft.AspNetCore.Mvc.Testing
- Testcontainers for .NET
- PostgreSQL container
- Real EF Core migrations against the container database
- Real HTTP calls through HttpClient

Integration tests should cover:
- Register user
- Login user and receive JWT
- Access protected endpoint without token returns 401
- Access protected endpoint with token succeeds
- Create task
- List tasks for authenticated user
- Update task
- Delete task
- User A cannot access User B's task

Frontend requirements:
Create an Angular frontend integrated with the API.

Frontend pages:
- Login
- Register
- Task list
- Create task
- Edit task

Frontend behavior:
- Use Angular Router
- Use Reactive Forms
- Use an HTTP interceptor to attach the JWT token
- Use route guards for authenticated pages
- Store token in localStorage
- Show validation errors
- Show loading and error states
- Make the UI responsive and user-friendly
- Keep component and service structure clean

Suggested Angular structure:
- core/auth
- core/interceptors
- core/guards
- features/auth
- features/tasks
- shared/components
- shared/models

Aspire requirements:
- Add an AppHost project
- Wire the API and PostgreSQL database through Aspire
- Include ServiceDefaults
- Make it easy to run the full app locally

README requirements:
Create a clear README with:
- Project overview
- User story
- Architecture explanation
- Technologies used
- Setup instructions
- How to run with Aspire
- How to run tests
- Docker requirement for Testcontainers
- Seeded credentials
- API endpoint list
- Explanation of Clean Architecture decisions
- Explanation of TDD/testing strategy
- A section called "Use of Generative AI"

In the "Use of Generative AI" section, include:
- The prompt used to generate the project
- How the generated code was validated
- What was corrected or improved
- How edge cases, authentication, authorization, and validation were handled

Code quality requirements:
- Use clear naming
- Keep controllers thin
- Use DTOs for API contracts
- Use async/await
- Use cancellation tokens where appropriate
- Return appropriate HTTP status codes
- Avoid leaking password hashes
- Avoid using EF Core entities directly as public API response models
- Add comments only when they clarify non-obvious decisions
- Ensure the solution builds without warnings where possible

Please generate the complete solution, including backend, frontend, tests, Aspire setup, seed data, and README.
```

After the first implementation, I used AI again to refactor the repository layer with these prompts:

```text
you created 2 repository classes thath uses the same methods, create a base repository, that have the basic crud operations and use polimothism to reutilizate the methods.
```

```text
the save changes could also be at the base respoditory
```

I also used AI to correct duplicated response mapping logic with this prompt:

```text
the functions like ToResponse is a bad architecture decision, because if yout want to use in another class you will need to copy and past the function, how we can correct this problem?
```

## Iterative Use of AI During Development

AI was not used only once to generate code. I used it iteratively during development, review, debugging, and validation. The interaction was closer to pair programming: I reviewed the generated code, questioned architectural decisions, asked for corrections, tested the result, and used runtime errors to guide the next changes.

Examples of follow-up prompts and decisions:

```text
In the controller, it needs to follow REST API conventions.

[HttpGet("{id:guid}")]
public Task<TaskResponse> Get(Guid id, CancellationToken cancellationToken) =>
    taskService.GetAsync(id, currentUser.UserId, cancellationToken);

Return 200 when the request succeeds and 204 when the resource is not found. Check the project for places that do not follow this pattern and adjust them.
```

This led to controller actions returning explicit `ActionResult` responses instead of returning DTOs directly. It made the HTTP contract clearer and easier to test.

```text
add a middleware of global exception handler
```

This led to a centralized exception handling approach using ASP.NET Core exception handling. Validation errors, authorization errors, conflict errors, and unexpected errors are handled consistently instead of duplicating try/catch logic everywhere.

```text
When I run the project, the Aspire dashboard link is not appearing.
```

```text
I am running the project using Aspire, not running the API directly.
```

These prompts were used during runtime troubleshooting. The AI helped distinguish between running the API directly and running the full application through Aspire. This was important because, in Aspire mode, the database connection string is provided by the AppHost resource, not only by `appsettings.Development.json`.

```text
Now I need to add the frontend image/resource to Aspire as well and point it to the backend.
```

This led to adding the Angular frontend as a `client` resource in the Aspire AppHost and configuring the Angular proxy to forward `/api` requests to the Aspire-managed API service.

```text
In the frontend, I want you to group the tasks by status like a Kanban board, and I want to change the status by dragging a task to a different column.
```

This led to using Angular CDK DragDrop to create a Kanban-style task board grouped by `Pending`, `InProgress`, and `Done`. Dragging a task between columns updates its status through the existing task update endpoint.

## Questions and Decisions During the Process

During the project, I used AI to clarify doubts and compare implementation choices, but the final decisions were guided by the exercise requirements and by manual review.

Important decisions included:

- using controllers instead of minimal APIs because the exercise expected controller-based REST endpoints;
- keeping business rules in the Application layer instead of controllers;
- keeping EF Core, JWT, password hashing, and repositories in Infrastructure;
- using `DateOnly` for due dates because the domain only needs a date, not a time;
- returning DTOs instead of exposing EF Core entities directly;
- using `204 No Content` for task not-found cases based on the project review request;
- using `npm.cmd` on Windows to avoid PowerShell execution policy issues with `npm.ps1`;
- using Angular CDK DragDrop for the Kanban behavior instead of writing custom drag-and-drop logic;
- using Testcontainers for integration tests so the API is tested against a real PostgreSQL database.

The AI helped propose fixes and implementation details, but I reviewed the result by running the app, checking errors, and asking follow-up questions when something did not match the intended architecture or behavior.

## Example of AI-Assisted Refactoring

The first generated repositories duplicated basic persistence methods.

Old sample:

```csharp
public sealed class UserRepository(TaskManagerDbContext dbContext) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken) =>
        dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}

public sealed class TaskItemRepository(TaskManagerDbContext dbContext) : ITaskItemRepository
{
    public Task AddAsync(TaskItem task, CancellationToken cancellationToken) =>
        dbContext.Tasks.AddAsync(task, cancellationToken).AsTask();

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tasks.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
```

New sample:

```csharp
public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public abstract class Repository<TEntity>(TaskManagerDbContext dbContext) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected DbSet<TEntity> Set => dbContext.Set<TEntity>();

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken) =>
        Set.AddAsync(entity, cancellationToken).AsTask();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Set.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Remove(TEntity entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
```

Concrete repositories now reuse the base behavior and keep only their specific queries:

```csharp
public sealed class UserRepository(TaskManagerDbContext dbContext)
    : Repository<User>(dbContext), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Set.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
}

public sealed class TaskItemRepository(TaskManagerDbContext dbContext)
    : Repository<TaskItem>(dbContext), ITaskItemRepository
{
    public async Task<IReadOnlyList<TaskItem>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await Set.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
}
```

## Improvement From This Change

This refactor improved the code because:

- duplicated CRUD code was removed from `UserRepository` and `TaskItemRepository`;
- common repository behavior is now implemented once in `Repository<TEntity>`;
- future repositories can reuse `AddAsync`, `GetByIdAsync`, `Remove`, and `SaveChangesAsync`;
- concrete repositories are easier to read because they only contain queries specific to that entity;
- the Application layer still depends on abstractions, not directly on Entity Framework Core.

## Example of Mapping Refactoring

The first implementation had private mapping methods inside services.

Old sample:

```csharp
public sealed class TaskService(...)
{
    public async Task<TaskResponse> GetAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        return ToResponse(task);
    }

    private static TaskResponse ToResponse(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.DueDate, task.UserId, task.CreatedAt, task.UpdatedAt);
}
```

New sample:

```csharp
public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.DueDate, task.UserId, task.CreatedAt, task.UpdatedAt);
}

public sealed class TaskService(...)
{
    public async Task<TaskResponse> GetAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        return task.ToResponse();
    }
}
```

## Improvement From The Mapping Change

This change improved the architecture because:

- mapping logic is reusable by other Application services;
- services no longer need private copied `ToResponse` methods;
- DTO conversion remains outside the Domain layer;
- the Application layer still owns the API-facing response models;
- future response mapping changes can be made in one place.

## Debugging and Manual Validation With AI

AI was also used to investigate real runtime errors instead of only writing new code.

One example was the PostgreSQL authentication error while running through Aspire:

```text
Npgsql.PostgresException: '28P01: password authentication failed for user "postgres"'
```

The initial assumption was that the direct API connection string could be wrong. After clarifying that the application was running through Aspire, the fix was moved to the AppHost configuration. The PostgreSQL resource was changed to avoid a stale persistent local volume during development, because Aspire can regenerate database credentials while an old volume still expects a previous password.

Another example was the EF Core migration error:

```text
Microsoft.EntityFrameworkCore.Migrations.PendingModelChangesWarning:
The model for context 'TaskManagerDbContext' has pending changes.
```

The AI helped inspect the migration files and identify that the migration existed but the `TaskManagerDbContextModelSnapshot` was incomplete. The migration was recreated so EF Core had an accurate snapshot of the current model, seed data, relationships, and column mappings.

These corrections were not accepted blindly. They were validated with build commands, EF Core migration checks, frontend builds, and test execution.

## Manual Checks Performed

In addition to automated tests, I used manual validation while developing:

- ran the backend through Aspire and checked whether the dashboard and resources appeared correctly;
- verified that the API connected to the Aspire-managed PostgreSQL database instead of the direct local database configuration;
- checked PostgreSQL startup/authentication errors from container logs;
- confirmed that EF Core migrations could run at application startup;
- built the Angular frontend after adding the Kanban UI and CDK dependency;
- checked that the frontend could be registered in Aspire as a separate `client` resource;
- reviewed controller responses to ensure the HTTP contract matched the intended REST behavior;
- reviewed that auth responses did not expose password hashes.

These checks helped catch issues that were not visible from code generation alone, such as Aspire resource configuration, locked build files while the AppHost was running, npm/PowerShell behavior on Windows, and EF Core migration snapshot problems.

## Test Coverage Improvements

After the first implementation, I asked whether the logic and integration tests covered all important scenarios:

```text
Do the logic and integration tests cover all scenarios?
```

The answer showed that the original tests covered the main happy paths and core authorization rule, but not enough negative and edge cases. I then asked AI to generate the missing tests:

```text
Generate those tests, then.
```

The test suite was expanded to cover:

- registration with duplicate email;
- registration with invalid email;
- login with invalid password;
- current user endpoint without leaking password hash;
- seeded demo user login;
- protected endpoint access with and without JWT;
- task create/list/update/delete flow;
- task not found cases;
- long title and long description validation;
- invalid task status sent through JSON;
- User B being blocked from reading, updating, or deleting User A's task;
- task listing returning only the authenticated user's tasks.

Application tests were also expanded to cover:

- auth service registration behavior;
- duplicate email conflict;
- invalid email validation;
- invalid password handling;
- user not found handling;
- task delete behavior;
- title and description length validation;
- not found and forbidden task rules.

## Manual Mutation Testing

To check whether the tests were meaningful, I used AI to temporarily introduce controlled bugs and run the tests again:

```text
Introduce some bugs into the project and run the tests again to verify whether they fail correctly. If a test still succeeds when it should fail, adjust the test coverage.
```

The temporary bugs included:

- removing the task ownership check;
- allowing task titles longer than the configured limit;
- ignoring duplicate email validation;
- changing the expected `204 No Content` behavior to `404 Not Found` for not-found task cases.

The tests failed when these bugs were introduced, which confirmed that the suite was able to catch important regressions. After each check, the temporary bug was reverted and the test suite was run again.

This step was useful because it validated not only that the tests passed, but also that they could fail for the right reasons.

## Corrections Made After AI Output

Several improvements were made after reviewing or running the AI-generated code:

- repository duplication was removed by introducing a generic base repository;
- `SaveChangesAsync` was moved into the base repository abstraction;
- duplicated mapping methods were moved to reusable mapping extension classes;
- controller methods were changed to return explicit HTTP responses;
- global exception handling was added;
- Aspire PostgreSQL configuration was adjusted after authentication errors;
- EF Core migrations were recreated after detecting an incomplete model snapshot;
- Angular was wired into Aspire and configured to proxy API calls to the backend resource;
- the task list UI was improved into a Kanban board with drag-and-drop status changes;
- unit and integration tests were expanded after identifying missing edge cases;
- tests were validated with controlled bug insertion.

## Validation

The project was validated multiple times during implementation using backend, frontend, EF Core, and integration test commands.

Examples:

```powershell
cmd /c dotnet build TaskManager.slnx --no-restore
cmd /c dotnet build TaskManager.AppHost\TaskManager.AppHost.csproj --configuration Release --no-restore
cmd /c dotnet ef migrations has-pending-model-changes --project TaskManager.Infrastructure --startup-project TaskManager.Api --context TaskManagerDbContext --configuration Release
cmd /c dotnet test TaskManager.Application.Tests\TaskManager.Application.Tests.csproj --configuration Release --no-restore
cmd /c dotnet test TaskManager.Api.IntegrationTests\TaskManager.Api.IntegrationTests.csproj --configuration Release --no-restore
cmd /c npm.cmd run build
```

The final validation results were:

- Application tests: 15 passing;
- API integration tests: 20 passing;
- Angular build passing;
- AppHost build passing;
- EF Core pending model changes check passing.

## What AI Helped With

AI was useful for:

- transforming the interview requirements into a complete implementation prompt;
- creating the initial Clean Architecture structure;
- generating repetitive project setup code faster;
- identifying duplicated repository logic;
- improving mapping reuse;
- reviewing controller HTTP response patterns;
- adding global exception handling;
- troubleshooting Aspire and PostgreSQL configuration;
- fixing EF Core migration snapshot issues;
- wiring the Angular frontend into Aspire;
- implementing the Kanban drag-and-drop UI;
- expanding unit and integration tests;
- using controlled bug insertion to validate test quality.

## My Role in the Process

My role was to guide the AI, review the generated code, identify architecture problems, ask for specific corrections, run the project, report real runtime errors, and validate the result with automated and manual checks.

Important examples of human review decisions were:

- noticing duplicated repository methods and requesting a base repository;
- questioning private `ToResponse` methods inside services because they would be copied across classes;
- requiring REST-style controller responses instead of returning service DTOs directly;
- clarifying that the app was running through Aspire, not directly through the API;
- asking whether the tests covered enough scenarios;
- validating the tests by intentionally introducing bugs and checking whether the test suite caught them.

This made the AI usage controlled and iterative instead of a one-shot code generation process.
