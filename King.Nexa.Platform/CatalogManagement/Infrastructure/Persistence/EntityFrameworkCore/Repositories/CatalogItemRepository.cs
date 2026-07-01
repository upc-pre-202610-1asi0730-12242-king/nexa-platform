using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
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

    private IQueryable<CatalogItem> Scoped()
    {
        var query = Context.CatalogItems.AsQueryable();
        return workspaceContext.TenantId is { } tenantId
            ? query.Where(catalogItem => catalogItem.TenantId == tenantId)
            : query.Where(_ => false);
    }
}
