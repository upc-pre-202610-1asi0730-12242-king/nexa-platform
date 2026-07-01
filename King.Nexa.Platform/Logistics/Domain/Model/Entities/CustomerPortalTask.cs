using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Model.Entities;

// Retained only to keep the historical database model migration-compatible.
public class CustomerPortalTask : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int ClientAccountId { get; set; }
    public string PortalName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string UploadChannel { get; set; } = "manual";
    public string RequiredDocuments { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string Owner { get; set; } = string.Empty;
}
