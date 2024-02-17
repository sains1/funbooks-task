using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Temporal;

public static class TemporalHostingExtensions
{
    public static IServiceCollection AddConfiguredTemporalClient(this IServiceCollection services, IConfiguration configuration)
    {
        var ns = configuration["Temporal:Namespace"];
        var host = configuration["Temporal:TargetHost"];

        ArgumentException.ThrowIfNullOrEmpty(ns);
        ArgumentException.ThrowIfNullOrEmpty(host);

        services.AddTemporalClient(opts =>
        {
            opts.Namespace = ns;
            opts.TargetHost = host;
        });

        return services;
    }
}