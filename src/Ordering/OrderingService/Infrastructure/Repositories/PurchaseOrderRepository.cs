using System.Diagnostics;

using Microsoft.EntityFrameworkCore;

using OrderingService.Application.PurchaseOrders;
using OrderingService.Domain;

namespace OrderingService.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly OrderingDbContext _context;
    public PurchaseOrderRepository(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task AddIfNotExistsAsync(PurchaseOrder purchaseOrder)
    {
        Activity.Current?.AddEvent(new(nameof(PurchaseOrderRepository) + nameof(AddIfNotExistsAsync)));

        if (await ExistsAsync(purchaseOrder.CustomerId, purchaseOrder.PurchaseOrderNumber))
        {
            return;
        }

        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int customerId, int purchaseOrderNumber)
    {
        Activity.Current?.AddEvent(new(nameof(PurchaseOrderRepository) + nameof(ExistsAsync)));

        return await _context.PurchaseOrders.AsNoTracking()
            .AnyAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber && po.CustomerId == customerId);
    }
}