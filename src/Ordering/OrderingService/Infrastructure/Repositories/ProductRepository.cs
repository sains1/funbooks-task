using Microsoft.EntityFrameworkCore;

using OrderingService.Application.Products;
using OrderingService.Domain;

namespace OrderingService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderingDbContext dbContext;

    public ProductRepository(OrderingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Product?> GetProductOrNullAsync(Guid id)
    {
        using var activity = Otel.ActivitySource.StartActivity(nameof(GetProductOrNullAsync));

        return dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == id);
    }

    public async Task<ICollection<Product>> GetProductsByIdBulkAsync(IEnumerable<Guid> ids)
    {
        using var activity = Otel.ActivitySource.StartActivity(nameof(GetProductsByIdBulkAsync));

        return await dbContext.Products.AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .ToListAsync();
    }
}