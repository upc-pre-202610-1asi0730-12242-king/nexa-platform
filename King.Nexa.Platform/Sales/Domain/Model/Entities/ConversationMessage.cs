using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class ConversationMessage : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int? ClientAccountId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public int? OrderId { get; set; }
    public string SenderRole { get; set; } = "commercial";
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool VisibleToBuyer { get; set; } = true;
}
