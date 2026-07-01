using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;
using King.Nexa.Platform.Warehouse.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class WarehouseRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<WarehouseAggregate>(context), IWarehouseRepository
{
    public override async Task<WarehouseAggregate?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(warehouse => warehouse.Id == id, cancellationToken);

    public override async Task<IEnumerable<WarehouseAggregate>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().ToListAsync(cancellationToken);

    public async Task<WarehouseAggregate?> FindByLocationAsync(WarehouseLocation location, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(warehouse => warehouse.Location == location, cancellationToken);

    private IQueryable<WarehouseAggregate> Scoped()
    {
        var query = Context.Warehouses.AsQueryable();
        return workspaceContext.TenantId is { } tenantId
            ? query.Where(warehouse => warehouse.TenantId == tenantId)
            : query.Where(_ => false);
    }
}
