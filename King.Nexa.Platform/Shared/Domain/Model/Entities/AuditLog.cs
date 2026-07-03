namespace King.Nexa.Platform.Shared.Domain.Model.Entities;

public class AuditLog : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }

    public int? WorkspaceId { get; set; }

    public int ActorUserId { get; set; }

    public int? ActorMembershipId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public string ResourceId { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}
