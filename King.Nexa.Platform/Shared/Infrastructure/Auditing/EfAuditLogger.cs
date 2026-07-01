using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace King.Nexa.Platform.Shared.Infrastructure.Auditing;

public class EfAuditLogger(
    AppDbContext context,
    ICurrentUserContext userContext,
    ICurrentWorkspaceContext workspaceContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EfAuditLogger> logger) : IAuditLogger
{
    public async Task RecordAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = entry.TenantId ?? workspaceContext.TenantId;
            if (tenantId is null or <= 0)
            {
                logger.LogWarning("Audit entry skipped because tenant scope is missing. Action {Action}, Resource {ResourceType}:{ResourceId}",
                    entry.Action,
                    entry.ResourceType,
                    entry.ResourceId);
                return;
            }

            var httpContext = httpContextAccessor.HttpContext;
            await context.AuditLogs.AddAsync(new AuditLog
            {
                TenantId = tenantId.Value,
                WorkspaceId = entry.WorkspaceId ?? workspaceContext.WorkspaceId,
                ActorUserId = userContext.UserId ?? 0,
                ActorMembershipId = workspaceContext.MembershipId,
                Action = entry.Action,
                ResourceType = entry.ResourceType,
                ResourceId = entry.ResourceId,
                MetadataJson = entry.MetadataJson,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Audit entry failed. Action {Action}, Resource {ResourceType}:{ResourceId}",
                entry.Action,
                entry.ResourceType,
                entry.ResourceId);
        }
    }
}
