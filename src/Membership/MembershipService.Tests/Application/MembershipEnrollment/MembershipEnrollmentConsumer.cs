using MassTransit;
using MassTransit.Testing;

using MembershipService.Application.MembershipEnrollment;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using OrderingService.Contracts.Events;

namespace ShippingService.Tests.Application.ProductShipping;

public class MembershipEnrollmentConsumerTests : IAsyncLifetime
{
    private ITestHarness? _harness;
    private readonly IMembershipRepository _membershipRepository = Substitute.For<IMembershipRepository>();

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
        var consumerHarness = _harness.GetConsumerHarness<MembershipEnrollmentConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<OrderSubmitted>());

        await _membershipRepository.DidNotReceiveWithAnyArgs().EnrollUserInMembership(Arg.Any<int>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task WhenOrderContainsPhysicalItems_ThenShippingSlipWillBeGenerated()
    {
        // arrange
        ArgumentNullException.ThrowIfNull(_harness);

        var lineItem = new OrderLineItem { ProductId = Guid.NewGuid(), ProductType = ProductType.Membership, Quantity = 1 };

        // act
        await _harness.Bus.Publish(new OrderSubmitted
        {
            CustomerId = 1,
            PurchaseOrderNumber = 123,
            LineItems = [lineItem],
        });

        // assert
        var consumerHarness = _harness.GetConsumerHarness<MembershipEnrollmentConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<OrderSubmitted>());

        await _membershipRepository.Received(1).EnrollUserInMembership(1, lineItem.ProductId);
    }

    public Task DisposeAsync()
    {
        _harness?.Stop();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        var provider = new ServiceCollection()
            .AddSingleton(_membershipRepository)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<MembershipEnrollmentConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = provider.GetRequiredService<ITestHarness>();

        await _harness.Start();
    }
}