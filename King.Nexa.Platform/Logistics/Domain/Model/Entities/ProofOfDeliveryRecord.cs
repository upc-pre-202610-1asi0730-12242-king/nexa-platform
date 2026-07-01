using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Model.Entities;

public class ProofOfDeliveryRecord : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int DispatchOrderId { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public bool PhotoReference { get; set; }
    public bool SignatureReference { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
}
