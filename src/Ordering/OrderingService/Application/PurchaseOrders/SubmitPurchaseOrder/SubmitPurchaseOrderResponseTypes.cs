using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

using OneOf;

namespace OrderingService.Application.SubmitPurchaseOrder;

[GenerateOneOf]
public partial class SubmitPurchaseOrderResponseTypes : OneOfBase<SubmitPurchaseOrderSuccess, ConflictResult, ValidationResult>
{
}

public class SubmitPurchaseOrderSuccess
{
    public required int CustomerId { get; set; }
    public required int PurchaseOrderNumber { get; set; }
    public required decimal TotalPrice { get; set; }
}