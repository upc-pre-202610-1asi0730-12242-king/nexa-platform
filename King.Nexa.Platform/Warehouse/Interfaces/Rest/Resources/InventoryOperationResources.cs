namespace King.Nexa.Platform.Warehouse.Interfaces.Rest.Resources;

public record InventoryLotResource(
    int BackendId,
    int TenantId,
    int InventoryItemId,
    string ProductId,
    string CatalogItemId,
    int WarehouseId,
    string Warehouse,
    string Id,
    string LotCode,
    int Qty,
    int Reserved,
    DateOnly EntryDate,
    DateOnly? Expiry,
    string Zone,
    string Status,
    decimal? MinimumTemperature,
    decimal? MaximumTemperature,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class UpsertInventoryLotResource
{
    public int? InventoryItemId { get; init; }
    public string? ProductId { get; init; }
    public int? WarehouseId { get; init; }
    public string? Warehouse { get; init; }
    public string? Id { get; init; }
    public string? LotCode { get; init; }
    public int Quantity { get; init; }
    public int? Qty { get; init; }
    public int ReservedQuantity { get; init; }
    public int? Reserved { get; init; }
    public DateOnly? EntryDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public DateOnly? Expiry { get; init; }
    public string? Zone { get; init; }
    public string? Status { get; init; }
    public decimal? MinimumTemperature { get; init; }
    public decimal? MaximumTemperature { get; init; }
}

public record InventoryMovementResource(
    int BackendId,
    int TenantId,
    string Id,
    string Code,
    int InventoryItemId,
    string ProductId,
    string? LotId,
    string? Warehouse,
    string Type,
    int Qty,
    string? OrderId,
    string Reason,
    string Note,
    decimal? TemperatureReading,
    string User,
    DateTime Date,
    DateTime CreatedAt);

public class CreateInventoryMovementResource
{
    public string? Id { get; init; }
    public string? Code { get; init; }
    public int? InventoryItemId { get; init; }
    public string? ProductId { get; init; }
    public int? WarehouseId { get; init; }
    public string? Warehouse { get; init; }
    public string? LotId { get; init; }
    public string? LotNumber { get; init; }
    public string? Type { get; init; }
    public string? MovementType { get; init; }
    public int Quantity { get; init; }
    public int? Qty { get; init; }
    public string? OrderId { get; init; }
    public string? Reference { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
    public string? Notes { get; init; }
    public decimal? TemperatureReading { get; init; }
    public string? User { get; init; }
    public DateTime? OccurredAt { get; init; }
    public DateOnly? ExpirationDate { get; init; }
}

public record InventoryReservationResource(
    int Id,
    int TenantId,
    string Code,
    int InventoryItemId,
    string ProductId,
    string? LotCode,
    string? OrderId,
    int? PurchaseRequestId,
    int Units,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class CreateInventoryReservationResource
{
    public string? Id { get; init; }
    public string? Code { get; init; }
    public int? InventoryItemId { get; init; }
    public string? ProductId { get; init; }
    public string? LotId { get; init; }
    public string? LotCode { get; init; }
    public string? OrderId { get; init; }
    public int? PurchaseRequestId { get; init; }
    public int Units { get; init; }
    public int? Quantity { get; init; }
}
