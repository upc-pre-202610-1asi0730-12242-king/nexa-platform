using King.Nexa.Platform.Warehouse.Domain.Model.Entities;

namespace King.Nexa.Platform.Warehouse.Domain.Repositories;

public interface IInventoryOperationsReadRepository
{
    Task<IEnumerable<InventoryLotReadRecord>> ListLotsAsync(CancellationToken cancellationToken = default);
    Task<InventoryLotReadRecord?> FindLotByCodeAsync(string lotCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovementReadRecord>> ListMovementsAsync(CancellationToken cancellationToken = default);
    Task<InventoryMovementReadRecord?> FindMovementByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryReservationReadRecord>> ListReservationsAsync(CancellationToken cancellationToken = default);
    Task<InventoryReservationReadRecord?> FindReservationAsync(int id, CancellationToken cancellationToken = default);
}

public record InventoryLotReadRecord(InventoryLot Lot, string ProductId, string CatalogItemId, string Warehouse);

public record InventoryMovementReadRecord(InventoryMovement Movement, string ProductId, string? LotCode, string? Warehouse, string? OrderNumber);

public record InventoryReservationReadRecord(InventoryReservationRecord Reservation, string ProductId, string? LotCode, string? OrderNumber);

