using FluentValidation.Results;

using OneOf;

using SharedKernel.GenericResponses;

namespace OrderingService.Application.SubmitPurchaseOrder;

[GenerateOneOf]
public partial class SubmitPurchaseOrderResponseTypes : OneOfBase<SubmitPurchaseOrderSuccess, ConflictResponse, ValidationResult, MissingProductResponse>
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