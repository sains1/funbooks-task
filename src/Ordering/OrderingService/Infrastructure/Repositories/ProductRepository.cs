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
}