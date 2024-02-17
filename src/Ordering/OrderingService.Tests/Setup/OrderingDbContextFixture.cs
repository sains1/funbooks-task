using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OrderingService.Infrastructure;

using Respawn;

using Testcontainers.PostgreSql;

namespace OrderingService.Tests.Setup;

public class OrderingDbContextFixture : IAsyncLifetime
{
    private IServiceProvider? _serviceProvider;
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        await _postgresContainer.StartAsync();

        services.AddDbContext<OrderingDbContext>(opts => opts.UseNpgsql(_postgresContainer.GetConnectionString(), np => np.EnableRetryOnFailure()));

        _serviceProvider = services.BuildServiceProvider();

        var context = _serviceProvider.GetRequiredService<OrderingDbContext>();

        await context.Database.MigrateAsync();
    }

    public OrderingDbContext GetScopedDbContext()
    {
        ArgumentNullException.ThrowIfNull(_serviceProvider);

        var scope = _serviceProvider.CreateScope();

        return scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    }

    public async Task ResetAsync()
    {
        ArgumentNullException.ThrowIfNull(_serviceProvider);

        var context = _serviceProvider.GetRequiredService<OrderingDbContext>();

        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var reset = await Respawner.CreateAsync(connection,
           new RespawnerOptions
           {
               SchemasToInclude = ["ordering"],
               DbAdapter = DbAdapter.Postgres,
           });

        await reset.ResetAsync(connection);
    }

    public Task DisposeAsync()
    {
        return _postgresContainer.StopAsync();
    }
}