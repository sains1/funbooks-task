using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OrderingService.Domain;

namespace OrderingService.Infrastructure.EntityConfiguration;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(p => new { p.CustomerId, p.PurchaseOrderNumber });

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TotalCost)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.CustomerId);
    }
}