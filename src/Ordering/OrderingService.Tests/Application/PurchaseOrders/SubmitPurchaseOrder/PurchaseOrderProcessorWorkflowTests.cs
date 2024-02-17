using AutoFixture;
using AutoFixture.Xunit2;

using NSubstitute;

using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;
using OrderingService.Domain;
using OrderingService.Tests.Setup;

using Temporalio.Client;
using Temporalio.Exceptions;
using Temporalio.Generators.Workflows;

namespace OrderingService.Tests.Application.PurchaseOrders.SubmitPurchaseOrder;

public class PurchaseOrderProcessorWorkflowTests
{

    [Theory, AutoData]
    public async Task WhenPurchaseOrderExists_ShouldThrowApplicationExceptionWithConflict(SubmitPurchaseOrderCommand command)
    {
        // arrange
        var activities = new MockPurchaseOrderProcessorActivities();

        activities.MockCheckPurchaseOrderExistsAsync(Arg.Is<CheckPurchaseOrderExistsArgs>(args =>
            args.CustomerId == command.CustomerId && args.PurchaseOrderId == command.PurchaseOrderNumber)).Returns(true);

        var (env, worker) = await TemporalSetupHelpers.SetupWorker(opts => opts
            .AddWorkflow<PurchaseOrderProcessorWorkflow>()
            .AddAllActivities(activities));

        await worker.ExecuteAsync(async () =>
        {
            // act
            var act = async () => await env.Client.ExecuteWorkflowAsync((PurchaseOrderProcessorWorkflow wf) => wf.SubmitAsync(command),
                new(id: Guid.NewGuid().ToString(), taskQueue: worker.Options.TaskQueue!));

            // assert
            var ex = await Assert.ThrowsAsync<WorkflowFailedException>(act);
            var appEx = Assert.IsType<ApplicationFailureException>(ex.InnerException);
            Assert.Equal(PurchaseOrderProcessorWorkflow.PurchaseOrderConflict, appEx.ErrorType);
        });
    }


    [Theory, AutoData]
    public async Task WhenPurchaseOrderContainsInvalidProducts_ShouldThrowApplicationExceptionWithValidationError(SubmitPurchaseOrderCommand command)
    {
        // arrange
        var activities = new MockPurchaseOrderProcessorActivities();

        activities.MockCheckPurchaseOrderExistsAsync(Arg.Is<CheckPurchaseOrderExistsArgs>(args =>
            args.CustomerId == command.CustomerId && args.PurchaseOrderId == command.PurchaseOrderNumber))
            .Returns(false);

        activities.MockGetPurchaseOrderProductInformation(Arg.Any<GetPurchaseOrderProductInformation>())
            .Returns(new List<Product>());

        var (env, worker) = await TemporalSetupHelpers.SetupWorker(opts => opts
            .AddWorkflow<PurchaseOrderProcessorWorkflow>()
            .AddAllActivities(activities));

        await worker.ExecuteAsync(async () =>
        {
            // act
            var act = async () => await env.Client.ExecuteWorkflowAsync(
                (PurchaseOrderProcessorWorkflow wf) => wf.SubmitAsync(command),
                new(id: $"wf-{Guid.NewGuid()}", taskQueue: worker.Options.TaskQueue!));

            // assert
            var ex = await Assert.ThrowsAsync<WorkflowFailedException>(act);
            var appEx = Assert.IsType<ApplicationFailureException>(ex.InnerException);
            Assert.Equal(PurchaseOrderProcessorWorkflow.MissingProduct, appEx.ErrorType);
        });
    }

    [Theory, AutoData]
    public async Task WhenPurchaseOrderValid_ShouldReturnSavedPurchaseOrder(SubmitPurchaseOrderCommand command, Fixture fixture)
    {
        // arrange
        var activities = new MockPurchaseOrderProcessorActivities();

        activities.MockCheckPurchaseOrderExistsAsync(Arg.Is<CheckPurchaseOrderExistsArgs>(args =>
            args.CustomerId == command.CustomerId && args.PurchaseOrderId == command.PurchaseOrderNumber))
            .Returns(false);

        var products = fixture.CreateMany<Product>(command.LineItems.Count);

        activities.MockGetPurchaseOrderProductInformation(Arg.Any<GetPurchaseOrderProductInformation>())
            .Returns(products.ToList());

        var (env, worker) = await TemporalSetupHelpers.SetupWorker(opts => opts
            .AddWorkflow<PurchaseOrderProcessorWorkflow>()
            .AddAllActivities(activities));

        await worker.ExecuteAsync(async () =>
        {
            // act
            var response = await env.Client.ExecuteWorkflowAsync(
                (PurchaseOrderProcessorWorkflow wf) => wf.SubmitAsync(command),
                new(id: $"wf-{Guid.NewGuid()}", taskQueue: worker.Options.TaskQueue!));

            // assert
            var expected = new SubmitPurchaseOrderSuccess
            {
                CustomerId = command.CustomerId,
                PurchaseOrderNumber = command.PurchaseOrderNumber,
                TotalPrice = products.Select(x => x.Price).Sum()
            };

            Assert.Equivalent(expected, response);
        });
    }
}

[GenerateNSubstituteMocks]
public partial class MockPurchaseOrderProcessorActivities : ActivityMockBase<PurchaseOrderProcessorActivities>
{
}