using FluentValidation;

using OrderingService.Application.SubmitPurchaseOrder;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

public class SubmitPurchaseOrderHandler(IValidator<SubmitPurchaseOrderCommand> validator, IPurchaseOrderProcessorWorkflowClient workflowClient)
{
    public async Task<SubmitPurchaseOrderResponseTypes> Handle(SubmitPurchaseOrderCommand command)
    {
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
        {
            return result;
        }

        return await workflowClient.SubmitAsync(command);
    }
}