using System.Diagnostics;

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
        Activity.Current?.AddEvent(new (nameof(PurchaseOrderRepository) + nameof(AddPurchaseOrderAsync)));

        context.PurchaseOrders.Add(purchaseOrder);

        await context.SaveChangesAsync();
    }

    public async Task<bool> PurchaseOrderExistsAsync(int customerId, int purchaseOrderNumber)
    {
        Activity.Current?.AddEvent(new (nameof(PurchaseOrderRepository) + nameof(PurchaseOrderExistsAsync)));

        return await context.PurchaseOrders.AsNoTracking()
            .AnyAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber && po.CustomerId == customerId);
    }
}