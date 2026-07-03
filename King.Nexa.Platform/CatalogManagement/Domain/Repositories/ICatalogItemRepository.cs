using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Model.Queries;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.CatalogManagement.Domain.Repositories;

public interface ICatalogItemRepository : IBaseRepository<CatalogItem>
{
    Task<CatalogItem?> FindByCatalogItemIdAsync(CatalogItemId catalogItemId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CatalogItem>> ListByCategoryNameAsync(CategoryName categoryName, CancellationToken cancellationToken = default);

    Task<IEnumerable<CatalogItem>> ListByBrandNameAsync(BrandName brandName, CancellationToken cancellationToken = default);

    Task<IEnumerable<CatalogItem>> ListByColdChainRequirementAsync(ColdChainRequirement coldChainRequirement, CancellationToken cancellationToken = default);

    Task<PagedResult<CatalogItem>> SearchAsync(CatalogItemCollectionQuery query, CancellationToken cancellationToken = default);
}
