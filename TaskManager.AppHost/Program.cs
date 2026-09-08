var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var cache = builder.AddRedis("cache");

var database = postgres.AddDatabase("taskmanagerdb", "taskmanager");

var api = builder.AddProject<Projects.TaskManager_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddExecutable("client", "cmd", "../TaskManager.Client", "/c", "npm.cmd", "run", "start:aspire")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT");

builder.Build().Run();
