using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Entities;

public class PaymentMethodRecord : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int ClientAccountId { get; set; }
    public string Type { get; set; } = "transfer";
    public string Label { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public bool IsDefault { get; set; }
}
