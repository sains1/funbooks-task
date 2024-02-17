using OrderingService.Application.SubmitPurchaseOrder;
using OrderingService.Domain;

using SharedKernel.Temporal;

using ShippingService.Application.ProductShipping;

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


        // Business rule 1: if there are any physical products, we need to ship them
        if (products.Any(x => x.Type == ProductType.Physical))
        {
            var quantities = command.LineItems.ToDictionary(x => x.ProductId, x => x.Quantity);
            await Workflow.StartChildWorkflowAsync(
                (IProductShippingWorkflow wf) => wf.ShipAsync(new ProductShippingWorkflowArgs
                {
                    CustomerId = command.CustomerId,
                    PurchaseOrderNumber = command.PurchaseOrderNumber,
                    ShippingLineItems =
                        products.Select(x => new ShippingLineItem { ProductId = x.ProductId, ProductName = x.ProductName, Quantity = quantities[x.ProductId] }).ToList(),
                }),
                new ChildWorkflowOptions
                {
                    Id = IProductShippingWorkflow.WfId(command.CustomerId, command.PurchaseOrderNumber),
                    TaskQueue = TemporalConstants.ShippingServiceTaskQueue,
                    ParentClosePolicy = ParentClosePolicy.Abandon // shipping runs async, no need to wait
                });
        }


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