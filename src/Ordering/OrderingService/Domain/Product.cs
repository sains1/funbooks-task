namespace OrderingService.Domain;

public class Product
{
    public required Guid ProductId { get; set; }
    public required decimal Price { get; set; }
    public required string ProductName { get; set; }
    public required ProductType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}