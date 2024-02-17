using OrderingService.Application.SubmitPurchaseOrder;

using Temporalio.Workflows;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

/// <summary>
/// Workflow for processing purchase orders
/// </summary>
[Workflow]
public interface IPurchaseOrderProcessorWorkflow
{
    [WorkflowRun]
    public Task<SubmitPurchaseOrderResponseTypes> SubmitAsync(SubmitPurchaseOrderCommand command);

    public static string GetId(int customerId, int purchaseOrderNumber) => $"purchase-order-processor-{customerId}-{purchaseOrderNumber}";
}


/// <summary>
/// Client interface for interacting with the IPurchaseOrderProcessorWorkflow
/// </summary>
public interface IPurchaseOrderProcessorWorkflowClient : IPurchaseOrderProcessorWorkflow
{
}