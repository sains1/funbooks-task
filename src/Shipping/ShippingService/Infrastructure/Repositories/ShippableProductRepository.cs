using Microsoft.EntityFrameworkCore;

using ShippingService.Application.ProductShipping;
using ShippingService.Domain;

namespace ShippingService.Infrastructure.Repositories;

public class ShippableProductRepository : IShippableProductRepository
{
    private readonly ShippingDbContext context;
    public ShippableProductRepository(ShippingDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<ShippableProduct>> GetShippableProductsBulkAsync(IEnumerable<Guid> productIds)
    {
        using var activity = Otel.ActivitySource.StartActivity(nameof(GetShippableProductsBulkAsync));

        // no tests due to time constraints
        return await context.ShippableProducts.Where(x => productIds.Contains(x.ProductId)).ToListAsync();
    }
}