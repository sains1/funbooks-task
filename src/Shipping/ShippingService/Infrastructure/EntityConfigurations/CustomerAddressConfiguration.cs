using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShippingService.Domain;

namespace ShippingService.Infrastructure.EntityConfigurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.AddressLine1).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(10).IsRequired();

        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasData(
            new CustomerAddress
            {
                CustomerId = 12345,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 101",
                PostCode = "SW1 1AA",
            },
            new CustomerAddress
            {
                CustomerId = 56789,
                AddressLine1 = "789 Oak St",
                AddressLine2 = "Unit 303",
                PostCode = "AB0 2XY",
            });
    }
}