using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.CatalogManagement.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> FindByProductCodeAsync(ProductCode productCode, CancellationToken cancellationToken = default);
}
