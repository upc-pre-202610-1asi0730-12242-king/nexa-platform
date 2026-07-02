using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Model.Events;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Entities;

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

    public void RecordCreation() => AddDomainEvent(new InventoryReservationCreated(Code, TenantId, InventoryItemId, Units));
}
