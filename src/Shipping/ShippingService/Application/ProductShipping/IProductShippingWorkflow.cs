using Temporalio.Workflows;

namespace ShippingService.Application.ProductShipping;

// impl. note - this can move to a contracts assembly so that other services don't need to take a dependency
//      on the worker service implementation itself

[Workflow]
public interface IProductShippingWorkflow
{
    [WorkflowRun]
    public Task ShipAsync(ProductShippingWorkflowArgs args);

    public static string WfId(int customerId, int purchaseOrderNumber) => $"product-shipping-workflow-{customerId}-{purchaseOrderNumber}";
}

public class ProductShippingWorkflowArgs
{
    public required int CustomerId { get; set; }
    public required int PurchaseOrderNumber { get; set; }
    public required ICollection<ShippingLineItem> ShippingLineItems { get; set; }
}

public class ShippingLineItem
{
    public required Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public required int Quantity { get; set; }
}