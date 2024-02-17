var builder = DistributedApplication.CreateBuilder(args);


var db = builder.AddPostgresContainer("db", 5432, "postgres")
    .AddDatabase("funbooks");

builder.AddProject<Projects.OrderingService>("orderingservice")
    .WithReference(db);

builder.Build().Run();