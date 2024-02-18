using System.Diagnostics;

using MassTransit;
using MassTransit.Configuration;

namespace SharedKernel.MassTransit;

public static class MassTransitMiddlewareExtensions
{
    public static void UseExceptionLogger<T>(this IPipeConfigurator<T> configurator, ActivitySource source)
        where T : class, PipeContext
    {
        configurator.AddPipeSpecification(new ExceptionLoggerSpecification<T>(source));
    }
}

public class ExceptionLoggerSpecification<T>(ActivitySource source):
    IPipeSpecification<T>
    where T : class, PipeContext
{
    public IEnumerable<ValidationResult> Validate()
    {
        return Enumerable.Empty<ValidationResult>();
    }

    public void Apply(IPipeBuilder<T> builder)
    {
        builder.AddFilter(new ExceptionLoggerFilter<T>(source));
    }
}

public class ExceptionLoggerFilter<T>(ActivitySource source):
    IFilter<T>
    where T : class, PipeContext
{
    public void Probe(ProbeContext context)
    {
    }

    public async Task Send(T context, IPipe<T> next)
    {
        source.StartActivity();
        await next.Send(context);
    }
}