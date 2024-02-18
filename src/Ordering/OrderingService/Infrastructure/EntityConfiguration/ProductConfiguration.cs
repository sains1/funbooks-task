using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OrderingService.Contracts.Events;
using OrderingService.Domain;

namespace OrderingService.Infrastructure.EntityConfiguration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).ValueGeneratedOnAdd();
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ProductName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Type).IsRequired();
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<Product> builder)
    {
        // for demo purposes
        builder.HasData(
            new Product
            {
                ProductId = new Guid("6831ee62-b099-44e7-b3e2-d2cd045cc2f5"),
                ProductName = "The Girl on the Train",
                Price = 9.99m,
                Type = ProductType.Physical
            },
            new Product
            {
                ProductId = new Guid("1d217f91-bef1-4eb6-ada8-d9d36739c03e"),
                ProductName = "Comprehensive First Aid Training",
                Price = 19.99m,
                Type = ProductType.Digital
            },
            new Product
            {
                ProductId = new Guid("3ea5f11d-c4ee-4f08-bdde-82559c7bd0af"),
                ProductName = "Book Club",
                Price = 29.99m,
                Type = ProductType.Membership
            }
        );
    }
}