using OrderingService.Application.SubmitPurchaseOrder;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

public interface IPurchaseOrderProcessorWorkflowClient
{
    Task<SubmitPurchaseOrderResponseTypes> SubmitAsync(SubmitPurchaseOrderCommand command);
}