using System.Reflection;

using FluentValidation;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using OrderingService.Application.Products;
using OrderingService.Application.Products.GetProduct;
using OrderingService.Application.PurchaseOrders;
using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Infrastructure;
using OrderingService.Infrastructure.Repositories;

using Polly;

using SharedKernel.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApiServices(Assembly.GetExecutingAssembly());
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// app handlers (this could use Assembly scanning, or a library like Mediatr)
builder.Services.AddScoped<GetProductHandler>();
builder.Services.AddScoped<SubmitPurchaseOrderHandler>();

// infra layer
builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderingDb"), np => np.EnableRetryOnFailure()));

// repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();

// messaging
builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = true;
});

builder.Services.AddMassTransit(options =>
{
    options.SetKebabCaseEndpointNameFormatter();

    var entryAssembly = Assembly.GetEntryAssembly();

    options.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    options.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<OrderingDbContext>(context);
    });

    options.AddConsumers(entryAssembly);
    options.AddActivities(entryAssembly);

    options.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", h =>
        {
            h.Password("rabbit");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

    var retry = Policy.Handle<Exception>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    await retry.ExecuteAsync(async () =>
    {
        await dbContext.Database.MigrateAsync();
    });
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();