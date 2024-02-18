using System.Reflection;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Polly;

using ShippingService.Application.ProductShipping;
using ShippingService.Infrastructure;
using ShippingService.Infrastructure.Repositories;
using ShippingService.Infrastructure.Services;
using ShippingService.Options;
using ShippingService.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults(x => x.AddSource(Otel.ActivitySource.Name));

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = true;
});

builder.Services.AddMassTransit(options =>
{
    options.SetKebabCaseEndpointNameFormatter();

    var entryAssembly = Assembly.GetEntryAssembly();

    options.AddConsumers(entryAssembly);
    options.AddActivities(entryAssembly);

    options.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], h =>
        {
            h.Password(builder.Configuration["RabbitMq:Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// infra layer
builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShippingDb"), np => np.EnableRetryOnFailure()));

builder.Services.AddSingleton<IEmailService, DebugEmailService>();
builder.Services.AddScoped<IShippableProductRepository, ShippableProductRepository>();
builder.Services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();

// options
builder.Services.AddOptions<ShippingOptions>()
    .Bind(builder.Configuration.GetSection(ShippingOptions.SectionName))
    .ValidateDataAnnotations();

var host = builder.Build();

var opts = host.Services.GetRequiredService<IOptions<ShippingOptions>>();

if (builder.Environment.IsDevelopment())
{
    // in a proper app I'd apply migrations in a pipeline, but this is fine for dev!
    var scope = host.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();

    var retry = Policy.Handle<Exception>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    await retry.ExecuteAsync(async () =>
    {
        await dbContext.Database.MigrateAsync();
    });
}

host.Run();