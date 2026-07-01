using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Entities;

public class InventoryLot : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int InventoryItemId { get; set; }
    public int WarehouseId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public DateOnly EntryDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? ExpirationDate { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public decimal? MinimumTemperature { get; set; }
    public decimal? MaximumTemperature { get; set; }

    public void RegisterMovement(int signedUnits)
    {
        if (signedUnits == 0) throw new InvalidOperationException("Lot movement units cannot be zero.");
        if (Quantity + signedUnits < ReservedQuantity) throw new InvalidOperationException("Lot movement would consume reserved stock.");
        Quantity += signedUnits;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(int units)
    {
        if (units <= 0) throw new InvalidOperationException("Reservation units must be positive.");
        if (ReservedQuantity + units > Quantity) throw new InvalidOperationException("Requested units exceed lot availability.");
        ReservedQuantity += units;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release(int units)
    {
        if (units <= 0 || units > ReservedQuantity) throw new InvalidOperationException("Released units exceed lot reservation.");
        ReservedQuantity -= units;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class InventoryMovement : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int InventoryItemId { get; set; }
    public int? InventoryLotId { get; set; }
    public int? WarehouseId { get; set; }
    public int? OrderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string MovementType { get; set; } = "entry";
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? TemperatureReading { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public class InventoryReservationRecord : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int InventoryItemId { get; set; }
    public int? InventoryLotId { get; set; }
    public int? OrderId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Units { get; set; }
    public string Status { get; set; } = "reserved";
}
