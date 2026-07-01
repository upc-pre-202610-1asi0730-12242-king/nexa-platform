using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

public class OrganizationRegistrationRequest : AuditableEntity
{
    public string ExternalId { get; set; } = string.Empty;
    public string Status { get; set; } = "pending_review";
    public string CompanyName { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string WorkspaceSlug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
