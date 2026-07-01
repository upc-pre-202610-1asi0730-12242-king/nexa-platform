using King.Nexa.Platform.Warehouse.Domain.Model.Entities;

namespace King.Nexa.Platform.Warehouse.Application.CommandServices;

public interface IInventoryOperationsCommandService
{
    Task<InventoryLot> CreateLotAsync(InventoryLotDraft draft, CancellationToken cancellationToken = default);
    Task<InventoryLot?> UpdateLotAsync(int id, InventoryLotDraft draft, CancellationToken cancellationToken = default);
    Task<InventoryMovement> CreateMovementAsync(InventoryMovementDraft draft, CancellationToken cancellationToken = default);
    Task<InventoryReservationRecord> CreateReservationAsync(InventoryReservationDraft draft, CancellationToken cancellationToken = default);
    Task<InventoryReservationRecord?> ReleaseReservationAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryReservationRecord?> ReleaseReservationAsync(InventoryReservationDraft draft, CancellationToken cancellationToken = default);
}

public record InventoryLotDraft(
    int? InventoryItemId,
    string? ProductId,
    int? WarehouseId,
    string? Warehouse,
    string LotCode,
    int Quantity,
    int ReservedQuantity,
    DateOnly? EntryDate,
    DateOnly? ExpirationDate,
    string? Zone,
    string? Status,
    decimal? MinimumTemperature,
    decimal? MaximumTemperature);

public record InventoryMovementDraft(
    string? Code,
    int? InventoryItemId,
    string? ProductId,
    int? WarehouseId,
    string? Warehouse,
    string? LotCode,
    string MovementType,
    int Quantity,
    string? OrderReference,
    string? Reason,
    decimal? TemperatureReading,
    string? PerformedBy,
    DateTime? OccurredAt,
    DateOnly? ExpirationDate);

public record InventoryReservationDraft(
    string? Code,
    int? InventoryItemId,
    string? ProductId,
    string? LotCode,
    string? OrderReference,
    int? PurchaseRequestId,
    int Units);
