using FluentValidation;

using MassTransit;

using OrderingService.Application.Products;
using OrderingService.Application.SubmitPurchaseOrder;
using OrderingService.Contracts.Events;
using OrderingService.Domain;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

public class SubmitPurchaseOrderHandler(IValidator<SubmitPurchaseOrderCommand> validator,
    IProductRepository productRepository,
    IPurchaseOrderRepository purchaseOrderRepository,
    IPublishEndpoint publisher)
{
    public async Task<SubmitPurchaseOrderResponseTypes> Handle(SubmitPurchaseOrderCommand command)
    {
        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            return validation;
        }

        var exists = await purchaseOrderRepository.PurchaseOrderExistsAsync(command.CustomerId, command.PurchaseOrderNumber);
        if (exists)
        {
            return new PurchaseOrderAlreadyExists();
        }

        var products = (await productRepository.GetProductsByIdBulkAsync(command.LineItems.Select(x => x.ProductId)))
            .ToDictionary(x => x.ProductId, x => x);

        if (command.LineItems.Any(x => !products.ContainsKey(x.ProductId)))
        {
            return new MissingProductResponse();
        }

        var cost = command.LineItems.Select(x => new { x.Quantity, products[x.ProductId].Price })
            .Select(x => x.Quantity * x.Price)
            .Sum();

        // Note - event is saved transactionally along with the PurchaseOrder to give us stronger guarantees about
        //      getting the message onto the bus, but it won't be published unless the PurchaseOrder is also committed
        await publisher.Publish(new OrderSubmitted
        {
            CustomerId = command.CustomerId,
            PurchaseOrderNumber = command.PurchaseOrderNumber,
            LineItems = command.LineItems.Select(x =>
                new OrderLineItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ProductType = products[x.ProductId].Type,
                }).ToList(),
        });

        await purchaseOrderRepository.AddPurchaseOrderAsync(new PurchaseOrder
        {
            CustomerId = command.CustomerId,
            PurchaseOrderNumber = command.PurchaseOrderNumber,
            TotalCost = cost,
            LineItems = command.LineItems.Select(x => new LineItem
            {
                CustomerId = command.CustomerId,
                PurchaseOrderNumber = command.PurchaseOrderNumber,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
            }).ToList(),
        });

        return new SubmitPurchaseOrderSuccess
        {
            CustomerId = command.CustomerId,
            PurchaseOrderNumber = command.PurchaseOrderNumber,
            TotalPrice = cost,
        };
    }
}