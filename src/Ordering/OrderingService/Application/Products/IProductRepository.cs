using OrderingService.Domain;

namespace OrderingService.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetProductOrNullAsync(Guid id);
}