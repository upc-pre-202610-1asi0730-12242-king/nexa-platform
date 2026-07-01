using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Warehouse.Domain.Repositories;

public interface IInventoryOperationsCommandRepository
{
    Task<InventoryItem?> FindInventoryItemByIdAsync(int tenantId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryItem>> ListInventoryItemsAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WarehouseAggregate>> ListActiveWarehousesAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<InventoryLot?> FindLotByIdAsync(int tenantId, int id, CancellationToken cancellationToken = default);
    Task<InventoryLot?> FindLotByCodeAsync(int tenantId, string lotCode, CancellationToken cancellationToken = default);
    Task AddLotAsync(InventoryLot lot, CancellationToken cancellationToken = default);
    Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task<InventoryReservationRecord?> FindReservationByIdAsync(int tenantId, int id, CancellationToken cancellationToken = default);
    Task AddReservationAsync(InventoryReservationRecord reservation, CancellationToken cancellationToken = default);
    Task<int?> FindOrderIdByReferenceAsync(int tenantId, string? reference, CancellationToken cancellationToken = default);
    Task<int?> ResolvePurchaseRequestIdAsync(int tenantId, int? id, CancellationToken cancellationToken = default);
}
