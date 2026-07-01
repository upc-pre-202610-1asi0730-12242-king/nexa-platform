using King.Nexa.Platform.Warehouse.Application.CommandServices;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Warehouse.Application.Internal.CommandServices;

public class InventoryOperationsCommandService(
    IInventoryOperationsCommandRepository repository,
    ICatalogItemRepository catalogItemRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : IInventoryOperationsCommandService
{
    public async Task<InventoryLot> CreateLotAsync(InventoryLotDraft draft, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        var item = await ResolveInventoryItemAsync(draft.InventoryItemId, draft.ProductId, tenantId, cancellationToken);
        var warehouse = await ResolveWarehouseAsync(draft.WarehouseId, draft.Warehouse, tenantId, true, cancellationToken);
        if (string.IsNullOrWhiteSpace(draft.LotCode)) throw new InvalidOperationException("Lot code is required.");
        if (draft.Quantity < 0 || draft.ReservedQuantity < 0 || draft.ReservedQuantity > draft.Quantity)
            throw new InvalidOperationException("Lot quantities are invalid.");
        ValidateExpiration(draft.ExpirationDate);

        var lot = new InventoryLot
        {
            TenantId = tenantId,
            InventoryItemId = item.Id,
            WarehouseId = warehouse!.Id,
            LotCode = draft.LotCode.Trim(),
            Quantity = draft.Quantity,
            ReservedQuantity = draft.ReservedQuantity,
            EntryDate = draft.EntryDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = draft.ExpirationDate,
            Zone = draft.Zone?.Trim() ?? string.Empty,
            Status = string.IsNullOrWhiteSpace(draft.Status) ? "active" : draft.Status.Trim().ToLowerInvariant(),
            MinimumTemperature = draft.MinimumTemperature,
            MaximumTemperature = draft.MaximumTemperature
        };
        await repository.AddLotAsync(lot, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return lot;
    }

    public async Task<InventoryLot?> UpdateLotAsync(int id, InventoryLotDraft draft, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        var lot = await repository.FindLotByIdAsync(tenantId, id, cancellationToken);
        if (lot is null) return null;
        var item = await ResolveInventoryItemAsync(draft.InventoryItemId ?? lot.InventoryItemId, draft.ProductId, tenantId, cancellationToken);
        var warehouse = await ResolveWarehouseAsync(draft.WarehouseId ?? lot.WarehouseId, draft.Warehouse, tenantId, true, cancellationToken);
        var quantity = draft.Quantity == 0 ? lot.Quantity : draft.Quantity;
        var reserved = draft.ReservedQuantity == 0 ? lot.ReservedQuantity : draft.ReservedQuantity;
        if (quantity < 0 || reserved < 0 || reserved > quantity) throw new InvalidOperationException("Lot quantities are invalid.");
        lot.InventoryItemId = item.Id;
        lot.WarehouseId = warehouse!.Id;
        lot.LotCode = string.IsNullOrWhiteSpace(draft.LotCode) ? lot.LotCode : draft.LotCode.Trim();
        lot.Quantity = quantity;
        lot.ReservedQuantity = reserved;
        lot.EntryDate = draft.EntryDate ?? lot.EntryDate;
        lot.ExpirationDate = draft.ExpirationDate ?? lot.ExpirationDate;
        lot.Zone = draft.Zone is null ? lot.Zone : draft.Zone.Trim();
        lot.Status = draft.Status is null ? lot.Status : draft.Status.Trim().ToLowerInvariant();
        lot.MinimumTemperature = draft.MinimumTemperature ?? lot.MinimumTemperature;
        lot.MaximumTemperature = draft.MaximumTemperature ?? lot.MaximumTemperature;
        lot.UpdatedAt = DateTime.UtcNow;
        ValidateExpiration(lot.ExpirationDate);
        await unitOfWork.CompleteAsync(cancellationToken);
        return lot;
    }

    public async Task<InventoryMovement> CreateMovementAsync(InventoryMovementDraft draft, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        var item = await ResolveInventoryItemAsync(draft.InventoryItemId, draft.ProductId, tenantId, cancellationToken);
        var type = NormalizeMovementType(draft.MovementType);
        var rawUnits = draft.Quantity;
        if (rawUnits == 0) throw new InvalidOperationException("Movement quantity cannot be zero.");
        var units = Math.Abs(rawUnits);
        var signedUnits = SignedUnits(type, rawUnits);
        ApplyInventoryMovement(item, type, units, signedUnits, draft.Code);
        if (type is "entry" or "exit" or "adjustment")
        {
            var catalogItem = await catalogItemRepository.FindByCatalogItemIdAsync(
                new CatalogItemId(item.CatalogItemId.Value),
                cancellationToken);
            if (catalogItem is null || catalogItem.TenantId != tenantId)
                throw new InvalidOperationException("Catalog stock was not found for the inventory item.");
            catalogItem.SynchronizeAvailableStock(item.AvailableQuantity.Value);
            catalogItemRepository.Update(catalogItem);
        }

        var warehouse = await ResolveWarehouseAsync(draft.WarehouseId, draft.Warehouse, tenantId, false, cancellationToken);
        var lot = await ResolveOrCreateLotAsync(draft, item, warehouse, tenantId, signedUnits, cancellationToken);
        ApplyLotMovement(lot, type, units, signedUnits);
        await unitOfWork.CompleteAsync(cancellationToken);
        var orderId = await ResolveOrderIdAsync(draft.OrderReference, tenantId, cancellationToken);
        var movement = new InventoryMovement
        {
            TenantId = tenantId,
            InventoryItemId = item.Id,
            InventoryLotId = lot?.Id,
            WarehouseId = warehouse?.Id ?? lot?.WarehouseId,
            OrderId = orderId,
            Code = string.IsNullOrWhiteSpace(draft.Code) ? $"STM-{Guid.NewGuid():N}"[..16].ToUpperInvariant() : draft.Code.Trim(),
            MovementType = type,
            Quantity = signedUnits,
            Reason = string.IsNullOrWhiteSpace(draft.Reason) ? type : draft.Reason.Trim(),
            TemperatureReading = draft.TemperatureReading,
            PerformedBy = draft.PerformedBy?.Trim() ?? string.Empty,
            OccurredAt = draft.OccurredAt ?? DateTime.UtcNow
        };
        await repository.AddMovementAsync(movement, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return movement;
    }

    public async Task<InventoryReservationRecord> CreateReservationAsync(InventoryReservationDraft draft, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        var item = await ResolveInventoryItemAsync(draft.InventoryItemId, draft.ProductId, tenantId, cancellationToken);
        if (draft.Units <= 0) throw new InvalidOperationException("Reservation units must be positive.");
        var code = string.IsNullOrWhiteSpace(draft.Code) ? $"RES-{Guid.NewGuid():N}"[..16].ToUpperInvariant() : draft.Code.Trim();
        item.Reserve(new Domain.Model.ValueObjects.InventoryReservation(code, draft.Units));
        var lot = await FindLotAsync(draft.LotCode, tenantId, cancellationToken);
        if (lot is not null && lot.InventoryItemId != item.Id)
            throw new InvalidOperationException("Lot belongs to another inventory item.");
        lot?.Reserve(draft.Units);
        var reservation = new InventoryReservationRecord
        {
            TenantId = tenantId,
            InventoryItemId = item.Id,
            InventoryLotId = lot?.Id,
            OrderId = await ResolveOrderIdAsync(draft.OrderReference, tenantId, cancellationToken),
            PurchaseRequestId = await ResolvePurchaseRequestIdAsync(draft.PurchaseRequestId, tenantId, cancellationToken),
            Code = code,
            Units = draft.Units,
            Status = "reserved"
        };
        await repository.AddReservationAsync(reservation, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return reservation;
    }

    public async Task<InventoryReservationRecord?> ReleaseReservationAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        var reservation = await repository.FindReservationByIdAsync(tenantId, id, cancellationToken);
        return await ReleaseReservationAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<InventoryReservationRecord?> ReleaseReservationAsync(InventoryReservationDraft draft, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        var item = await ResolveInventoryItemAsync(draft.InventoryItemId, draft.ProductId, tenantId, cancellationToken);
        if (string.IsNullOrWhiteSpace(draft.Code)) throw new InvalidOperationException("Reservation code is required.");
        var reservation = await repository.FindActiveReservationByCodeAsync(tenantId, item.Id, draft.Code, cancellationToken);
        if (reservation is null) return null;
        if (draft.Units > 0 && reservation.Units != draft.Units)
            throw new InvalidOperationException("Reservation units do not match the existing reservation.");
        return await ReleaseReservationAsync(tenantId, reservation, cancellationToken);
    }

    private async Task<InventoryReservationRecord?> ReleaseReservationAsync(int tenantId, InventoryReservationRecord? reservation, CancellationToken cancellationToken)
    {
        if (reservation is null) return null;
        if (reservation.Status == "released") return reservation;
        var item = await repository.FindInventoryItemByIdAsync(tenantId, reservation.InventoryItemId, cancellationToken)
                   ?? throw new InvalidOperationException("Inventory item does not belong to the current tenant.");
        item.Release(new Domain.Model.ValueObjects.InventoryReservation(reservation.Code, reservation.Units));
        if (reservation.InventoryLotId.HasValue)
        {
            var lot = await repository.FindLotByIdAsync(tenantId, reservation.InventoryLotId.Value, cancellationToken);
            lot?.Release(reservation.Units);
        }
        reservation.Status = "released";
        reservation.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return reservation;
    }

    private int CurrentTenantId() => workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");

    private async Task<InventoryItem> ResolveInventoryItemAsync(int? id, string? productId, int tenantId, CancellationToken cancellationToken)
    {
        if (id.HasValue)
        {
            return await repository.FindInventoryItemByIdAsync(tenantId, id.Value, cancellationToken)
                ?? throw new InvalidOperationException("Inventory item does not belong to the current tenant.");
        }
        if (string.IsNullOrWhiteSpace(productId)) throw new InvalidOperationException("Product or inventory item is required.");
        var items = await repository.ListInventoryItemsAsync(tenantId, cancellationToken);
        return items.FirstOrDefault(row =>
                   row.ProductId.Value.Equals(productId.Trim(), StringComparison.OrdinalIgnoreCase) ||
                   row.CatalogItemId.Value.Equals(productId.Trim(), StringComparison.OrdinalIgnoreCase))
               ?? throw new InvalidOperationException("Inventory item was not found for the product.");
    }

    private async Task<WarehouseAggregate?> ResolveWarehouseAsync(int? id, string? value, int tenantId, bool required, CancellationToken cancellationToken)
    {
        var warehouses = await repository.ListActiveWarehousesAsync(tenantId, cancellationToken);
        var warehouse = id.HasValue
            ? warehouses.FirstOrDefault(row => row.Id == id.Value)
            : string.IsNullOrWhiteSpace(value)
                ? warehouses.FirstOrDefault()
                : warehouses.FirstOrDefault(row =>
                    row.Name.Value.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    row.Location.Value.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (required && warehouse is null) throw new InvalidOperationException("Warehouse is required and must belong to the current tenant.");
        return warehouse;
    }

    private async Task<InventoryLot?> ResolveOrCreateLotAsync(InventoryMovementDraft draft, InventoryItem item, WarehouseAggregate? warehouse, int tenantId, int signedUnits, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(draft.LotCode)) return null;
        var lot = await FindLotAsync(draft.LotCode, tenantId, cancellationToken);
        if (lot is not null)
        {
            if (lot.InventoryItemId != item.Id) throw new InvalidOperationException("Lot belongs to another inventory item.");
            return lot;
        }
        if (signedUnits <= 0) throw new InvalidOperationException("Inventory lot was not found.");
        warehouse ??= await ResolveWarehouseAsync(null, null, tenantId, true, cancellationToken);
        lot = new InventoryLot
        {
            TenantId = tenantId,
            InventoryItemId = item.Id,
            WarehouseId = warehouse!.Id,
            LotCode = draft.LotCode.Trim(),
            Quantity = 0,
            EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = draft.ExpirationDate,
            Status = "active"
        };
        await repository.AddLotAsync(lot, cancellationToken);
        return lot;
    }

    private Task<InventoryLot?> FindLotAsync(string? code, int tenantId, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(code)
            ? Task.FromResult<InventoryLot?>(null)
            : repository.FindLotByCodeAsync(tenantId, code, cancellationToken);

    private Task<int?> ResolveOrderIdAsync(string? reference, int tenantId, CancellationToken cancellationToken) =>
        repository.FindOrderIdByReferenceAsync(tenantId, reference, cancellationToken);

    private Task<int?> ResolvePurchaseRequestIdAsync(int? id, int tenantId, CancellationToken cancellationToken) =>
        repository.ResolvePurchaseRequestIdAsync(tenantId, id, cancellationToken);

    private static void ApplyInventoryMovement(InventoryItem item, string type, int units, int signedUnits, string? code)
    {
        var reservation = new Domain.Model.ValueObjects.InventoryReservation(code ?? "movement", units);
        switch (type)
        {
            case "entry": item.RegisterMovement("entry", units); break;
            case "exit": item.RegisterMovement("exit", units); break;
            case "adjustment": item.RegisterMovement(signedUnits > 0 ? "adjustment-in" : "adjustment-out", units); break;
            case "reservation": item.Reserve(reservation); break;
            case "reservation_release": item.Release(reservation); break;
            case "review": break;
        }
    }

    private static void ApplyLotMovement(InventoryLot? lot, string type, int units, int signedUnits)
    {
        if (lot is null) return;
        if (type is "entry" or "exit" or "adjustment") lot.RegisterMovement(signedUnits);
        if (type == "reservation") lot.Reserve(units);
        if (type == "reservation_release") lot.Release(units);
    }

    private static string NormalizeMovementType(string value)
    {
        var type = value.Trim().ToLowerInvariant().Replace('-', '_');
        return type switch
        {
            "inbound" or "ingreso" => "entry",
            "outbound" or "salida" => "exit",
            "reserva" => "reservation",
            "release" => "reservation_release",
            "entry" or "exit" or "adjustment" or "reservation" or "reservation_release" or "review" => type,
            _ => throw new InvalidOperationException("Unsupported inventory movement type.")
        };
    }

    private static int SignedUnits(string type, int rawUnits) => type switch
    {
        "entry" or "reservation_release" => Math.Abs(rawUnits),
        "exit" or "reservation" => -Math.Abs(rawUnits),
        "adjustment" => rawUnits,
        "review" => rawUnits,
        _ => rawUnits
    };

    private static void ValidateExpiration(DateOnly? expirationDate)
    {
        if (expirationDate.HasValue && expirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Expired lots cannot be created or activated.");
    }
}
