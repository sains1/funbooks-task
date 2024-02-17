using System.Diagnostics;

namespace OrderingService.Application.Products.GetProduct;

public class GetProductHandler(IProductRepository repository)
{
    public async Task<ProductDto?> GetProductAsync(GetProductQuery query)
    {
        Activity.Current?.AddEvent(new(nameof(GetProductHandler)));

        var result = await repository.GetProductOrNullAsync(query.ProductId);

        if (result is null)
        {
            return null;
        }

        return ProductDto.FromProduct(result);
    }
}