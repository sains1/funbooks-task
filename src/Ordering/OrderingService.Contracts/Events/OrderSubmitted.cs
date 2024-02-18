namespace OrderingService.Contracts.Events;

public class OrderSubmitted
{
    public required int CustomerId { get; set; }
    public required int PurchaseOrderNumber { get; set; }
    public required ICollection<OrderLineItem> LineItems { get; set; }
}

public class OrderLineItem
{
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
    public required ProductType ProductType { get; set; }
}