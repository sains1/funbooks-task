using Microsoft.AspNetCore.Mvc;

using OrderingService.Application.Products;
using OrderingService.Application.Products.GetProduct;

namespace OrderingService.Controllers;

[Route("[controller]")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Gets a product by its ProductID
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    /// <remarks>
    /// Example Product Ids:
    /// - 6831ee62-b099-44e7-b3e2-d2cd045cc2f5
    /// - 1d217f91-bef1-4eb6-ada8-d9d36739c03e
    /// - 3ea5f11d-c4ee-4f08-bdde-82559c7bd0af
    /// </remarks>

    [HttpGet("{productId:guid}", Name = nameof(GetProduct))]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        [FromRoute] Guid productId,
        [FromServices] GetProductHandler handler)
    {
        var result = await handler.GetProductAsync(new GetProductQuery { ProductId = productId });

        return result switch
        {
            { } => Ok(result),
            null => NotFound()
        };
    }
}