using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.OpenApi;

public static class OpenApiHostingExtensions
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services, Assembly assembly)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opts =>
        {
            // include xml docs
            var xmlFilename = $"{assembly.GetName().Name}.xml";
            opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return services;
    }
}