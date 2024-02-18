using AutoFixture;
using AutoFixture.Xunit2;

using FluentValidation;
using FluentValidation.Results;

using MassTransit;

using NSubstitute;

using OrderingService.Application.Products;
using OrderingService.Application.PurchaseOrders;
using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;
using OrderingService.Contracts.Events;
using OrderingService.Domain;

namespace OrderingService.Tests.Application.PurchaseOrders.SubmitPurchaseOrder;

public class SubmitPurchaseOrderHandlerTests
{
    private readonly IValidator<SubmitPurchaseOrderCommand> _validator = Substitute.For<IValidator<SubmitPurchaseOrderCommand>>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IPurchaseOrderRepository _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();

    private readonly SubmitPurchaseOrderHandler _sut;
    public SubmitPurchaseOrderHandlerTests()
    {
        _sut = new(_validator, _productRepository, _purchaseOrderRepository, _publishEndpoint);
    }

    [Theory, AutoData]
    public async Task WhenPurchaseOrderNotValid_ValidationResultIsReturned(SubmitPurchaseOrderCommand command)
    {
        // arrange
        var expected = new FluentValidation.Results.ValidationResult([new ValidationFailure(nameof(command.PurchaseOrderNumber), "bad order number")]);
        _validator.ValidateAsync(command).Returns(Task.FromResult(expected));

        // act
        var result = await _sut.Handle(command);

        // assert
        Assert.IsType<FluentValidation.Results.ValidationResult>(result.AsT2);
        Assert.Equivalent(expected, result.AsT2);
    }

    [Theory, AutoData]
    public async Task WhenPurchaseOrderAlreadyExists_AlreadyExistsResponseIsReturned(SubmitPurchaseOrderCommand command)
    {
        // arrange
        _validator.ValidateAsync(command).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
        _purchaseOrderRepository.PurchaseOrderExistsAsync(command.CustomerId, command.PurchaseOrderNumber).Returns(true);

        // act
        var response = await _sut.Handle(command);

        // assert
        Assert.IsType<PurchaseOrderAlreadyExists>(response.AsT1);
    }

    [Theory, AutoData]
    public async Task WhenProductsAreMissing_MissingProductResponseIsReturned(SubmitPurchaseOrderCommand command)
    {
        // arrange
        _validator.ValidateAsync(command).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
        _purchaseOrderRepository.PurchaseOrderExistsAsync(command.CustomerId, command.PurchaseOrderNumber).Returns(false);
        _productRepository.GetProductsByIdBulkAsync(Arg.Any<IEnumerable<Guid>>()).Returns([]);

        // act
        var response = await _sut.Handle(command);

        // assert
        Assert.IsType<MissingProductResponse>(response.AsT3);
    }

    [Theory, AutoData]
    public async Task WhenOrderIsValid_SuccessResponseIsReturned(SubmitPurchaseOrderCommand command, Fixture fixture)
    {
        // arrange
        _validator.ValidateAsync(command).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
        _purchaseOrderRepository.PurchaseOrderExistsAsync(command.CustomerId, command.PurchaseOrderNumber).Returns(false);

        var products = command.LineItems.Select(x => fixture.Build<Product>().With(p => p.ProductId, x.ProductId).Create()).ToList();
        _productRepository.GetProductsByIdBulkAsync(Arg.Any<IEnumerable<Guid>>()).Returns(products);

        // act
        var response = await _sut.Handle(command);

        // assert
        var success = Assert.IsType<SubmitPurchaseOrderSuccess>(response.AsT0);

        decimal expectedSum = 0;
        foreach (var item in command.LineItems)
        {
            expectedSum += item.Quantity * products.First(x => x.ProductId == item.ProductId).Price;
        }

        Assert.Equal(expectedSum, success.TotalPrice);

        await _publishEndpoint.Received(1).Publish(Arg.Is<OrderSubmitted>(x =>
            x.CustomerId == command.CustomerId &&
            x.PurchaseOrderNumber == command.PurchaseOrderNumber &&
            x.LineItems.Count == command.LineItems.Count));
    }
}