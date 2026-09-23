namespace EngineeringKnowledgeHub.Tests.Application;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.ProductsAndTechnologies;
using EngineeringKnowledgeHub.Tests.Common;
using Xunit;

public class ProductAndTechnologyServiceTests
{
    private readonly InMemoryProductRepository _productRepo = new();
    private readonly InMemoryTechnologyRepository _techRepo = new();
    private readonly ProductService _productService;
    private readonly TechnologyService _techService;

    public ProductAndTechnologyServiceTests()
    {
        _productService = new ProductService(_productRepo);
        _techService = new TechnologyService(_techRepo);
    }

    [Fact]
    public async Task CreateProduct_With_Valid_Data_Should_Succeed()
    {
        var command = new CreateProductCommand("Toll Collection", "TOLL-SYS", "Automated toll system");
        var result = await _productService.CreateProductAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Toll Collection", result.Name);
        Assert.Equal("TOLL-SYS", result.Code);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateProduct_With_Duplicate_Code_Should_Throw_ConflictException()
    {
        await _productService.CreateProductAsync(new CreateProductCommand("Prod 1", "P-01"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _productService.CreateProductAsync(new CreateProductCommand("Prod 2", "P-01")));
    }

    [Fact]
    public async Task UpdateProduct_With_Duplicate_Code_Should_Throw_ConflictException()
    {
        await _productService.CreateProductAsync(new CreateProductCommand("Prod 1", "CODE-1"));
        var prod2 = await _productService.CreateProductAsync(new CreateProductCommand("Prod 2", "CODE-2"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _productService.UpdateProductAsync(new UpdateProductCommand(prod2.Id, "Prod 2 New", "CODE-1")));
    }

    [Fact]
    public async Task Activate_And_Deactivate_Product_Should_Toggle_Status()
    {
        var prod = await _productService.CreateProductAsync(new CreateProductCommand("Prod", "CODE"));

        var deactivated = await _productService.DeactivateProductAsync(prod.Id);
        Assert.False(deactivated.IsActive);

        var activated = await _productService.ActivateProductAsync(prod.Id);
        Assert.True(activated.IsActive);
    }

    [Fact]
    public async Task GetProductById_With_NonExistent_Id_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _productService.GetProductByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateTechnology_And_GetById_Should_Succeed()
    {
        var tech = await _techService.CreateTechnologyAsync(new CreateTechnologyCommand("WPF", "Windows Presentation Foundation"));

        var fetched = await _techService.GetTechnologyByIdAsync(tech.Id);

        Assert.Equal("WPF", fetched.Name);
        Assert.Equal("Windows Presentation Foundation", fetched.Description);
    }

    [Fact]
    public async Task UpdateTechnology_With_Valid_Data_Should_Update()
    {
        var tech = await _techService.CreateTechnologyAsync(new CreateTechnologyCommand("SQL Server"));
        var updated = await _techService.UpdateTechnologyAsync(new UpdateTechnologyCommand(tech.Id, "SQL Server 2025", "Latest RDBMS"));

        Assert.Equal("SQL Server 2025", updated.Name);
        Assert.Equal("Latest RDBMS", updated.Description);
    }

    [Fact]
    public async Task GetTechnologyById_With_NonExistent_Id_Should_Throw_NotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _techService.GetTechnologyByIdAsync(Guid.NewGuid()));
    }
}
