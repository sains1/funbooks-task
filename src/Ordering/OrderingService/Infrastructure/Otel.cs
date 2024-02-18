using System.Diagnostics;

namespace OrderingService.Infrastructure;

public static class Otel
{
    public static ActivitySource ActivitySource = new ActivitySource("OrderingService");
}