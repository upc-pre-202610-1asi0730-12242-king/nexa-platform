using King.Nexa.Platform.Warehouse.Domain.Model.Entities;

namespace King.Nexa.Platform.Warehouse.Application.QueryServices;

public interface IInventoryOperationsQueryService
{
    Task<IEnumerable<InventoryLotSnapshot>> ListLotsAsync(CancellationToken cancellationToken = default);
    Task<InventoryLotSnapshot?> GetLotByCodeAsync(string lotCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryMovementSnapshot>> ListMovementsAsync(CancellationToken cancellationToken = default);
    Task<InventoryMovementSnapshot?> GetMovementByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryReservationSnapshot>> ListReservationsAsync(CancellationToken cancellationToken = default);
    Task<InventoryReservationSnapshot?> GetReservationAsync(int id, CancellationToken cancellationToken = default);
}

public record InventoryLotSnapshot(InventoryLot Lot, string ProductId, string CatalogItemId, string Warehouse);
public record InventoryMovementSnapshot(InventoryMovement Movement, string ProductId, string? LotCode, string? Warehouse, string? OrderNumber);
public record InventoryReservationSnapshot(InventoryReservationRecord Reservation, string ProductId, string? LotCode, string? OrderNumber);
