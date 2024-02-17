using AutoFixture;
using AutoFixture.Xunit2;

using NSubstitute;

using OrderingService.Application.Products;
using OrderingService.Application.PurchaseOrders;
using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Domain;

namespace OrderingService.Tests.Application.PurchaseOrders.SubmitPurchaseOrder;

public class PurchaseOrderProcessorActivitiesTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();

    private readonly PurchaseOrderProcessorActivities _sut;
    public PurchaseOrderProcessorActivitiesTests()
    {
        _sut = new PurchaseOrderProcessorActivities(_purchaseOrderRepository, _productRepository);
    }

    [Theory, AutoData]
    public async Task CheckPurchaseOrderExistsAsync_WhenPurchaseOrderExists_ShouldReturnTrue(CheckPurchaseOrderExistsArgs args)
    {
        // arrange
        _purchaseOrderRepository.ExistsAsync(args.CustomerId, args.PurchaseOrderId).Returns(true);

        // act
        var res = await _sut.CheckPurchaseOrderExistsAsync(args);

        // assert
        Assert.True(res);
    }

    [Theory, AutoData]
    public async Task CheckPurchaseOrderExistsAsync_WhenPurchaseOrderNotExists_ShouldReturnFalse(CheckPurchaseOrderExistsArgs args)
    {
        // arrange
        _purchaseOrderRepository.ExistsAsync(args.CustomerId, args.PurchaseOrderId).Returns(false);

        // act
        var res = await _sut.CheckPurchaseOrderExistsAsync(args);

        // assert
        Assert.False(res);
    }


    [Theory, AutoData]
    public async Task GetPurchaseOrderProductInformation_ShouldReturnProductInformation(GetPurchaseOrderProductInformation args,
        ICollection<Product> products)
    {
        // arrange
        _productRepository.GetProductsByIdBulkAsync(Arg.Any<IEnumerable<Guid>>()).Returns(products);

        // act
        var res = await _sut.GetPurchaseOrderProductInformation(args);

        // assert
        Assert.Equivalent(products, res);
    }

    [Theory, AutoData]
    public async Task SavePurchaseOrderAsync_ShouldBeInvokedCorrectly(Fixture fixture)
    {
        // arrange
        var po = fixture.Build<PurchaseOrder>()
            .Without(x => x.Customer)
            .Without(x => x.LineItems)
            .Create();

        // act
        await _sut.SavePurchaseOrderAsync(po);

        // assert
        await _purchaseOrderRepository.Received(1).AddIfNotExistsAsync(po);
    }
}