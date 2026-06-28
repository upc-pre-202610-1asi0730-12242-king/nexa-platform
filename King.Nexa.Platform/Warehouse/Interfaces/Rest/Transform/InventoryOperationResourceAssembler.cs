using King.Nexa.Platform.Warehouse.Application.CommandServices;
using King.Nexa.Platform.Warehouse.Application.QueryServices;
using King.Nexa.Platform.Warehouse.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Warehouse.Interfaces.Rest.Transform;

public static class InventoryOperationResourceAssembler
{
    public static InventoryLotResource ToResourceFromSnapshot(InventoryLotSnapshot snapshot)
    {
        var lot = snapshot.Lot;
        return new InventoryLotResource(
            lot.Id, lot.TenantId, lot.InventoryItemId, snapshot.ProductId, snapshot.CatalogItemId,
            lot.WarehouseId, snapshot.Warehouse, lot.LotCode, lot.LotCode, lot.Quantity,
            lot.ReservedQuantity, lot.EntryDate, lot.ExpirationDate, lot.Zone, lot.Status,
            lot.MinimumTemperature, lot.MaximumTemperature, lot.CreatedAt, lot.UpdatedAt);
    }

    public static InventoryLotDraft ToDraftFromResource(UpsertInventoryLotResource resource) =>
        new(
            resource.InventoryItemId,
            resource.ProductId,
            resource.WarehouseId,
            resource.Warehouse,
            resource.LotCode ?? resource.Id ?? string.Empty,
            resource.Qty ?? resource.Quantity,
            resource.Reserved ?? resource.ReservedQuantity,
            resource.EntryDate,
            resource.Expiry ?? resource.ExpirationDate,
            resource.Zone,
            resource.Status,
            resource.MinimumTemperature,
            resource.MaximumTemperature);

    public static InventoryMovementResource ToResourceFromSnapshot(InventoryMovementSnapshot snapshot)
    {
        var movement = snapshot.Movement;
        return new InventoryMovementResource(
            movement.Id, movement.TenantId, movement.Code, movement.Code, movement.InventoryItemId,
            snapshot.ProductId, snapshot.LotCode, snapshot.Warehouse, movement.MovementType,
            movement.Quantity, snapshot.OrderNumber, movement.Reason, movement.Reason,
            movement.TemperatureReading, movement.PerformedBy, movement.OccurredAt, movement.CreatedAt);
    }

    public static InventoryMovementDraft ToDraftFromResource(CreateInventoryMovementResource resource) =>
        new(
            resource.Code ?? resource.Id,
            resource.InventoryItemId,
            resource.ProductId,
            resource.WarehouseId,
            resource.Warehouse,
            resource.LotNumber ?? resource.LotId,
            resource.MovementType ?? resource.Type ?? string.Empty,
            resource.Qty ?? resource.Quantity,
            resource.OrderId ?? resource.Reference,
            resource.Reason ?? resource.Notes ?? resource.Note,
            resource.TemperatureReading,
            resource.User,
            resource.OccurredAt,
            resource.ExpirationDate);

    public static InventoryReservationResource ToResourceFromSnapshot(InventoryReservationSnapshot snapshot)
    {
        var reservation = snapshot.Reservation;
        return new InventoryReservationResource(
            reservation.Id, reservation.TenantId, reservation.Code, reservation.InventoryItemId,
            snapshot.ProductId, snapshot.LotCode, snapshot.OrderNumber, reservation.PurchaseRequestId, reservation.Units,
            reservation.Status, reservation.CreatedAt, reservation.UpdatedAt);
    }

    public static InventoryReservationDraft ToDraftFromResource(CreateInventoryReservationResource resource) =>
        new(
            resource.Code ?? resource.Id,
            resource.InventoryItemId,
            resource.ProductId,
            resource.LotCode ?? resource.LotId,
            resource.OrderId,
            resource.PurchaseRequestId,
            resource.Quantity ?? resource.Units);
}

