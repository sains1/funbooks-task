using AutoFixture;
using AutoFixture.Xunit2;

using NSubstitute;

using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;
using OrderingService.Domain;
using OrderingService.Tests.Setup;

using SharedKernel.Temporal;

using ShippingService.Application.ProductShipping;

using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Exceptions;
using Temporalio.Generators.Workflows;
using Temporalio.Worker;
using Temporalio.Workflows;

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

    [Theory, AutoData]
    public async Task WhenPurchaseOrderContainsShippableItems_ShouldTriggerShippingWorkflow(SubmitPurchaseOrderCommand command, Fixture fixture,
        SubmitPurchaseOrderCommand.ProductRequest productRequest)
    {
        // arrange
        var activities = new MockPurchaseOrderProcessorActivities();
        activities.MockCheckPurchaseOrderExistsAsync(Arg.Is<CheckPurchaseOrderExistsArgs>(args =>
            args.CustomerId == command.CustomerId && args.PurchaseOrderId == command.PurchaseOrderNumber))
            .Returns(false);

        command.LineItems = [productRequest];

        var productDetails = fixture.Build<Product>()
            .With(x => x.ProductId, productRequest.ProductId)
            .With(x => x.Type, ProductType.Physical)
            .Create();

        activities.MockGetPurchaseOrderProductInformation(Arg.Any<GetPurchaseOrderProductInformation>())
            .Returns(new List<Product> { productDetails });

        var shippingActivities = new MockShippingActivities();

        var (env, worker) = await TemporalSetupHelpers.SetupWorker(opts => opts
            .AddWorkflow<PurchaseOrderProcessorWorkflow>()
            .AddAllActivities(activities));

        var shippingWorker = new TemporalWorker(env.Client, new TemporalWorkerOptions(TemporalConstants.ShippingServiceTaskQueue)
            .AddWorkflow<ProductShippingWorkflow>()
            .AddAllActivities(shippingActivities));


        await worker.ExecuteAsync(async () =>
        {
            // act
            var response = await env.Client.ExecuteWorkflowAsync(
                (PurchaseOrderProcessorWorkflow wf) => wf.SubmitAsync(command),
                new(id: $"wf-{Guid.NewGuid()}", taskQueue: worker.Options.TaskQueue!));

            // assert
            var expected = new
            {
                command.CustomerId,
                command.PurchaseOrderNumber,
                TotalPrice = productDetails.Price
            };

            Assert.IsType<SubmitPurchaseOrderSuccess>(response);
        });

        await shippingWorker.ExecuteAsync(async () =>
        {
            await env.DelayAsync(1000);

            var actualShippingArgs = shippingActivities.Args;
            var expectedShippingArgs = new ProductShippingWorkflowArgs
            {
                ShippingLineItems = [new ShippingLineItem
                {
                    ProductId = productDetails.ProductId,
                    ProductName = productDetails.ProductName,
                    Quantity = productRequest.Quantity
                }],
                CustomerId = command.CustomerId,
                PurchaseOrderNumber = command.PurchaseOrderNumber
            };

            Assert.NotNull(actualShippingArgs);
            Assert.Equivalent(expectedShippingArgs, actualShippingArgs);
        });
    }
}

[GenerateNSubstituteMocks]
public partial class MockPurchaseOrderProcessorActivities : ActivityMockBase<PurchaseOrderProcessorActivities>
{
}

[Workflow]
public class ProductShippingWorkflow : IProductShippingWorkflow
{
    [WorkflowRun]
    public async Task ShipAsync(ProductShippingWorkflowArgs args)
    {
        await Workflow.ExecuteActivityAsync((MockShippingActivities act) => act.MockCallback(args),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(60) });
    }
}

public class MockShippingActivities
{
    public ProductShippingWorkflowArgs? Args;

    [Activity]
    public Task MockCallback(ProductShippingWorkflowArgs args)
    {
        Args = args;
        return Task.CompletedTask;
    }
}