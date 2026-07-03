using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Queries;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Queries;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;
using King.Nexa.Platform.Warehouse.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class InventoryItemRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<InventoryItem>(context), IInventoryItemRepository
{
    public override async Task<InventoryItem?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public override async Task<IEnumerable<InventoryItem>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().ToListAsync(cancellationToken);

    public async Task<InventoryItem?> FindByCatalogItemIdAsync(CatalogItemId catalogItemId, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(item => item.CatalogItemId == catalogItemId, cancellationToken);

    public async Task<IEnumerable<InventoryItem>> ListByWarehouseLocationAsync(WarehouseLocation warehouseLocation, CancellationToken cancellationToken = default) =>
        await Scoped().Where(item => item.WarehouseLocation == warehouseLocation).ToListAsync(cancellationToken);

    public async Task<IEnumerable<InventoryItem>> ListLowStockAsync(int threshold, CancellationToken cancellationToken = default)
    {
        var items = await Scoped().ToListAsync(cancellationToken);
        return items.Where(item => item.AvailableQuantity.Value <= threshold);
    }

    public async Task<PagedResult<InventoryItem>> SearchAsync(InventoryItemCollectionQuery query, CancellationToken cancellationToken = default)
    {
        var items = Scoped().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.ProductId))
            items = items.Where(item => item.ProductId == new ProductId(query.ProductId));
        if (query.WarehouseId.HasValue)
            items = items.Where(item => Context.InventoryLots.Any(lot =>
                lot.TenantId == item.TenantId &&
                lot.InventoryItemId == item.Id &&
                lot.WarehouseId == query.WarehouseId.Value));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            items = items.Where(item =>
                item.ProductId == new ProductId(search) ||
                item.CatalogItemId == new CatalogItemId(search) ||
                item.WarehouseLocation == new WarehouseLocation(search));
        }

        items = items.OrderBy(item => item.ProductId);
        return await items.ToPagedResultAsync(query.Pagination, cancellationToken);
    }

    private IQueryable<InventoryItem> Scoped()
    {
        var query = Context.InventoryItems.AsQueryable();
        return workspaceContext.TenantId is { } tenantId
            ? query.Where(item => item.TenantId == tenantId)
            : query.Where(_ => false);
    }
}
