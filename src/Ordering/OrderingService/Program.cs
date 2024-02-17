using System.Reflection;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using OrderingService.Application.Products;
using OrderingService.Application.Products.GetProduct;
using OrderingService.Application.PurchaseOrders;
using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Infrastructure;
using OrderingService.Infrastructure.Repositories;
using OrderingService.Infrastructure.WorkflowInvocations;

using SharedKernel.OpenApi;
using SharedKernel.Temporal;

using Temporalio.Extensions.Hosting;

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

// workflow clients
builder.Services.AddScoped<IPurchaseOrderProcessorWorkflowClient, PurchaseOrderProcessorWorkflowClient>();

// temporal
builder.Services.AddConfiguredTemporalClient(builder.Configuration);
builder.Services.AddHostedTemporalWorker(TemporalConstants.OrderingServiceTaskQueue)
    .AddWorkflow<PurchaseOrderProcessorWorkflow>()
    .AddScopedActivities<PurchaseOrderProcessorActivities>();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();