using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using OrderingService.Application.PurchaseOrders.SubmitPurchaseOrder;
using OrderingService.Application.SubmitPurchaseOrder;

using SharedKernel.Validation;

namespace OrderingService.Controllers;

[Route("[controller]")]
public class PurchaseOrderController : ControllerBase
{
    /// <summary>
    /// Submits a new purchase order.
    /// </summary>
    /// <param name="purchaseOrderId"></param>
    /// <param name="body"></param>
    /// <param name="handler"></param>
    /// <param name="customerId"></param>
    /// <returns></returns>
    /// <remarks>
    /// Example customer IDs:
    /// - 12345
    /// - 56789
    ///
    /// Example product IDs:
    /// - 6831ee62-b099-44e7-b3e2-d2cd045cc2f5
    /// - 1d217f91-bef1-4eb6-ada8-d9d36739c03e
    /// - 3ea5f11d-c4ee-4f08-bdde-82559c7bd0af.
    /// </remarks>
    [HttpPost("{purchaseOrderId:int}")]
    [ProducesResponseType(typeof(SubmitPurchaseOrderSuccess), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitPurchaseOrder(
        [FromRoute] int purchaseOrderId,
        [FromBody] SubmitPurchaseOrderBody body,
        [FromServices] SubmitPurchaseOrderHandler handler,
        [FromHeader(Name = "X-Customer-ID")] int customerId) // In a realistic scenario this might come from a JWT or similar
    {
        var response = await handler.Handle(new SubmitPurchaseOrderCommand { CustomerId = customerId, PurchaseOrderNumber = purchaseOrderId, LineItems = body.LineItems });

        return response.Match<IActionResult>(
            success => Created(string.Empty, success), // can return action when we have that route
            conflict => Conflict(),
            validation =>
            {
                validation.AddToModelState(ModelState);
                return BadRequest(ModelState);
            },
            missing =>
            {
                ModelState.AddModelError(nameof(body.LineItems), "One of more products were missing");
                return BadRequest(ModelState);
            });
    }

    public class SubmitPurchaseOrderBody
    {
        [Required]
        public ICollection<SubmitPurchaseOrderCommand.ProductRequest> LineItems { get; set; } =[];
    }
}