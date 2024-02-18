using System.Diagnostics;

namespace MembershipService.Infrastructure;

public static class Otel
{
    public static ActivitySource ActivitySource = new ActivitySource("MembershipService");
}