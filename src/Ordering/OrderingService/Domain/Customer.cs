namespace OrderingService.Domain;

public class Customer
{
    public required int CustomerId { get; set; }

    public IEnumerable<PurchaseOrder>? PurchaseOrders { get; set; }
}