using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.CatalogManagement.Interfaces.REST.Resources;
using King.Nexa.Platform.CatalogManagement.Interfaces.REST.Transform;
using King.Nexa.Platform.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.CatalogManagement.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController(IProductRepository productRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProducts(CancellationToken cancellationToken)
    {
        var products = await productRepository.ListAsync(cancellationToken);
        return Ok(products.Select(ProductResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await productRepository.FindByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(ProductResourceFromEntityAssembler.ToResourceFromEntity(product));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductResource resource, CancellationToken cancellationToken)
    {
        var command = CreateProductCommandFromResourceAssembler.ToCommandFromResource(resource);
        var product = new Product(command);
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, ProductResourceFromEntityAssembler.ToResourceFromEntity(product));
    }
}
