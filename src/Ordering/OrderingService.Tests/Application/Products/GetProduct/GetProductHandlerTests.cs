using AutoFixture.Xunit2;

using NSubstitute;

using OrderingService.Application.Products;
using OrderingService.Application.Products.GetProduct;
using OrderingService.Domain;

namespace OrderingService.Tests.Application.Products.GetProduct;

public class GetProductHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly GetProductHandler _sut;
    public GetProductHandlerTests()
    {
        _sut = new GetProductHandler(_repository);
    }

    [Theory, AutoData]
    public async Task WhenProductExists_ShouldReturnProductDto(Product product)
    {
        // arrange
        _repository.GetProductOrNullAsync(product.ProductId).Returns(product);

        // act
        var result = await _sut.GetProductAsync(new GetProductQuery { ProductId = product.ProductId });

        // assert
        var expected = new ProductDto
        {
            ProductId = product.ProductId,
            Price = product.Price,
            ProductName = product.ProductName
        };

        Assert.Equivalent(expected, result);
    }

    [Theory, AutoData]
    public async Task WhenProductNotExists_ShouldReturnNull(Guid productId)
    {
        // arrange

        // act
        var product = await _sut.GetProductAsync(new GetProductQuery { ProductId = productId });

        // assert
        Assert.Null(product);
    }
}