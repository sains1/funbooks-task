using AutoFixture.Xunit2;

using FluentValidation;
using FluentValidation.Results;

using NSubstitute;

using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;

namespace OrderingService.Tests.Application.PurchaseOrders.SubmitPurchaseOrder;

public class SubmitPurchaseOrderHandlerTests
{
    private readonly IValidator<SubmitPurchaseOrderCommand> _validator = Substitute.For<IValidator<SubmitPurchaseOrderCommand>>();
    private readonly IPurchaseOrderProcessorWorkflowClient _workflowClient = Substitute.For<IPurchaseOrderProcessorWorkflowClient>();
    private readonly SubmitPurchaseOrderHandler _sut;
    public SubmitPurchaseOrderHandlerTests()
    {
        _sut = new(_validator, _workflowClient);
    }

    [Theory, AutoData]
    public async Task WhenPurchaseOrderNotValid_ValidationResultIsReturned(SubmitPurchaseOrderCommand command)
    {
        // arrange
        var expected = new ValidationResult([new ValidationFailure(nameof(command.PurchaseOrderNumber), "bad order number")]);
        _validator.ValidateAsync(command).Returns(Task.FromResult(expected));

        // act
        var result = await _sut.Handle(command);

        // assert
        Assert.IsType<ValidationResult>(result.AsT2);
        Assert.Equivalent(expected, result.AsT2);

        _ = _workflowClient.DidNotReceive().SubmitAsync(command);
    }

    [Theory, AutoData]
    public async Task WhenPurchaseOrderValid_WorkflowIsExecuted(SubmitPurchaseOrderCommand command, SubmitPurchaseOrderSuccess expectedResponse)
    {
        // arrange
        _validator.ValidateAsync(command).Returns(Task.FromResult(new ValidationResult()));
        _workflowClient.SubmitAsync(command).Returns(expectedResponse);

        // act
        var response = await _sut.Handle(command);

        // assert
        var type = Assert.IsType<SubmitPurchaseOrderSuccess>(response.AsT0);
        Assert.Equal(expectedResponse, type);

        _ = _workflowClient.Received(1).SubmitAsync(command);
    }

}