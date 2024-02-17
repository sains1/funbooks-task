using FluentValidation.Results;

using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;

using SharedKernel.GenericResponses;
using SharedKernel.Temporal;

using Temporalio.Client;
using Temporalio.Exceptions;

namespace OrderingService.Infrastructure.WorkflowInvocations;

public class PurchaseOrderProcessorWorkflowClient(ITemporalClient client) : IPurchaseOrderProcessorWorkflowClient
{
    public async Task<SubmitPurchaseOrderResponseTypes> SubmitAsync(SubmitPurchaseOrderCommand command)
    {
        try
        {
            return await client.ExecuteWorkflowAsync((PurchaseOrderProcessorWorkflow wf) => wf.SubmitAsync(command), new WorkflowOptions
            {
                Id = PurchaseOrderProcessorWorkflow.GetId(command.CustomerId, command.PurchaseOrderNumber),
                TaskQueue = TemporalConstants.OrderingServiceTaskQueue
            });
        }
        catch (WorkflowFailedException ex) when (ex.InnerException is ApplicationFailureException inner)
        {
            return inner.ErrorType switch
            {
                PurchaseOrderProcessorWorkflow.PurchaseOrderConflict => ConflictResponse.Value,
                PurchaseOrderProcessorWorkflow.MissingProduct => new MissingProductResponse(),
                _ => throw inner
            };
        }
    }
}