using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;

using SharedKernel.Temporal;

using Temporalio.Client;

namespace OrderingService.Infrastructure.WorkflowInvocations;

public class PurchaseOrderProcessorWorkflowClient(ITemporalClient client) : IPurchaseOrderProcessorWorkflowClient
{
    public Task<SubmitPurchaseOrderResponseTypes> SubmitAsync(SubmitPurchaseOrderCommand command)
    {
        return client.ExecuteWorkflowAsync((IPurchaseOrderProcessorWorkflow wf) => wf.SubmitAsync(command), new WorkflowOptions
        {
            Id = IPurchaseOrderProcessorWorkflow.GetId(command.CustomerId, command.PurchaseOrderNumber),
            TaskQueue = TemporalConstants.OrderingServiceTaskQueue
        });
    }
}