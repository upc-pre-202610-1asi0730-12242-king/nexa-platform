using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Model.Events;

namespace King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

public class Workspace : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string EmailDomain { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public bool IsPrimary { get; set; } = true;

    public void RecordCreation() => AddDomainEvent(new WorkspaceCreated(Slug, TenantId));

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Workspace name is required.", nameof(name));
        Name = name.Trim();
    }
}

public class UserWorkspaceMembership : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int WorkspaceId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public bool PortalAccess { get; set; }
    public int? ClientAccountId { get; set; }
}

public class WorkspacePreference : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int WorkspaceId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = "string";
}
