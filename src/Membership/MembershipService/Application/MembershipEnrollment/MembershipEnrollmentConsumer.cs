using MassTransit;

using OrderingService.Contracts.Events;

namespace MembershipService.Application.MembershipEnrollment;

public class MembershipEnrollmentConsumer(IMembershipRepository repository)
    : IConsumer<OrderSubmitted>
{
    public async Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var membershipProducts = context.Message.LineItems.Where(x => x.ProductType == ProductType.Membership).ToList();
        if (!membershipProducts.Any())
        {
            return; // nothing to do
        }

        foreach (var membershipProduct in membershipProducts)
        {
            await repository.EnrollUserInMembership(context.Message.CustomerId, membershipProduct.ProductId);
        }
    }
}