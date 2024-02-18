using System.Diagnostics;

using ShippingService.Services;

namespace ShippingService.Infrastructure.Services;

// debug service that prints emails to console instead of sending them
public class DebugEmailService(ILogger<DebugEmailService> logger): IEmailService
{
    public Task Send(string email, string subject, string body)
    {
        Activity.Current?.AddEvent(new(nameof(DebugEmailService)));

        logger.LogInformation("sending email to {email} with subject {subject}\n{body}", email, subject, body);

        return Task.CompletedTask;
    }
}