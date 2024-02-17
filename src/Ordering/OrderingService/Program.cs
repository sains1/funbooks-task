using System.Reflection;

using Microsoft.EntityFrameworkCore;

using OrderingService.Application.Products;
using OrderingService.Application.Products.GetProduct;
using OrderingService.Infrastructure;
using OrderingService.Infrastructure.Repositories;

using SharedKernel.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApiServices(Assembly.GetExecutingAssembly());
builder.Services.AddControllers();

// app handlers (this could use Assembly scanning, or a library like Mediatr)
builder.Services.AddScoped<GetProductHandler>();

// data layer
builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderingDb"), np => np.EnableRetryOnFailure()));

builder.Services.AddScoped<IProductRepository, ProductRepository>();

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