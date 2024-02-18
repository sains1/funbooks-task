using ShippingService.Domain;

namespace ShippingService.Application.ProductShipping;

public interface IShippableProductRepository
{
    Task<IEnumerable<ShippableProduct>> GetShippableProductsBulkAsync(IEnumerable<Guid> productIds);
}