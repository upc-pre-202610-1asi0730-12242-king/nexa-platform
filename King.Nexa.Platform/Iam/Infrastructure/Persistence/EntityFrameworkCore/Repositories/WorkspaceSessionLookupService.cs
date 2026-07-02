using King.Nexa.Platform.Iam.Application.OutboundServices;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class WorkspaceSessionLookupService(AppDbContext context) : IWorkspaceSessionLookupService
{
    public async Task<WorkspaceSessionLookupResult?> FindActiveSessionAsync(int userId, string workspaceSlug, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeWorkspaceInput(workspaceSlug);
        var workspaceCandidates = await context.Workspaces.AsNoTracking()
            .Where(row => row.Status == "active")
            .Join(
                context.Tenants.AsNoTracking(),
                workspaceRow => workspaceRow.TenantId,
                tenantRow => tenantRow.Id,
                (workspaceRow, tenantRow) => new { Workspace = workspaceRow, Tenant = tenantRow })
            .ToListAsync(cancellationToken);
        var workspace = workspaceCandidates
            .Where(row =>
                row.Workspace.Slug == normalized ||
                row.Tenant.Slug == normalized ||
                NormalizeWorkspaceInput(row.Workspace.Name) == normalized ||
                NormalizeWorkspaceInput(row.Tenant.Name) == normalized ||
                NormalizeWorkspaceInput(row.Tenant.LegalName) == normalized)
            .OrderBy(row => WorkspaceMatchRank(row.Workspace, row.Tenant, normalized))
            .FirstOrDefault();
        if (workspace is null) return null;

        var membership = await context.UserWorkspaceMemberships.AsNoTracking()
            .FirstOrDefaultAsync(row =>
                row.WorkspaceId == workspace.Workspace.Id &&
                row.UserId == userId &&
                row.Status == "active", cancellationToken);
        if (membership is null) return null;

        var tenant = await context.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == workspace.Workspace.TenantId && row.Status == "active", cancellationToken);
        return tenant is null ? null : new WorkspaceSessionLookupResult(tenant, workspace.Workspace, membership);
    }

    private static string NormalizeWorkspaceInput(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant()
                .Replace(".", "-")
                .Replace("_", "-")
                .Replace(" ", "-");

    private static int WorkspaceMatchRank(TenantManagement.Domain.Model.Entities.Workspace workspace, TenantManagement.Domain.Model.Aggregates.Tenant tenant, string normalized)
    {
        if (workspace.Slug == normalized) return 0;
        if (workspace.IsPrimary && tenant.Slug == normalized) return 1;
        if (workspace.IsPrimary && NormalizeWorkspaceInput(tenant.Name) == normalized) return 2;
        if (workspace.IsPrimary && NormalizeWorkspaceInput(tenant.LegalName) == normalized) return 3;
        if (NormalizeWorkspaceInput(workspace.Name) == normalized) return 4;
        return 5;
    }
}
