using OrderingService.Domain;

namespace OrderingService.Application.PurchaseOrders;

public interface IPurchaseOrderRepository
{
    Task<bool> PurchaseOrderExistsAsync(int customerId, int purchaseOrderNumber);

    Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder);
}