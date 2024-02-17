using SharedKernel.Temporal;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddConfiguredTemporalClient(builder.Configuration);
builder.Services.AddHostedTemporalWorker(TemporalConstants.ShippingServiceTaskQueue);

var host = builder.Build();
host.Run();
