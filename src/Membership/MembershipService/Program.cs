using System.Reflection;

using MassTransit;

using MembershipService.Application.MembershipEnrollment;
using MembershipService.Infrastructure;
using MembershipService.Infrastructure.Repositories;

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

builder.Services.AddScoped<IMembershipRepository, DebugMembershipRepository>();

var host = builder.Build();
host.Run();