using System.Diagnostics;

namespace ShippingService.Infrastructure;

public static class Otel
{
    public static ActivitySource ActivitySource = new ActivitySource("ShippingService");
}