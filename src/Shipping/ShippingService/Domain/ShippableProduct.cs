namespace ShippingService.Domain;

public class ShippableProduct
{
    public required Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string Sku { get; set; }
    public required decimal WeightKg { get; set; }
}