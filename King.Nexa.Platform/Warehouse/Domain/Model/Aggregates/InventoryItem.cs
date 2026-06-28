using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Model.Commands;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root for stock availability and reservation state.
/// </summary>
public class InventoryItem : AuditableEntity, ITenantScoped
{
    protected InventoryItem()
    {
        ProductId = null!;
        CatalogItemId = null!;
        AvailableQuantity = null!;
        ReservedQuantity = null!;
        WarehouseLocation = null!;
        TemperatureRange = null!;
    }

    public InventoryItem(CreateInventoryItemCommand command)
    {
        ProductId = command.ProductId;
        CatalogItemId = command.CatalogItemId;
        AvailableQuantity = command.AvailableQuantity;
        ReservedQuantity = new StockQuantity(0);
        WarehouseLocation = command.WarehouseLocation;
        TemperatureRange = command.TemperatureRange;
    }

    public ProductId ProductId { get; private set; }

    public int TenantId { get; private set; }

    public CatalogItemId CatalogItemId { get; private set; }

    public StockQuantity AvailableQuantity { get; private set; }

    public StockQuantity ReservedQuantity { get; private set; }

    public WarehouseLocation WarehouseLocation { get; private set; }

    public TemperatureRange TemperatureRange { get; private set; }

    public void AssignTenant(int tenantId)
    {
        if (tenantId <= 0) throw new ArgumentException("Tenant id must be positive.", nameof(tenantId));
        TenantId = tenantId;
    }

    public void Update(UpdateInventoryItemCommand command)
    {
        ProductId = command.ProductId;
        CatalogItemId = command.CatalogItemId;
        AvailableQuantity = command.AvailableQuantity;
        WarehouseLocation = command.WarehouseLocation;
        TemperatureRange = command.TemperatureRange;
    }

    public void Reserve(InventoryReservation reservation)
    {
        if (reservation.Units > AvailableQuantity.Value)
            throw new InvalidOperationException("Requested units exceed available stock.");

        AvailableQuantity = new StockQuantity(AvailableQuantity.Value - reservation.Units);
        ReservedQuantity = new StockQuantity(ReservedQuantity.Value + reservation.Units);
    }

    public void Release(InventoryReservation reservation)
    {
        if (reservation.Units > ReservedQuantity.Value)
            throw new InvalidOperationException("Released units exceed reserved stock.");

        AvailableQuantity = new StockQuantity(AvailableQuantity.Value + reservation.Units);
        ReservedQuantity = new StockQuantity(ReservedQuantity.Value - reservation.Units);
    }

    public void RegisterMovement(string movementType, int units)
    {
        if (string.IsNullOrWhiteSpace(movementType))
            throw new ArgumentException("Movement type is required.", nameof(movementType));
        if (units <= 0)
            throw new ArgumentOutOfRangeException(nameof(units), "Movement units must be positive.");

        var normalizedType = movementType.Trim().ToLowerInvariant();
        if (normalizedType is "inbound" or "entry" or "adjustment-in")
        {
            AvailableQuantity = new StockQuantity(AvailableQuantity.Value + units);
            return;
        }

        if (normalizedType is "outbound" or "exit" or "adjustment-out")
        {
            if (units > AvailableQuantity.Value)
                throw new InvalidOperationException("Movement units exceed available stock.");

            AvailableQuantity = new StockQuantity(AvailableQuantity.Value - units);
            return;
        }

        throw new InvalidOperationException("Unsupported inventory movement type.");
    }
}

