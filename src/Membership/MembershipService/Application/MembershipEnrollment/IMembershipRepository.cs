namespace MembershipService.Application.MembershipEnrollment;

public interface IMembershipRepository
{
    Task EnrollUserInMembership(int customerId, Guid productId);
}