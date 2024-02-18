using Microsoft.EntityFrameworkCore;

using ShippingService.Domain;

namespace ShippingService.Infrastructure;

public class ShippingDbContext(DbContextOptions<ShippingDbContext> options)
    : DbContext(options)
{
    public DbSet<ShippableProduct> ShippableProducts => Set<ShippableProduct>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shipping");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShippingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}