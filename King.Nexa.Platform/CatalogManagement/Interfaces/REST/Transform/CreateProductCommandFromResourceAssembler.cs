using King.Nexa.Platform.CatalogManagement.Domain.Model.Commands;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Interfaces.REST.Resources;

namespace King.Nexa.Platform.CatalogManagement.Interfaces.REST.Transform;

public static class CreateProductCommandFromResourceAssembler
{
    public static CreateProductCommand ToCommandFromResource(CreateProductResource resource) =>
        new(new ProductCode(resource.ProductCode), resource.Name, new CategoryName(resource.CategoryName), Enum.Parse<ColdChainRequirement>(resource.ColdChainRequirement, true));
}
