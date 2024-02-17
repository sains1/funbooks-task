using OrderingService.Application.Products;
using OrderingService.Domain;

using Temporalio.Activities;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

public class PurchaseOrderProcessorActivities(IPurchaseOrderRepository repository, IProductRepository productRepository)
{
    [Activity]
    public async Task<bool> CheckPurchaseOrderExistsAsync(CheckPurchaseOrderExistsArgs args)
    {
        return await repository.ExistsAsync(args.CustomerId, args.PurchaseOrderId);
    }

    [Activity]
    public async Task<ICollection<Product>> GetPurchaseOrderProductInformation(GetPurchaseOrderProductInformation args)
    {
        // could also check the stock here if we had it!

        return await productRepository.GetProductsByIdBulkAsync(args.Products.Select(x => x.ProductId));
    }

    [Activity]
    public async Task SavePurchaseOrderAsync(PurchaseOrder purchaseOrder)
    {
        // note - we need this to remain idempotent, in the unlikely event that saving the purchase order succeeds, but saving the result
        //      to temporal fails causing a retry of the activity
        await repository.AddIfNotExistsAsync(purchaseOrder);
    }
}


public record CheckPurchaseOrderExistsArgs(int CustomerId, int PurchaseOrderId);

public record GetPurchaseOrderProductInformation(ICollection<SubmitPurchaseOrderCommand.ProductRequest> Products);