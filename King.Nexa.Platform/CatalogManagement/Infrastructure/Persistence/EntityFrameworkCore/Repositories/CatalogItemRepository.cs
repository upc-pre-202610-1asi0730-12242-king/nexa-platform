using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Model.Queries;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Queries;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.CatalogManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class CatalogItemRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<CatalogItem>(context), ICatalogItemRepository
{
    public override async Task<CatalogItem?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(catalogItem => catalogItem.Id == id, cancellationToken);

    public override async Task<IEnumerable<CatalogItem>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().ToListAsync(cancellationToken);

    public async Task<CatalogItem?> FindByCatalogItemIdAsync(CatalogItemId catalogItemId, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(catalogItem => catalogItem.CatalogItemId == catalogItemId, cancellationToken);

    public async Task<IEnumerable<CatalogItem>> ListByCategoryNameAsync(CategoryName categoryName, CancellationToken cancellationToken = default) =>
        await Scoped().Where(catalogItem => catalogItem.CategoryName == categoryName).ToListAsync(cancellationToken);

    public async Task<IEnumerable<CatalogItem>> ListByBrandNameAsync(BrandName brandName, CancellationToken cancellationToken = default) =>
        await Scoped().Where(catalogItem => catalogItem.BrandName == brandName).ToListAsync(cancellationToken);

    public async Task<IEnumerable<CatalogItem>> ListByColdChainRequirementAsync(ColdChainRequirement coldChainRequirement, CancellationToken cancellationToken = default) =>
        await Scoped().Where(catalogItem => catalogItem.ColdChainRequirement == coldChainRequirement).ToListAsync(cancellationToken);

    public async Task<PagedResult<CatalogItem>> SearchAsync(CatalogItemCollectionQuery query, CancellationToken cancellationToken = default)
    {
        var items = Scoped().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Brand))
            items = items.Where(item => item.BrandName == new BrandName(query.Brand));
        if (!string.IsNullOrWhiteSpace(query.Category))
            items = items.Where(item => item.CategoryName == new CategoryName(query.Category));
        if (query.ColdChain.HasValue)
            items = items.Where(item => item.ColdChainRequirement == query.ColdChain.Value);
        if (query.Active.HasValue)
            items = items.Where(item => item.IsActive == query.Active.Value);
        if (query.CreatedFrom.HasValue)
            items = items.Where(item => item.CreatedAt >= query.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (query.CreatedTo.HasValue)
            items = items.Where(item => item.CreatedAt <= query.CreatedTo.Value.ToDateTime(TimeOnly.MaxValue));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var pattern = $"%{search}%";
            items = items.Where(item =>
                item.CatalogItemId == new CatalogItemId(search) ||
                item.ProductId == new ProductId(search) ||
                EF.Functions.ILike(item.Description, pattern));
        }

        items = items.OrderBy(item => item.ItemName);
        return await items.ToPagedResultAsync(query.Pagination, cancellationToken);
    }

    private IQueryable<CatalogItem> Scoped()
    {
        var query = Context.CatalogItems.AsQueryable();
        return workspaceContext.TenantId is { } tenantId
            ? query.Where(catalogItem => catalogItem.TenantId == tenantId)
            : query.Where(_ => false);
    }
}
