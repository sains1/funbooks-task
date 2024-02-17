using AutoFixture;
using AutoFixture.Xunit2;

using Microsoft.EntityFrameworkCore;

using OrderingService.Domain;
using OrderingService.Infrastructure.Repositories;
using OrderingService.Tests.Setup;

using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

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
        var (customer, po) = CreateTestOrder(fixture);

        var dbContext = _fixture.GetScopedDbContext();

        dbContext.AddRange(customer, po);

        await dbContext.SaveChangesAsync();

        // act
        var result = await _sut.ExistsAsync(po.CustomerId, po.PurchaseOrderNumber);

        // assert
        Assert.True(result);
    }

    [Theory, AutoData]
    public async Task ExistsAsync_WhenPurchaseOrderExistsForADifferentCustomer_ShouldReturnFalse(Fixture fixture)
    {
        // arrange
        var (customer, po) = CreateTestOrder(fixture);

        var dbContext = _fixture.GetScopedDbContext();

        dbContext.AddRange(customer, po);

        await dbContext.SaveChangesAsync();

        // act
        var result = await _sut.ExistsAsync(fixture.Create<int>(), po.PurchaseOrderNumber);

        // assert
        Assert.False(result);
    }

    [Theory, AutoData]
    public async Task ExistsAsync_WhenPurchaseOrderExists_ShouldReturnFalse(Fixture fixture)
    {
        // arrange

        // act
        var result = await _sut.ExistsAsync(fixture.Create<int>(), fixture.Create<int>());

        // assert
        Assert.False(result);
    }


    [Theory, AutoData]
    public async Task AddIfNotExistsAsync_WhenPurchaseOrderNotExists_ShouldAddNewPurchaseOrder(Fixture fixture)
    {
        // arrange
        var dbContext = _fixture.GetScopedDbContext();
        var (customer, po) = CreateTestOrder(fixture);

        // act
        await _sut.AddIfNotExistsAsync(po);

        // assert
        var count = await dbContext.PurchaseOrders.CountAsync();
        Assert.Equal(1, count);
    }

    [Theory, AutoData]
    public async Task AddIfNotExistsAsync_WhenPurchaseOrderExists_ShouldNotAddPurchaseOrder(Fixture fixture)
    {
        // arrange
        var dbContext = _fixture.GetScopedDbContext();
        var (customer, po) = CreateTestOrder(fixture);

        dbContext.AddRange(customer, po);

        await dbContext.SaveChangesAsync();

        // act
        await _sut.AddIfNotExistsAsync(po);

        // assert
        var count = await dbContext.PurchaseOrders.CountAsync();
        Assert.Equal(1, count);
    }

    private (Customer, PurchaseOrder) CreateTestOrder(Fixture fixture)
    {
        var customer = new Customer { CustomerId = fixture.Create<int>() };
        var po = new PurchaseOrder
        {
            CustomerId = customer.CustomerId,
            Customer = customer,
            PurchaseOrderNumber = fixture.Create<int>(),
            TotalCost = fixture.Create<decimal>()
        };

        return (customer, po);
    }



    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() => _fixture.ResetAsync();
}