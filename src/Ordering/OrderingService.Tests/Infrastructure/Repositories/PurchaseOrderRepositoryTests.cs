using AutoFixture;
using AutoFixture.Xunit2;

using Microsoft.EntityFrameworkCore;

using OrderingService.Domain;
using OrderingService.Infrastructure.Repositories;
using OrderingService.Tests.Setup;

namespace OrderingService.Tests.Infrastructure.Repositories;

public class PurchaseOrderRepositoryTests : IAsyncLifetime, IClassFixture<OrderingDbContextFixture>
{
    private readonly PurchaseOrderRepository _sut;
    private readonly OrderingDbContextFixture _fixture;

    public PurchaseOrderRepositoryTests(OrderingDbContextFixture fixture)
    {
        _sut = new PurchaseOrderRepository(fixture.GetScopedDbContext());
        _fixture = fixture;
    }

    [Theory, AutoData]
    public async Task ExistsAsync_WhenPurchaseOrderExists_ShouldReturnTrue(Fixture fixture)
    {
        // arrange
        var (customer, po, product) = CreateTestOrder(fixture);

        var dbContext = _fixture.GetScopedDbContext();

        dbContext.AddRange(customer, po, product);

        await dbContext.SaveChangesAsync();

        // act
        var result = await _sut.PurchaseOrderExistsAsync(po.CustomerId, po.PurchaseOrderNumber);

        // assert
        Assert.True(result);
    }

    [Theory, AutoData]
    public async Task ExistsAsync_WhenPurchaseOrderExistsForADifferentCustomer_ShouldReturnFalse(Fixture fixture)
    {
        // arrange
        var (customer, po, product) = CreateTestOrder(fixture);

        var dbContext = _fixture.GetScopedDbContext();

        dbContext.AddRange(customer, po, product);

        await dbContext.SaveChangesAsync();

        // act
        var result = await _sut.PurchaseOrderExistsAsync(fixture.Create<int>(), po.PurchaseOrderNumber);

        // assert
        Assert.False(result);
    }

    [Theory, AutoData]
    public async Task ExistsAsync_WhenPurchaseOrderExists_ShouldReturnFalse(Fixture fixture)
    {
        // arrange

        // act
        var result = await _sut.PurchaseOrderExistsAsync(fixture.Create<int>(), fixture.Create<int>());

        // assert
        Assert.False(result);
    }


    [Theory, AutoData]
    public async Task AddPurchaseOrderAsync_WhenPurchaseOrderNotExists_ShouldAddNewPurchaseOrder(Fixture fixture)
    {
        // arrange
        var dbContext = _fixture.GetScopedDbContext();
        var (customer, po, product) = CreateTestOrder(fixture);

        dbContext.AddRange(customer, product);
        await dbContext.SaveChangesAsync();

        var count = await dbContext.PurchaseOrders.CountAsync();
        Assert.Equal(0, count); // ensure we were initially at 0

        // act
        await _sut.AddPurchaseOrderAsync(po);

        // assert
        count = await dbContext.PurchaseOrders.CountAsync();
        Assert.Equal(1, count);
    }

    private (Customer, PurchaseOrder, Product) CreateTestOrder(Fixture fixture)
    {
        var customer = new Customer { CustomerId = fixture.Create<int>() };
        var product = fixture.Build<Product>().Without(x => x.CreatedAt).Create();
        var id = fixture.Create<int>();
        var po = new PurchaseOrder
        {
            CustomerId = customer.CustomerId,
            PurchaseOrderNumber = id,
            TotalCost = fixture.Create<decimal>(),
            LineItems = new List<LineItem>
            {
                new LineItem
                {
                    CustomerId = customer.CustomerId,
                    ProductId = product.ProductId,
                    PurchaseOrderNumber= id,
                    Quantity = fixture.Create<int>()
                }
            }
        };

        return (customer, po, product);
    }



    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() => _fixture.ResetAsync();
}