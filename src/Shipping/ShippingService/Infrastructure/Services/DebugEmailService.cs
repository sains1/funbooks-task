using ShippingService.Services;

namespace ShippingService.Infrastructure.Services;

public class DebugEmailService(ILogger<DebugEmailService> logger)
    : IEmailService
{
    public async Task Send(string email, string subject, string body)
    {
        // debug service that prints emails to console instead of sending them
        using var activity = Otel.ActivitySource.StartActivity(nameof(DebugEmailService));

        logger.LogInformation("sending email to {email} with subject {subject}\n{body}", email, subject, body);

        await Task.Delay(20);
    }
}