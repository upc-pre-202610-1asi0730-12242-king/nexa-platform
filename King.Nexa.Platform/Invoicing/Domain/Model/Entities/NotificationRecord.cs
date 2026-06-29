using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Entities;

public class NotificationRecord : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int? ClientAccountId { get; set; }
    public string RecipientRole { get; set; } = "buyer";
    public string Type { get; set; } = "status";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool Read { get; set; }
}

