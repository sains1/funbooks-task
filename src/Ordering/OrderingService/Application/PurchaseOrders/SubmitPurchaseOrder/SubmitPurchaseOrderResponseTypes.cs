using FluentValidation.Results;

using OneOf;

namespace OrderingService.Application.SubmitPurchaseOrder;

[GenerateOneOf]
public partial class SubmitPurchaseOrderResponseTypes : OneOfBase<SubmitPurchaseOrderSuccess, PurchaseOrderAlreadyExists, ValidationResult, MissingProductResponse>
{
}

public class SubmitPurchaseOrderSuccess
{
    public required int CustomerId { get; set; }

    public required int PurchaseOrderNumber { get; set; }

    public required decimal TotalPrice { get; set; }
}

public class MissingProductResponse
{
}

public class PurchaseOrderAlreadyExists
{
}