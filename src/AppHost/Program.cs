using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var slnDirectory = GetSolutionDirectory(Directory.GetCurrentDirectory());
ArgumentException.ThrowIfNullOrEmpty(slnDirectory);

builder.AddExecutable("temporal", "temporal", Directory.GetCurrentDirectory(), ["server", "start-dev", "--db-filename", slnDirectory + ".sqlite"]);

var db = builder.AddPostgresContainer("db", 5432, "postgres")
    .AddDatabase("funbooks");

builder.AddProject<Projects.OrderingService>("orderingservice")
    .WithReference(db);

builder.Build().Run();

static string? GetSolutionDirectory(string directoryPath)
{
    // Check if the current directory contains a solution file
    string[] solutionFiles = Directory.GetFiles(directoryPath, "*.sln");
    if (solutionFiles.Length > 0)
    {
        return solutionFiles[0]; // Return the first solution file found
    }

    // If not, move up to the parent directory
    string? parentDirectory = Directory.GetParent(directoryPath)?.FullName;
    if (parentDirectory != null)
    {
        return GetSolutionDirectory(parentDirectory); // Recurse up the tree
    }

    // If no solution file is found in any parent directory, return null
    return null;
}