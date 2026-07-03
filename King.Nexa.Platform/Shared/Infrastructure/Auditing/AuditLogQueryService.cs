using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Shared.Infrastructure.Auditing;

public class AuditLogQueryService(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IAuditLogQueryService
{
    public async Task<IReadOnlyCollection<AuditLog>> ListAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        if (workspaceContext.ClientAccountId.HasValue) return [];
        return await context.AuditLogs.AsNoTracking()
            .Where(row => row.TenantId == tenantId)
            .OrderByDescending(row => row.CreatedAt)
            .Take(Math.Clamp(limit, 1, 500))
            .ToListAsync(cancellationToken);
    }

    public Task<AuditLog?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        if (workspaceContext.ClientAccountId.HasValue) return Task.FromResult<AuditLog?>(null);
        return context.AuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(row => row.TenantId == tenantId && row.Id == id, cancellationToken);
    }
}
