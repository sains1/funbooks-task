using Microsoft.EntityFrameworkCore;

using OrderingService.Application.PurchaseOrders;
using OrderingService.Domain;

namespace OrderingService.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly OrderingDbContext context;

    public PurchaseOrderRepository(OrderingDbContext context)
    {
        this.context = context;
    }

    public async Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder)
    {
        using var activity = Otel.ActivitySource.StartActivity(nameof(AddPurchaseOrderAsync));

        context.PurchaseOrders.Add(purchaseOrder);

        await context.SaveChangesAsync();
    }

    public async Task<bool> PurchaseOrderExistsAsync(int customerId, int purchaseOrderNumber)
    {
        using var activity = Otel.ActivitySource.StartActivity(nameof(PurchaseOrderExistsAsync));

        return await context.PurchaseOrders.AsNoTracking()
            .AnyAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber && po.CustomerId == customerId);
    }
}