using MassTransit;

using Microsoft.Extensions.Options;

using OrderingService.Contracts.Events;

using ShippingService.Options;
using ShippingService.Services;

namespace ShippingService.Application.ProductShipping;

public class ProductShippingConsumer(IEmailService emailService, IOptions<ShippingOptions> options,
    IShippableProductRepository productRepository, ICustomerAddressRepository customerRepository)
    : IConsumer<OrderSubmitted>
{
    public async Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var shippableItems = context.Message.LineItems.Where(x => x.ProductType == ProductType.Physical);
        if (!shippableItems.Any())
        {
            return; // nothing to do
        }

        var productDetails = (await productRepository.GetShippableProductsBulkAsync(shippableItems.Select(x => x.ProductId))).ToDictionary(x => x.ProductId, x => x);
        var address = await customerRepository.GetCustomerAddressAsync(context.Message.CustomerId);
        var orderNumber = context.Message.PurchaseOrderNumber;

        var productSlipItems = shippableItems.Select(item => (item, productDetails[item.ProductId]))
            .Select(x => $"| {x.Item2.ProductName}, {x.item.Quantity}, {x.Item2.WeightKg}, {x.Item2.Sku}");

        // in a real app I'd move this out into some sort of templating system
        var slip = $"""
                    ------------------------------------------------
                    | FunBooksAndVids Shipping Slip                |
                    ------------------------------------------------
                    | Order: {orderNumber}
                    | Date: {DateTime.Now.ToShortDateString()}
                    |
                    | Ship To:
                    |     {address.AddressLine1}
                    |     {address.AddressLine2}
                    |     {address.PostCode}
                    |
                    | Name, Qty, Weight, Sku
                    {string.Join('\n', productSlipItems)}
                    ------------------------------------------------
                    """;

        await emailService.Send(options.Value.WarehouseEmailRecipient, $"Shipping details: {orderNumber}", slip);
    }
}