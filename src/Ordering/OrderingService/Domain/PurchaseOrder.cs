namespace OrderingService.Domain;

public class PurchaseOrder
{
    public required int CustomerId { get; set; }

    public required int PurchaseOrderNumber { get; set; }

    public required decimal TotalCost { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<LineItem>? LineItems { get; set; }
}