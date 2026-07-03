using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Entities;

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
