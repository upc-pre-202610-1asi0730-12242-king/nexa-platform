using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class InventoryOperationsReadRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IInventoryOperationsReadRepository
{
    public async Task<IEnumerable<InventoryLotReadRecord>> ListLotsAsync(CancellationToken cancellationToken = default)
    {
        var lots = await Scoped(context.InventoryLots.AsNoTracking())
            .OrderBy(row => row.ExpirationDate)
            .ThenBy(row => row.LotCode)
            .ToListAsync(cancellationToken);
        return await LotSnapshotsAsync(lots, cancellationToken);
    }

    public async Task<InventoryLotReadRecord?> FindLotByCodeAsync(string lotCode, CancellationToken cancellationToken = default)
    {
        var lot = await Scoped(context.InventoryLots.AsNoTracking())
            .FirstOrDefaultAsync(row => row.LotCode == lotCode, cancellationToken);
        return lot is null ? null : (await LotSnapshotsAsync([lot], cancellationToken)).Single();
    }

    public async Task<IEnumerable<InventoryMovementReadRecord>> ListMovementsAsync(CancellationToken cancellationToken = default)
    {
        var movements = await Scoped(context.InventoryMovements.AsNoTracking())
            .OrderByDescending(row => row.OccurredAt)
            .ToListAsync(cancellationToken);
        return await MovementSnapshotsAsync(movements, cancellationToken);
    }

    public async Task<InventoryMovementReadRecord?> FindMovementByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var movement = await Scoped(context.InventoryMovements.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Code == code, cancellationToken);
        return movement is null ? null : (await MovementSnapshotsAsync([movement], cancellationToken)).Single();
    }

    public async Task<IEnumerable<InventoryReservationReadRecord>> ListReservationsAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await Scoped(context.InventoryReservations.AsNoTracking())
            .OrderByDescending(row => row.CreatedAt)
            .ToListAsync(cancellationToken);
        return await ReservationSnapshotsAsync(reservations, cancellationToken);
    }

    public async Task<InventoryReservationReadRecord?> FindReservationAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await Scoped(context.InventoryReservations.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        return reservation is null ? null : (await ReservationSnapshotsAsync([reservation], cancellationToken)).Single();
    }

    private async Task<IReadOnlyCollection<InventoryLotReadRecord>> LotSnapshotsAsync(IReadOnlyCollection<InventoryLot> lots, CancellationToken cancellationToken)
    {
        var inventoryItems = await InventoryItemsAsync(lots.Select(row => row.InventoryItemId), cancellationToken);
        var warehouses = await WarehousesAsync(lots.Select(row => row.WarehouseId), cancellationToken);
        return lots.Select(lot =>
        {
            var item = inventoryItems[lot.InventoryItemId];
            return new InventoryLotReadRecord(lot, item.ProductId.Value, item.CatalogItemId.Value, warehouses.GetValueOrDefault(lot.WarehouseId, string.Empty));
        }).ToArray();
    }

    private async Task<IReadOnlyCollection<InventoryMovementReadRecord>> MovementSnapshotsAsync(IReadOnlyCollection<InventoryMovement> movements, CancellationToken cancellationToken)
    {
        var inventoryItems = await InventoryItemsAsync(movements.Select(row => row.InventoryItemId), cancellationToken);
        var lotIds = movements.Where(item => item.InventoryLotId.HasValue).Select(item => item.InventoryLotId!.Value).Distinct().ToList();
        var lots = await context.InventoryLots.AsNoTracking()
            .Where(row => lotIds.Contains(row.Id))
            .ToDictionaryAsync(row => row.Id, row => row.LotCode, cancellationToken);
        var warehouses = await WarehousesAsync(movements.Where(row => row.WarehouseId.HasValue).Select(row => row.WarehouseId!.Value), cancellationToken);
        var orderIds = movements.Where(row => row.OrderId.HasValue).Select(row => row.OrderId!.Value).Distinct().ToList();
        var orders = await context.Orders.AsNoTracking().Where(row => orderIds.Contains(row.Id)).ToDictionaryAsync(row => row.Id, row => row.OrderNumber.Value, cancellationToken);
        return movements.Select(movement => new InventoryMovementReadRecord(
            movement,
            inventoryItems[movement.InventoryItemId].ProductId.Value,
            movement.InventoryLotId.HasValue ? lots.GetValueOrDefault(movement.InventoryLotId.Value) : null,
            movement.WarehouseId.HasValue ? warehouses.GetValueOrDefault(movement.WarehouseId.Value) : null,
            movement.OrderId.HasValue ? orders.GetValueOrDefault(movement.OrderId.Value) : null)).ToArray();
    }

    private async Task<IReadOnlyCollection<InventoryReservationReadRecord>> ReservationSnapshotsAsync(IReadOnlyCollection<InventoryReservationRecord> reservations, CancellationToken cancellationToken)
    {
        var inventoryItems = await InventoryItemsAsync(reservations.Select(row => row.InventoryItemId), cancellationToken);
        var lotIds = reservations.Where(row => row.InventoryLotId.HasValue).Select(row => row.InventoryLotId!.Value).Distinct().ToList();
        var lots = await context.InventoryLots.AsNoTracking().Where(row => lotIds.Contains(row.Id)).ToDictionaryAsync(row => row.Id, row => row.LotCode, cancellationToken);
        var orderIds = reservations.Where(row => row.OrderId.HasValue).Select(row => row.OrderId!.Value).Distinct().ToList();
        var orders = await context.Orders.AsNoTracking().Where(row => orderIds.Contains(row.Id)).ToDictionaryAsync(row => row.Id, row => row.OrderNumber.Value, cancellationToken);
        return reservations.Select(reservation => new InventoryReservationReadRecord(
            reservation,
            inventoryItems[reservation.InventoryItemId].ProductId.Value,
            reservation.InventoryLotId.HasValue ? lots.GetValueOrDefault(reservation.InventoryLotId.Value) : null,
            reservation.OrderId.HasValue ? orders.GetValueOrDefault(reservation.OrderId.Value) : null)).ToArray();
    }

    private async Task<Dictionary<int, Domain.Model.Aggregates.InventoryItem>> InventoryItemsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var values = ids.Distinct().ToList();
        return await context.InventoryItems.AsNoTracking().Where(row => values.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken);
    }

    private async Task<Dictionary<int, string>> WarehousesAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var values = ids.Distinct().ToList();
        return await context.Warehouses.AsNoTracking().Where(row => values.Contains(row.Id)).ToDictionaryAsync(row => row.Id, row => row.Name.Value, cancellationToken);
    }

    private IQueryable<T> Scoped<T>(IQueryable<T> query) where T : class
    {
        if (!workspaceContext.TenantId.HasValue) return query.Where(_ => false);
        var tenantId = workspaceContext.TenantId.Value;
        return query.Where(row => EF.Property<int>(row, "TenantId") == tenantId);
    }
}
