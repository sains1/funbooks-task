using FluentValidation.TestHelper;

using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

namespace OrderingService.Tests.Application.PurchaseOrders.SubmitPurchaseOrder;

public class SubmitPurchaseOrderValidatorTests
{
    private readonly SubmitPurchaseOrderValidator _sut = new();

    [Fact]
    public async Task WhenLineItemsEmpty_ShouldReturnError()
    {
        // arrange
        var input = new SubmitPurchaseOrderCommand { CustomerId = 1, LineItems = [], PurchaseOrderNumber = 1 };

        // act
        var result = await _sut.TestValidateAsync(input);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.LineItems);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task WhenLineItemsContainAnInvalidQuantity_ShouldReturnError(int quantity)
    {
        // arrange
        var input = new SubmitPurchaseOrderCommand
        {
            CustomerId = 1,
            LineItems = [new SubmitPurchaseOrderCommand.ProductRequest { ProductId = Guid.NewGuid(), Quantity = quantity }],
            PurchaseOrderNumber = 1
        };

        // act
        var result = await _sut.TestValidateAsync(input);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.LineItems);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WhenCustomerIdIsInvalid_ShouldReturnError(int customerId)
    {
        // arrange
        var input = new SubmitPurchaseOrderCommand
        {
            CustomerId = customerId,
            LineItems = [new SubmitPurchaseOrderCommand.ProductRequest { ProductId = Guid.NewGuid(), Quantity = 1 }],
            PurchaseOrderNumber = 1
        };

        // act
        var result = await _sut.TestValidateAsync(input);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100_000_000)]
    public async Task WhenOrderNumberIsInvalid_ShouldReturnError(int orderNumber)
    {
        // arrange
        var input = new SubmitPurchaseOrderCommand
        {
            CustomerId = 1,
            LineItems = [new SubmitPurchaseOrderCommand.ProductRequest { ProductId = Guid.NewGuid(), Quantity = 1 }],
            PurchaseOrderNumber = orderNumber
        };

        // act
        var result = await _sut.TestValidateAsync(input);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.PurchaseOrderNumber);
    }
}