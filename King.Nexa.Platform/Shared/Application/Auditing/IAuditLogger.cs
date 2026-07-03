namespace King.Nexa.Platform.Shared.Application.Auditing;

public interface IAuditLogger
{
    Task RecordAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

public record AuditLogEntry(
    string Action,
    string ResourceType,
    string ResourceId,
    string? MetadataJson = null,
    int? TenantId = null,
    int? WorkspaceId = null);
