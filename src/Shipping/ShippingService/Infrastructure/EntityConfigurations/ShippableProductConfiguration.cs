using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShippingService.Domain;

namespace ShippingService.Infrastructure.EntityConfigurations;

public class ShippableProductConfiguration : IEntityTypeConfiguration<ShippableProduct>
{
    public void Configure(EntityTypeBuilder<ShippableProduct> builder)
    {
        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.ProductName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(10).IsRequired();
        builder.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");

        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<ShippableProduct> builder)
    {
        builder.HasData(new ShippableProduct
        {
            ProductId = new Guid("6831ee62-b099-44e7-b3e2-d2cd045cc2f5"),
            ProductName = "The Girl on the Train",
            Sku = "TGOTT1",
            WeightKg = 0.50M,
        });
    }
}