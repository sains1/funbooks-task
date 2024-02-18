var builder = DistributedApplication.CreateBuilder(args);

var slnDirectory = GetSolutionDirectory(Directory.GetCurrentDirectory());
ArgumentException.ThrowIfNullOrEmpty(slnDirectory);

var messaging = builder.AddRabbitMQContainer("rabbitmq", port: 5672, password: "rabbit");

var db = builder.AddPostgresContainer("db", 5432, "postgres")
    .AddDatabase("funbooks");

builder.AddProject<Projects.OrderingService>("orderingservice")
    .WithReference(messaging)
    .WithReference(db);

builder.Build().Run();

// recurse up the file tree until we find the solution file
static string? GetSolutionDirectory(string directoryPath)
{
    string[] solutionFiles = Directory.GetFiles(directoryPath, "*.sln");
    if (solutionFiles.Length > 0)
    {
        return solutionFiles[0];
    }

    string? parentDirectory = Directory.GetParent(directoryPath)?.FullName;
    if (parentDirectory != null)
    {
        return GetSolutionDirectory(parentDirectory);
    }

    return null;
}