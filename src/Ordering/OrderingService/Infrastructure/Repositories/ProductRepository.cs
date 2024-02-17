using System.Diagnostics;

using Microsoft.EntityFrameworkCore;

using OrderingService.Application.Products;
using OrderingService.Domain;

namespace OrderingService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderingDbContext _dbContext;
    public ProductRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetProductOrNullAsync(Guid id)
    {
        Activity.Current?.AddEvent(new(nameof(ProductRepository) + nameof(GetProductOrNullAsync)));

        return _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == id);
    }

    public async Task<ICollection<Product>> GetProductsByIdBulkAsync(IEnumerable<Guid> ids)
    {
        Activity.Current?.AddEvent(new(nameof(ProductRepository) + nameof(GetProductsByIdBulkAsync)));

        return await _dbContext.Products.AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .ToListAsync();
    }
}