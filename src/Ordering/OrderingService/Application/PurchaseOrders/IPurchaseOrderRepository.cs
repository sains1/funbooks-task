using OrderingService.Domain;

namespace OrderingService.Application.PurchaseOrders;

public interface IPurchaseOrderRepository
{
    Task<bool> ExistsAsync(int customerId, int purchaseOrderNumber);
    Task AddIfNotExistsAsync(PurchaseOrder purchaseOrder);
}