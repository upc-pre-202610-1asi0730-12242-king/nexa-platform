using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Interfaces.REST.Resources;

namespace King.Nexa.Platform.CatalogManagement.Interfaces.REST.Transform;

public static class ProductResourceFromEntityAssembler
{
    public static ProductResource ToResourceFromEntity(Product entity) =>
        new(entity.Id, entity.ProductCode.Value, entity.Name, entity.CategoryName.Value, entity.ColdChainRequirement.ToString());
}
