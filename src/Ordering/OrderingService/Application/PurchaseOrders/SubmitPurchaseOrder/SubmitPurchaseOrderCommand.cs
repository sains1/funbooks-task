using FluentValidation;

namespace OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;

public class SubmitPurchaseOrderCommand
{
    public required int CustomerId { get; set; }
    public required int PurchaseOrderNumber { get; set; }
    public required ICollection<ProductRequest> LineItems { get; set; }

    public class ProductRequest
    {
        public required Guid ProductId { get; set; }
        public required int Quantity { get; set; }
    }
}

public class SubmitPurchaseOrderValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
    public SubmitPurchaseOrderValidator()
    {
        RuleFor(x => x.LineItems).NotEmpty();

        RuleFor(x => x.LineItems)
            .ForEach(x => x.Must(item => item.Quantity > 0)).WithMessage("Quantity must be greater than 0")
            .ForEach(x => x.Must(item => item.Quantity < 100).WithMessage("Quantity must be less than 100"));

        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.PurchaseOrderNumber)
            .GreaterThan(0)
            .LessThan(100_000_000);
    }
}