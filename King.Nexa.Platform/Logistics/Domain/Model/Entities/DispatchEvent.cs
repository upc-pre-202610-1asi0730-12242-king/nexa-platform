using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Model.Entities;

public class DispatchEvent : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int DispatchOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool VisibleToBuyer { get; set; } = true;
}
