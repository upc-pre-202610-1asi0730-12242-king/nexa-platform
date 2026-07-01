using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Model.Entities;

public class TemperatureLog : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int? DispatchOrderId { get; set; }
    public int? OrderId { get; set; }
    public decimal Celsius { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Status { get; set; } = "ok";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
