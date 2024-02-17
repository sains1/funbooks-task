var builder = DistributedApplication.CreateBuilder(args);

var slnDirectory = GetSolutionDirectory(Directory.GetCurrentDirectory());
ArgumentException.ThrowIfNullOrEmpty(slnDirectory);

builder.AddExecutable("temporal", "temporal", Directory.GetCurrentDirectory(), ["server", "start-dev", "--db-filename", slnDirectory + ".sqlite"]);

var db = builder.AddPostgresContainer("db", 5432, "postgres")
    .AddDatabase("funbooks");

builder.AddProject<Projects.OrderingService>("orderingservice")
    .WithReference(db);

builder.AddProject<Projects.ShippingService>("shippingservice");

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