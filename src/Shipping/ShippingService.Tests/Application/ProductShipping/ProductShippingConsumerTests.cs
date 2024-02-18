using AutoFixture;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;

using OrderingService.Contracts.Events;

using ShippingService.Application.ProductShipping;
using ShippingService.Domain;
using ShippingService.Options;
using ShippingService.Services;

namespace ShippingService.Tests.Application.ProductShipping;

public class ProductShippingConsumerTests : IAsyncLifetime
{
    private ITestHarness? _harness;
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IShippableProductRepository _productRepository = Substitute.For<IShippableProductRepository>();
    private readonly ICustomerAddressRepository _customerAddressRepository = Substitute.For<ICustomerAddressRepository>();
    private readonly IOptions<ShippingOptions> _warehouseOptions = Substitute.For<IOptions<ShippingOptions>>();

    [Fact]
    public async Task WhenOrderContainsNoPhysicalItems_ThenMethodWillNoOp()
    {
        // arrange
        ArgumentNullException.ThrowIfNull(_harness);

        // act
        await _harness.Bus.Publish(new OrderSubmitted
        {
            CustomerId = 1,
            PurchaseOrderNumber = 123,
            LineItems = new List<OrderLineItem>(),
        });

        // assert
        var consumerHarness = _harness.GetConsumerHarness<ProductShippingConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<OrderSubmitted>());
        await _emailService.DidNotReceiveWithAnyArgs().Send(string.Empty, string.Empty, string.Empty);
    }

    [Fact]
    public async Task WhenOrderContainsPhysicalItems_ThenShippingSlipWillBeGenerated()
    {
        // arrange
        var fixture = new Fixture();
        ArgumentNullException.ThrowIfNull(_harness);

        const string recipient = "test@test.com";
        _warehouseOptions.Value.Returns(new ShippingOptions { WarehouseEmailRecipient = recipient });

        var lineItem = new OrderLineItem { ProductId = Guid.NewGuid(), ProductType = ProductType.Physical, Quantity = 1 };
        var productDetails = fixture.Build<ShippableProduct>().With(x => x.ProductId, lineItem.ProductId).Create();
        _productRepository.GetShippableProductsBulkAsync(Arg.Is<IEnumerable<Guid>>(x => x.First() == lineItem.ProductId)).Returns(new HashSet<ShippableProduct> { productDetails });

        var address = new CustomerAddress
        {
            AddressLine1 = "some road name",
            AddressLine2 = "city",
            PostCode = "AAA 2BC",
            CustomerId = 1,
        };
        _customerAddressRepository.GetCustomerAddressAsync(1).Returns(address);

        // act
        await _harness.Bus.Publish(new OrderSubmitted
        {
            CustomerId = 1,
            PurchaseOrderNumber = 123,
            LineItems = [lineItem],
        });

        // assert
        var consumerHarness = _harness.GetConsumerHarness<ProductShippingConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<OrderSubmitted>());

        await _emailService.Received(1).Send(Arg.Is<string>(x => x == recipient),
            Arg.Is<string>(x => x == $"Shipping details: {123}"),
            Arg.Is<string>(x => x.Contains(DateTime.UtcNow.ToShortDateString())
             && x.Contains(productDetails.Sku)
             && x.Contains(productDetails.ProductName)
             && x.Contains(productDetails.WeightKg.ToString())
             && x.Contains(lineItem.Quantity.ToString())
             && x.Contains(address.AddressLine1)
             && x.Contains(address.AddressLine2)
             && x.Contains(address.PostCode)));
    }

    public Task DisposeAsync()
    {
        _harness?.Stop();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        var provider = new ServiceCollection()
            .AddSingleton(_emailService)
            .AddSingleton(_warehouseOptions)
            .AddSingleton(_productRepository)
            .AddSingleton(_customerAddressRepository)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ProductShippingConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = provider.GetRequiredService<ITestHarness>();

        await _harness.Start();
    }
}