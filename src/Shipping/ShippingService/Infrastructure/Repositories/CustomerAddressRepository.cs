using Microsoft.EntityFrameworkCore;

using ShippingService.Application.ProductShipping;
using ShippingService.Domain;

namespace ShippingService.Infrastructure.Repositories;

public class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly ShippingDbContext context;
    public CustomerAddressRepository(ShippingDbContext context)
    {
        this.context = context;
    }

    public async Task<CustomerAddress> GetCustomerAddressAsync(int customerId)
    {
        using var activity = Otel.ActivitySource.StartActivity(nameof(GetCustomerAddressAsync));

        // note - no tests due to time constraints
        return await context.CustomerAddresses.FirstAsync(x => x.CustomerId == customerId);
    }
}