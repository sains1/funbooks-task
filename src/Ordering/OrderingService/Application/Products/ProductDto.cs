using OrderingService.Domain;

namespace OrderingService.Application.Products;

public class ProductDto
{
    public required Guid ProductId { get; set; }
    public required decimal Price { get; set; }
    public required string ProductName { get; set; }

    public static ProductDto FromProduct(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            Price = product.Price,
            ProductName = product.ProductName,
        };
    }
}