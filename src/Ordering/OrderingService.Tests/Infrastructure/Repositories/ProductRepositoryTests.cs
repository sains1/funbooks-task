using AutoFixture;
using AutoFixture.Xunit2;

using OrderingService.Domain;
using OrderingService.Infrastructure.Repositories;
using OrderingService.Tests.Setup;

namespace OrderingService.Tests.Infrastructure.Repositories;

public class ProductRepositoryTests : IAsyncLifetime, IClassFixture<OrderingDbContextFixture>
{
    private readonly ProductRepository _sut;
    private readonly OrderingDbContextFixture _fixture;

    public ProductRepositoryTests(OrderingDbContextFixture fixture)
    {
        _sut = new ProductRepository(fixture.GetScopedDbContext());
        _fixture = fixture;
    }

    [Theory, AutoData]
    public async Task GetProductOrNullAsync_WhenProductExists_ShouldReturnProduct(Fixture fixture)
    {
        // arrange
        var dbContext = _fixture.GetScopedDbContext();

        var product = fixture.Build<Product>().Without(x => x.ProductId).Without(x => x.CreatedAt).Create();
        dbContext.Products.Add(product);

        Assert.Equal(1, await dbContext.SaveChangesAsync());

        // act
        var result = await _sut.GetProductOrNullAsync(product.ProductId);

        // assert
        Assert.Equivalent(product, result);
    }

    [Theory, AutoData]
    public async Task GetProductOrNullAsync_WhenProductNotExists_ShouldReturnNull(Fixture fixture)
    {
        // arrange
        var product = fixture.Build<Product>().Without(x => x.ProductId).Without(x => x.CreatedAt).Create();

        // act
        var result = await _sut.GetProductOrNullAsync(product.ProductId);

        // assert
        Assert.Null(result);
    }


    [Theory, AutoData]
    public async Task GetProductsByIdBulkAsync_WhenProductsExists_ShouldReturnProducts(Fixture fixture)
    {
        // arrange
        var dbContext = _fixture.GetScopedDbContext();

        var products = fixture.Build<Product>().Without(x => x.ProductId).Without(x => x.CreatedAt).CreateMany(5);
        dbContext.Products.AddRange(products);

        Assert.Equal(5, await dbContext.SaveChangesAsync());

        // act
        var result = await _sut.GetProductsByIdBulkAsync(products.Select(x => x.ProductId));

        // assert
        Assert.Equivalent(products, result);
    }

    [Theory, AutoData]
    public async Task GetProductsByIdBulkAsync_WhenProductsNotExist_ShouldReturnEmptyCollection(Fixture fixture)
    {
        // arrange
        var dbContext = _fixture.GetScopedDbContext();

        var products = fixture.Build<Product>().Without(x => x.ProductId).Without(x => x.CreatedAt).CreateMany(5);
        dbContext.Products.AddRange(products);

        Assert.Equal(5, await dbContext.SaveChangesAsync());

        // act
        var result = await _sut.GetProductsByIdBulkAsync(new List<Guid> { Guid.NewGuid() });

        // assert
        Assert.Empty(result);
    }


    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() => _fixture.ResetAsync();
}