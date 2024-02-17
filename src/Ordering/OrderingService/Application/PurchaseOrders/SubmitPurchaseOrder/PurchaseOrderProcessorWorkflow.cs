using OrderingService.Application.SubmitPurchaseOrder;
using OrderingService.Domain;

using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

[Workflow]
public class PurchaseOrderProcessorWorkflow
{

    [WorkflowRun]
    public async Task<SubmitPurchaseOrderSuccess> SubmitAsync(SubmitPurchaseOrderCommand command)
    {
        // check if the purchase order already exists
        var exists = await Workflow.ExecuteActivityAsync((PurchaseOrderProcessorActivities act) =>
            act.CheckPurchaseOrderExistsAsync(new CheckPurchaseOrderExistsArgs(command.CustomerId, command.PurchaseOrderNumber)), new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10)
            });

        if (exists)
        {
            throw new ApplicationFailureException("Purchase order already exists", nonRetryable: true, errorType: PurchaseOrderConflict);
        }

        // check all the products are available
        var products = await Workflow.ExecuteActivityAsync((PurchaseOrderProcessorActivities act) =>
            act.GetPurchaseOrderProductInformation(new GetPurchaseOrderProductInformation(command.LineItems)), new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10)
            });

        if (products.Count < command.LineItems.Count)
        {
            throw new ApplicationFailureException("One or more products not available", nonRetryable: true, errorType: MissingProduct);
        }

        // upsert the purchase order
        var purchaseOrder = new PurchaseOrder
        {
            CustomerId = command.CustomerId,
            PurchaseOrderNumber = command.PurchaseOrderNumber,
            TotalCost = products.Select(x => x.Price).Sum()
        };

        await Workflow.ExecuteActivityAsync((PurchaseOrderProcessorActivities act) =>
            act.SavePurchaseOrderAsync(purchaseOrder), new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10)
            });


        return new SubmitPurchaseOrderSuccess
        {
            CustomerId = command.CustomerId,
            PurchaseOrderNumber = command.PurchaseOrderNumber,
            TotalPrice = purchaseOrder.TotalCost
        };
    }

    public static string GetId(int customerId, int purchaseOrderNumber) => $"purchase-order-processor-{customerId}-{purchaseOrderNumber}";

    public const string PurchaseOrderConflict = nameof(PurchaseOrderConflict);
    public const string MissingProduct = nameof(MissingProduct);
}