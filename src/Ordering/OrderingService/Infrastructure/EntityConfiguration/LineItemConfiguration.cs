using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OrderingService.Domain;

namespace OrderingService.Infrastructure.EntityConfiguration;

public class LineItemConfiguration : IEntityTypeConfiguration<LineItem>
{
    public void Configure(EntityTypeBuilder<LineItem> builder)
    {
        builder.HasKey(li => new { li.CustomerId, PurchaseOrderId = li.PurchaseOrderNumber, li.ProductId });
        builder.Property(li => li.ProductId).IsRequired();
        builder.Property(li => li.Quantity).IsRequired();

        builder.HasOne(li => li.Product)
            .WithMany()
            .HasForeignKey(li => li.ProductId);

        builder.HasOne(po => po.PurchaseOrder)
            .WithMany(li => li.LineItems)
            .HasForeignKey(li => new { li.CustomerId, li.PurchaseOrderNumber });
    }
}