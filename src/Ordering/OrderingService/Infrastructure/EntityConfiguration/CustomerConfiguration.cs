using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OrderingService.Domain;

namespace OrderingService.Infrastructure.EntityConfiguration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId).ValueGeneratedOnAdd();

        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<Customer> builder)
    {
        // for demo purposes
        builder.HasData(
            new Customer
            {
                CustomerId = 12345,
            },
            new Customer
            {
                CustomerId = 56789,
            });
    }
}