using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Repositories;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class InventoryOperationsCommandRepository(AppDbContext context) : IInventoryOperationsCommandRepository
{
    public Task<InventoryItem?> FindInventoryItemByIdAsync(int tenantId, int id, CancellationToken cancellationToken = default) =>
        context.InventoryItems.FirstOrDefaultAsync(row => row.TenantId == tenantId && row.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<InventoryItem>> ListInventoryItemsAsync(int tenantId, CancellationToken cancellationToken = default) =>
        await context.InventoryItems.Where(row => row.TenantId == tenantId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<WarehouseAggregate>> ListActiveWarehousesAsync(int tenantId, CancellationToken cancellationToken = default) =>
        await context.Warehouses.Where(row => row.TenantId == tenantId && row.IsActive).ToListAsync(cancellationToken);

    public Task<InventoryLot?> FindLotByIdAsync(int tenantId, int id, CancellationToken cancellationToken = default) =>
        context.InventoryLots.FirstOrDefaultAsync(row => row.TenantId == tenantId && row.Id == id, cancellationToken);

    public Task<InventoryLot?> FindLotByCodeAsync(int tenantId, string lotCode, CancellationToken cancellationToken = default) =>
        context.InventoryLots.FirstOrDefaultAsync(row => row.TenantId == tenantId && row.LotCode == lotCode.Trim(), cancellationToken);

    public Task AddLotAsync(InventoryLot lot, CancellationToken cancellationToken = default) =>
        context.InventoryLots.AddAsync(lot, cancellationToken).AsTask();

    public Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default) =>
        context.InventoryMovements.AddAsync(movement, cancellationToken).AsTask();

    public Task<InventoryReservationRecord?> FindReservationByIdAsync(int tenantId, int id, CancellationToken cancellationToken = default) =>
        context.InventoryReservations.FirstOrDefaultAsync(row => row.TenantId == tenantId && row.Id == id, cancellationToken);

    public Task<InventoryReservationRecord?> FindActiveReservationByCodeAsync(int tenantId, int inventoryItemId, string code, CancellationToken cancellationToken = default)
    {
        var value = code.Trim();
        return context.InventoryReservations.FirstOrDefaultAsync(
            row => row.TenantId == tenantId &&
                   row.InventoryItemId == inventoryItemId &&
                   row.Code == value &&
                   row.Status != "released",
            cancellationToken);
    }

    public Task AddReservationAsync(InventoryReservationRecord reservation, CancellationToken cancellationToken = default) =>
        context.InventoryReservations.AddAsync(reservation, cancellationToken).AsTask();

    public async Task<int?> FindOrderIdByReferenceAsync(int tenantId, string? reference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reference)) return null;
        if (int.TryParse(reference, out var id) &&
            await context.Orders.AnyAsync(row => row.TenantId == tenantId && row.Id == id, cancellationToken))
            return id;

        var value = reference.Trim();
        return await context.Orders
            .Where(row => row.TenantId == tenantId && row.OrderNumber == new OrderNumber(value))
            .Select(row => (int?)row.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> ResolvePurchaseRequestIdAsync(int tenantId, int? id, CancellationToken cancellationToken = default)
    {
        if (!id.HasValue) return null;
        return await context.PurchaseRequests.AnyAsync(row => row.TenantId == tenantId && row.Id == id.Value, cancellationToken)
            ? id
            : throw new InvalidOperationException("Purchase request does not belong to the current tenant.");
    }
}
