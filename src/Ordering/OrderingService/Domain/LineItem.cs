namespace OrderingService.Domain;

public class LineItem
{
    public required int CustomerId { get; set; }
    public required int PurchaseOrderNumber { get; set; }
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }

    public Product? Product { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
}