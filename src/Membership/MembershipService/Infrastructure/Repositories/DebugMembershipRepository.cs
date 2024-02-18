using MembershipService.Application.MembershipEnrollment;

namespace MembershipService.Infrastructure.Repositories;

public class DebugMembershipRepository(ILogger<DebugMembershipRepository> logger)
    : IMembershipRepository
{
    public async Task EnrollUserInMembership(int customerId, Guid productId)
    {
        // not implemented due to time constraints
        //      The implementation of this might just be a regular SQL call, or if the requirements of
        //      membership is to control authorization then I might have opted for a centralised permission
        //      service e.g. SpiceDB.
        using var activity = Otel.ActivitySource.StartActivity(nameof(EnrollUserInMembership));
        logger.LogInformation("Enrolling {customerId} in product {productId}", customerId, productId);
        await Task.Delay(30);
    }
}