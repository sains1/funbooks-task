using Microsoft.EntityFrameworkCore;

using OrderingService.Domain;

namespace OrderingService.Infrastructure;

public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<LineItem> LineItems => Set<LineItem>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ordering");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}